// Turns raw logs into answers: am I getting stronger, am I consistent,
// is my training balanced, and what needs attention.

import type { LoggedSet, LoggedWorkout } from "../../assets/js/data/apiTypes.ts";
import type { Exercise } from "../exercises/api.ts";
import { muscleShares } from "../exercises/muscles.ts";
import { e1rm, round1 } from "../../utils/bodyMath.ts";

// ── Periods & time buckets ────────────────────────────────────────────────────

export type Period = "4w" | "12w" | "6m" | "1y" | "all";

/** `short` is the phone label, where all five have to fit on one row. */
export const PERIODS: { value: Period; label: string; short: string; days: number | null }[] = [
    { value: "4w", label: "4 weeks", short: "4W", days: 28 },
    { value: "12w", label: "12 weeks", short: "12W", days: 84 },
    { value: "6m", label: "6 months", short: "6M", days: 182 },
    { value: "1y", label: "1 year", short: "1Y", days: 365 },
    { value: "all", label: "All time", short: "All", days: null },
];

const DAY = 86_400_000;

export interface Range { start: Date; end: Date; days: number }

export function periodRange(period: Period, logs: LoggedWorkout[], now = new Date()): Range {
    const end = now;
    const days = PERIODS.find(p => p.value === period)!.days;
    if (days !== null) return { start: new Date(end.getTime() - days * DAY), end, days };
    const first = logs.reduce((min, l) => Math.min(min, new Date(l.loggedAt).getTime()), end.getTime());
    return { start: new Date(first), end, days: Math.max(7, Math.round((end.getTime() - first) / DAY)) };
}

/** The same-length window right before `range`, for "vs previous period" comparisons. */
export function previousRange(range: Range): Range {
    return { start: new Date(range.start.getTime() - range.days * DAY), end: range.start, days: range.days };
}

export const inRange = (iso: string, r: Range) => {
    const t = new Date(iso).getTime();
    return t >= r.start.getTime() && t <= r.end.getTime();
};

/**
 * Picks the shortest period that actually has some training in it, so the page
 * never opens on an empty chart after a break.
 */
export function suggestPeriod(logs: LoggedWorkout[], now = new Date()): Period {
    for (const p of ["12w", "6m", "1y"] as Period[]) {
        const r = periodRange(p, logs, now);
        if (logs.filter(l => inRange(l.loggedAt, r)).length >= 4) return p;
    }
    return "all";
}

export interface Bucket { key: string; start: Date; end: Date; label: string; longLabel: string }

const startOfWeek = (d: Date) => {
    const x = new Date(d.getFullYear(), d.getMonth(), d.getDate());
    x.setDate(x.getDate() - ((x.getDay() + 6) % 7)); // Monday
    return x;
};

/** Weekly buckets for spans up to ~6 months, monthly beyond that. Every bucket is present, even empty ones. */
export function buckets(range: Range): { unit: "week" | "month"; items: Bucket[] } {
    const unit = range.days <= 190 ? "week" : "month";
    const items: Bucket[] = [];
    if (unit === "week") {
        for (let s = startOfWeek(range.start); s <= range.end; s = new Date(s.getTime() + 7 * DAY)) {
            const e = new Date(s.getTime() + 7 * DAY - 1);
            items.push({
                key: s.toISOString().slice(0, 10),
                start: s, end: e,
                label: s.toLocaleDateString("en-GB", { day: "numeric", month: "short" }),
                longLabel: `Week of ${s.toLocaleDateString("en-GB", { day: "numeric", month: "long" })}`,
            });
        }
    } else {
        for (let s = new Date(range.start.getFullYear(), range.start.getMonth(), 1); s <= range.end; s = new Date(s.getFullYear(), s.getMonth() + 1, 1)) {
            const e = new Date(s.getFullYear(), s.getMonth() + 1, 1, 0, 0, 0, -1);
            items.push({
                key: `${s.getFullYear()}-${s.getMonth() + 1}`,
                start: s, end: e,
                label: s.toLocaleDateString("en-GB", { month: "short", ...(s.getMonth() === 0 ? { year: "2-digit" } : {}) }),
                longLabel: s.toLocaleDateString("en-GB", { month: "long", year: "numeric" }),
            });
        }
    }
    return { unit, items };
}

// ── Set maths ─────────────────────────────────────────────────────────────────

export const toKg = (w: number | null, unit: string | null) =>
    w == null ? 0 : (unit ?? "kg").toLowerCase() === "lbs" ? w * 0.45359237 : w;

