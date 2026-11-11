// Turns raw logs into answers: am I getting stronger, am I consistent,
// is my training balanced, and what needs attention.

import type { LoggedSet, LoggedWorkout } from "../../assets/js/data/apiTypes.ts";
import type { Exercise } from "../exercises/api.ts";

// ── Periods & time buckets ────────────────────────────────────────────────────

export type Period = "4w" | "12w" | "6m" | "1y" | "all";

export const PERIODS: { value: Period; label: string; days: number | null }[] = [
    { value: "4w", label: "4 weeks", days: 28 },
    { value: "12w", label: "12 weeks", days: 84 },
    { value: "6m", label: "6 months", days: 182 },
    { value: "1y", label: "1 year", days: 365 },
    { value: "all", label: "All time", days: null },
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

const toKm = (d: number | null, unit: string | null) => {
    if (d == null) return 0;
    const u = (unit ?? "km").toLowerCase();
    return u === "miles" ? d * 1.609344 : u === "meters" ? d / 1000 : d;
};

const toMinutes = (dur: string | null) => {
    if (!dur) return 0;
    const [h, m, s] = dur.split(":").map(Number);
    return (h || 0) * 60 + (m || 0) + (s || 0) / 60;
};

const isWorkingStrengthSet = (s: LoggedSet) => s.type !== "Cardio" && s.marker !== "WarmUp";

/** Estimated one-rep max (Epley). Reps above 12 are capped: estimates get unreliable. */
export function e1rm(weightKg: number, reps: number): number {
    if (weightKg <= 0 || reps <= 0) return 0;
    if (reps === 1) return weightKg;
    return weightKg * (1 + Math.min(reps, 12) / 30);
}

const round1 = (n: number) => Math.round(n * 10) / 10;

// ── Strength: per-exercise progress ───────────────────────────────────────────

export interface ExerciseSession {
    logId: string;
    date: string;
    e1rm: number;
    /** Set with the highest estimated 1RM. */
    topSet: { weight: number; reps: number };
    heaviest: number;
    bestSetVolume: number;
    workingSets: number;
    volumeKg: number;
    isPr: boolean;
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
            let best = 0, top = { weight: 0, reps: 0 }, volume = 0, heaviest = 0, bestSetVolume = 0;
            for (const s of sets) {
                const kg = toKg(s.weight, s.weightUnit);
                const est = e1rm(kg, s.reps!);
                volume += kg * s.reps!;
                heaviest = Math.max(heaviest, kg);
                bestSetVolume = Math.max(bestSetVolume, kg * s.reps!);
                if (est > best) { best = est; top = { weight: round1(kg), reps: s.reps! }; }
            }
            const list = byExercise.get(ex.exerciseId) ?? [];
            list.push({
                logId: log.id, date: log.loggedAt, e1rm: round1(best), topSet: top,
                heaviest: round1(heaviest), bestSetVolume: Math.round(bestSetVolume),
                workingSets: sets.length, volumeKg: Math.round(volume), isPr: false,
            });
            byExercise.set(ex.exerciseId, list);
        }
    }

    const result: ExerciseProgress[] = [];
    for (const [id, all] of byExercise) {
        const info = exercises[id];
        let runningBest = 0;
        let lastImprovement: string | null = null;
        for (let i = 0; i < all.length; i++) {
            const s = all[i];
            if (s.e1rm > runningBest) {
                s.isPr = i > 0; // the first session is the baseline
                runningBest = s.e1rm;
                lastImprovement = s.date;
            }
        }
        const sessions = all.filter(s => inRange(s.date, range));
        if (!sessions.length) continue;
        const before = all.filter(s => new Date(s.date) < range.start);
        // Start = best before the period (or the first session in it); current = best of the last two sessions.
        const start = before.length ? Math.max(...before.map(s => s.e1rm)) : sessions[0].e1rm;
        const current = Math.max(...sessions.slice(-2).map(s => s.e1rm));
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
            prsInPeriod: sessions.filter(s => s.isPr).length,
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

export interface MuscleVolume { muscle: string; setsPerWeek: number; lastTrained: string | null }

/** Working sets per week for each muscle (an exercise counts for every muscle it targets). */
export function muscleBalance(logs: LoggedWorkout[], exercises: Record<string, Exercise>, range: Range): MuscleVolume[] {
    const sets = new Map<string, number>();
    const last = new Map<string, string>();
    for (const l of logs) {
        if (!inRange(l.loggedAt, range)) continue;
        for (const ex of l.exercises) {
            const n = ex.sets.filter(isWorkingStrengthSet).length;
            if (!n) continue;
            for (const m of exercises[ex.exerciseId]?.targetMuscles ?? []) {
                sets.set(m, (sets.get(m) ?? 0) + n);
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
        .map(([muscle, n]) => ({ muscle, setsPerWeek: round1(n / weeks), lastTrained: last.get(muscle) ?? null }))
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
            detail: `Estimated 1RM ${improver.start} → ${improver.current} kg.`,
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

    const trained = balance.filter(m => m.setsPerWeek > 0);
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
            detail: `Across ${exercisesWithPr} exercise${exercisesWithPr !== 1 ? "s" : ""}, by estimated 1RM.`,
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