export const toKm = (d: number | null, unit: string | null) => {
    if (d == null) return 0;
    const u = (unit ?? "km").toLowerCase();
    return u === "miles" ? d * 1.609344 : u === "meters" ? d / 1000 : d;
};

export const toMinutes = (dur: string | null) => {
    if (!dur) return 0;
    const [h, m, s] = dur.split(":").map(Number);
    return (h || 0) * 60 + (m || 0) + (s || 0) / 60;
};

export const isWorkingStrengthSet = (s: LoggedSet) => s.type !== "Cardio" && s.marker !== "WarmUp";


// ── Strength: per-exercise progress ───────────────────────────────────────────

export interface ExerciseSession {
    logId: string;
    date: string;
    e1rm: number;
    /** Set with the highest estimated 1RM. */
    topSet: { weight: number; reps: number };
    heaviest: number;
    /** Reps done with the heaviest weight (most reps if that weight was used more than once). */
    heaviestReps: number;
    bestSetVolume: number;
    workingSets: number;
    volumeKg: number;
    /** New best estimated 1RM. */
    isPr: boolean;
    /** Heavier set than any earlier session. */
    isWeightPr: boolean;
}

export interface ExerciseProgress {
    id: string;
    name: string;
    muscles: string[];
    /** Every session ever, oldest first (PR flags need the full history). */
    allSessions: ExerciseSession[];
    sessions: ExerciseSession[];
    start: number | null;
    current: number | null;
    changePct: number | null;
    bestE1rm: number;
    bestSet: { weight: number; reps: number; date: string } | null;
    daysSinceImprovement: number | null;
    prsInPeriod: number;
}

export function exerciseProgress(
    logs: LoggedWorkout[], exercises: Record<string, Exercise>, range: Range,
): ExerciseProgress[] {
    const byExercise = new Map<string, ExerciseSession[]>();
    const sorted = [...logs].sort((a, b) => new Date(a.loggedAt).getTime() - new Date(b.loggedAt).getTime());
    for (const log of sorted) {
        for (const ex of log.exercises) {
            const sets = ex.sets.filter(s => isWorkingStrengthSet(s) && (s.weight ?? 0) > 0 && (s.reps ?? 0) > 0);
            if (!sets.length) continue;
            let best = 0, top = { weight: 0, reps: 0 }, volume = 0, heaviest = 0, heaviestReps = 0, bestSetVolume = 0;
            for (const s of sets) {
                const kg = toKg(s.weight, s.weightUnit);
                const est = e1rm(kg, s.reps!, s.rir); // reps in reserve count as reps left
                volume += kg * s.reps!;
                if (kg > heaviest || (kg === heaviest && s.reps! > heaviestReps)) { heaviest = kg; heaviestReps = s.reps!; }
                bestSetVolume = Math.max(bestSetVolume, kg * s.reps!);
                if (est > best) { best = est; top = { weight: round1(kg), reps: s.reps! }; }
            }
            const list = byExercise.get(ex.exerciseId) ?? [];
            list.push({
                logId: log.id, date: log.loggedAt, e1rm: round1(best), topSet: top,
                heaviest: round1(heaviest), heaviestReps, bestSetVolume: Math.round(bestSetVolume),
                workingSets: sets.length, volumeKg: Math.round(volume), isPr: false, isWeightPr: false,
            });
            byExercise.set(ex.exerciseId, list);
        }
    }

    const result: ExerciseProgress[] = [];
    for (const [id, all] of byExercise) {
        const info = exercises[id];
        // Progress is measured by heaviest weight: the number people recognise
        // from the gym. Estimated 1RM PRs are still flagged for that chart.
        let bestE1rm = 0, bestWeight = 0;
        let lastImprovement: string | null = null;
        for (let i = 0; i < all.length; i++) {
            const s = all[i];
            if (s.e1rm > bestE1rm) { s.isPr = i > 0; bestE1rm = s.e1rm; } // the first session is the baseline
            if (s.heaviest > bestWeight) {
                s.isWeightPr = i > 0;
                bestWeight = s.heaviest;
                lastImprovement = s.date;
            }
        }
        const sessions = all.filter(s => inRange(s.date, range));
        if (!sessions.length) continue;
        const before = all.filter(s => new Date(s.date) < range.start);
        // Start = best before the period (or the first session in it); current = best of the last two sessions.
        // Start = heaviest before the period (or the first session in it); current = heaviest of the last two sessions.
        const start = before.length ? Math.max(...before.map(s => s.heaviest)) : sessions[0].heaviest;
        const current = Math.max(...sessions.slice(-2).map(s => s.heaviest));
        const bestSession = sessions.reduce((a, b) => (b.topSet.weight > a.topSet.weight ? b : a));
        result.push({
            id,
            name: info?.name ?? "Unknown exercise",
            muscles: info?.targetMuscles ?? [],
            allSessions: all,
            sessions,
            start,
            current,
            changePct: sessions.length + before.length >= 2 && start > 0 ? Math.round(((current - start) / start) * 100) : null,
            bestE1rm: Math.max(...sessions.map(s => s.e1rm)),
            bestSet: { ...bestSession.topSet, date: bestSession.date },
            daysSinceImprovement: lastImprovement
                ? Math.floor((range.end.getTime() - new Date(lastImprovement).getTime()) / DAY)
                : null,
            prsInPeriod: sessions.filter(s => s.isWeightPr).length,
        });
    }
    // Most-trained first: that's what people look for.
    return result.sort((a, b) => b.sessions.length - a.sessions.length || a.name.localeCompare(b.name));
}

// ── Consistency ───────────────────────────────────────────────────────────────

export interface ConsistencyPoint { key: string; label: string; longLabel: string; strength: number; cardio: number }

export function consistency(logs: LoggedWorkout[], range: Range) {
    const { unit, items } = buckets(range);
    const points: ConsistencyPoint[] = items.map(b => ({ key: b.key, label: b.label, longLabel: b.longLabel, strength: 0, cardio: 0 }));
    for (const l of logs) {
        const t = new Date(l.loggedAt).getTime();
        const i = items.findIndex(b => t >= b.start.getTime() && t <= b.end.getTime());
        if (i < 0) continue;
        const sets = l.exercises.flatMap(e => e.sets);
        if (sets.some(s => s.type !== "Cardio")) points[i].strength++;
        else if (sets.length) points[i].cardio++;
    }
    const total = points.reduce((n, p) => n + p.strength + p.cardio, 0);
    const weeks = range.days / 7;
    return { unit, points, total, perWeek: round1(total / weeks) };
}

// ── Muscle balance ────────────────────────────────────────────────────────────

export interface MuscleVolume {
    muscle: string;
    setsPerWeek: number;
    lastTrained: string | null;
    /** Some exercise worked it as a primary muscle, not only as a helper. */
    direct: boolean;
}

/** Working sets per week for each muscle: a full set for each primary muscle, half a set for each secondary one. */
export function muscleBalance(logs: LoggedWorkout[], exercises: Record<string, Exercise>, range: Range): MuscleVolume[] {
    const sets = new Map<string, number>();
    const last = new Map<string, string>();
    const direct = new Set<string>();
    for (const l of logs) {
        if (!inRange(l.loggedAt, range)) continue;
        for (const ex of l.exercises) {
            const n = ex.sets.filter(isWorkingStrengthSet).length;
            if (!n) continue;
            for (const [m, share] of muscleShares(exercises[ex.exerciseId])) {
                sets.set(m, (sets.get(m) ?? 0) + n * share);
                if (share === 1) direct.add(m);
                if (!last.has(m) || l.loggedAt > last.get(m)!) last.set(m, l.loggedAt);
            }
        }
    }
    // Average over the weeks you were actually training in this period, so a
    // long break doesn't make every muscle look under-trained.
    const dates = logs.filter(l => inRange(l.loggedAt, range)).map(l => new Date(l.loggedAt).getTime());
    const spanDays = dates.length ? (Math.max(...dates) - Math.min(...dates)) / DAY + 7 : range.days;
    const weeks = Math.max(1, Math.min(range.days, spanDays) / 7);
    return [...sets.entries()]
        .map(([muscle, n]) => ({ muscle, setsPerWeek: round1(n / weeks), lastTrained: last.get(muscle) ?? null, direct: direct.has(muscle) }))
        .sort((a, b) => b.setsPerWeek - a.setsPerWeek);
}

/** Commonly cited range of weekly working sets per muscle for growth. */
export const SET_RANGE = { low: 10, high: 20 };

// ── Highlights ────────────────────────────────────────────────────────────────

export type Tone = "good" | "attention" | "neutral";

export interface Highlight {
    id: string;
    tone: Tone;
    kind: "consistency" | "progress" | "stalled" | "muscle" | "prs" | "distance" | "longest" | "pace";
    title: string;
    detail: string;
    exerciseId?: string;
}

export function strengthHighlights(
    progress: ExerciseProgress[], balance: MuscleVolume[],
    perWeek: number, goalPerWeek: number | null, range: Range,
): Highlight[] {
    const out: Highlight[] = [];

    if (goalPerWeek && goalPerWeek > 0) {
        const onTrack = perWeek >= goalPerWeek - 0.25;
        out.push({
            id: "consistency", kind: "consistency", tone: onTrack ? "good" : "attention",
            title: `${perWeek} workouts a week`,
            detail: onTrack ? `On track with your goal of ${goalPerWeek} a week.` : `Your goal is ${goalPerWeek} a week — ${round1(goalPerWeek - perWeek)} short.`,
        });
    }

    const improver = progress
        .filter(p => p.changePct !== null && p.changePct > 0 && p.sessions.length >= 2)
        .sort((a, b) => b.changePct! - a.changePct!)[0];
    if (improver) {
        out.push({
            id: "progress", kind: "progress", tone: "good", exerciseId: improver.id,
            title: `${improver.name} up ${improver.changePct}%`,
            detail: `Heaviest set ${improver.start} → ${improver.current} kg.`,
        });
    }

    const recentCutoff = range.end.getTime() - 42 * DAY;
    const stalled = progress
        .filter(p => (p.daysSinceImprovement ?? 0) >= 42 && p.sessions.filter(s => new Date(s.date).getTime() >= recentCutoff).length >= 3)
        .sort((a, b) => b.sessions.length - a.sessions.length)[0];
    if (stalled) {
        out.push({
            id: "stalled", kind: "stalled", tone: "attention", exerciseId: stalled.id,
            title: `${stalled.name} has stalled`,
            detail: `No new best in ${Math.round(stalled.daysSinceImprovement! / 7)} weeks, despite regular sessions.`,
        });
    }

    // Only muscles you trained on purpose: one that only helps out (adductors in a squat) isn't neglected.
    const trained = balance.filter(m => m.setsPerWeek > 0 && m.direct);
    const low = [...trained].sort((a, b) => a.setsPerWeek - b.setsPerWeek)[0];
    if (low && trained.length >= 3 && low.setsPerWeek < SET_RANGE.low / 2) {
        out.push({
            id: "muscle", kind: "muscle", tone: "attention",
            title: `${low.muscle}: ${low.setsPerWeek} sets a week`,
            detail: "Your least-trained muscle group in this period.",
        });
    }

    const prs = progress.reduce((n, p) => n + p.prsInPeriod, 0);
    if (prs > 0) {
        const exercisesWithPr = progress.filter(p => p.prsInPeriod > 0).length;
        out.push({
            id: "prs", kind: "prs", tone: "neutral",
            title: `${prs} new personal record${prs !== 1 ? "s" : ""}`,
            detail: `Heavier than ever on ${exercisesWithPr} exercise${exercisesWithPr !== 1 ? "s" : ""}.`,
        });
    }
    return out.slice(0, 4);
}

// ── Cardio ────────────────────────────────────────────────────────────────────

export interface CardioSession { logId: string; date: string; exerciseId: string; name: string; km: number; minutes: number; pace: number | null }

export function cardioSessions(logs: LoggedWorkout[], exercises: Record<string, Exercise>): CardioSession[] {
    const out: CardioSession[] = [];
    for (const l of logs) {
        for (const ex of l.exercises) {
            const sets = ex.sets.filter(s => s.type === "Cardio");
            if (!sets.length) continue;
            const km = sets.reduce((n, s) => n + toKm(s.distance, s.distanceUnit), 0);
            const minutes = sets.reduce((n, s) => n + toMinutes(s.duration), 0);
            out.push({
                logId: l.id, date: l.loggedAt, exerciseId: ex.exerciseId, name: exercises[ex.exerciseId]?.name ?? "Cardio",
                km: round1(km), minutes: Math.round(minutes),
                pace: km > 0.2 && minutes > 0 ? minutes / km : null,
            });
        }
    }
    return out.sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime());
}

export function cardioByBucket(sessions: CardioSession[], range: Range) {
    const { unit, items } = buckets(range);
    const points = items.map(b => ({ key: b.key, label: b.label, longLabel: b.longLabel, km: 0, minutes: 0, sessions: 0 }));
    for (const s of sessions) {
        const t = new Date(s.date).getTime();
        const i = items.findIndex(b => t >= b.start.getTime() && t <= b.end.getTime());
        if (i < 0) continue;
        points[i].km = round1(points[i].km + s.km);
        points[i].minutes += s.minutes;
        points[i].sessions += 1;
    }
    return { unit, points };
}

export function formatPace(minPerKm: number): string {
    const m = Math.floor(minPerKm);
    const s = Math.round((minPerKm - m) * 60);
    return `${m}:${String(s === 60 ? 0 : s).padStart(2, "0")} /km`;
}

export function formatDuration(minutes: number): string {
    const h = Math.floor(minutes / 60);
    const m = Math.round(minutes % 60);
    return h ? `${h}h ${m}m` : `${m}m`;
}

export function cardioHighlights(current: CardioSession[], previous: CardioSession[], hasPrevious: boolean): Highlight[] {
    const out: Highlight[] = [];
    const km = current.reduce((n, s) => n + s.km, 0);
    const prevKm = previous.reduce((n, s) => n + s.km, 0);
    if (km > 0) {
        const pct = hasPrevious && prevKm > 0 ? Math.round(((km - prevKm) / prevKm) * 100) : null;
        out.push({
            id: "distance", kind: "distance", tone: pct === null ? "neutral" : pct >= 0 ? "good" : "attention",
            title: `${round1(km)} km in ${current.length} session${current.length !== 1 ? "s" : ""}`,
            detail: pct === null ? "No earlier period to compare with." : `${pct >= 0 ? "+" : ""}${pct}% distance vs the previous period.`,
        });
    }
    const longest = [...current].sort((a, b) => b.km - a.km)[0];
    if (longest && longest.km > 0) {
        out.push({
            id: "longest", kind: "longest", tone: "neutral",
            title: `Longest: ${longest.km} km`,
            detail: `${longest.name} on ${new Date(longest.date).toLocaleDateString("en-GB", { day: "numeric", month: "short" })}.`,
        });
    }
    // Pace only compares like with like: the most frequent activity.
    const counts = new Map<string, number>();
    current.forEach(s => { if (s.pace) counts.set(s.exerciseId, (counts.get(s.exerciseId) ?? 0) + 1); });
    const main = [...counts.entries()].sort((a, b) => b[1] - a[1])[0];
    if (main && main[1] >= 4) {
        const paced = current.filter(s => s.exerciseId === main[0] && s.pace);
        const firstHalf = paced.slice(0, Math.floor(paced.length / 2));
        const secondHalf = paced.slice(Math.floor(paced.length / 2));
        const avg = (xs: CardioSession[]) => xs.reduce((n, s) => n + s.pace!, 0) / xs.length;
        const delta = avg(secondHalf) - avg(firstHalf);
        out.push({
            id: "pace", kind: "pace", tone: delta <= 0 ? "good" : "attention",
            title: `${paced[0].name}: ${formatPace(avg(secondHalf))}`,
            detail: `${delta <= 0 ? "Faster" : "Slower"} by ${Math.abs(Math.round(delta * 60))} s/km than earlier in the period.`,
        });
    }
    return out;
}

// ── Cardio: per activity ──────────────────────────────────────────────────────

export interface CardioActivity {
    id: string;
    name: string;
    sessions: CardioSession[];
    totalKm: number;
    totalMinutes: number;
    longestKm: number;
    fastestPace: number | null;
    /** Change in average pace, second half vs first half of the period (negative = faster). */
    paceChangeSec: number | null;
}

export function cardioActivities(sessions: CardioSession[]): CardioActivity[] {
    const byId = new Map<string, CardioSession[]>();
    sessions.forEach(s => byId.set(s.exerciseId, [...(byId.get(s.exerciseId) ?? []), s]));
    return [...byId.entries()].map(([id, list]) => {
        const paced = list.filter(s => s.pace !== null);
        const avg = (xs: CardioSession[]) => xs.reduce((n, s) => n + s.pace!, 0) / xs.length;
        const half = Math.floor(paced.length / 2);
        return {
            id,
            name: list[0].name,
            sessions: list,
            totalKm: round1(list.reduce((n, s) => n + s.km, 0)),
            totalMinutes: list.reduce((n, s) => n + s.minutes, 0),
            longestKm: Math.max(...list.map(s => s.km)),
            fastestPace: paced.length ? Math.min(...paced.map(s => s.pace!)) : null,
            paceChangeSec: paced.length >= 4 ? Math.round((avg(paced.slice(half)) - avg(paced.slice(0, half))) * 60) : null,
        };
    }).sort((a, b) => b.sessions.length - a.sessions.length);
}
