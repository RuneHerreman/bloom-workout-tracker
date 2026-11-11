// Turns raw logs into what the dashboard shows: this week against the goal,
// this month against the same point last month, key lifts, records, a year of
// activity and where the last four weeks of sets went.

import type { LoggedWorkout, WorkoutTemplate } from "../../assets/js/data/apiTypes.ts";
import type { Exercise } from "../exercises/api.ts";
import { exerciseProgress, muscleBalance, toKg, type MuscleVolume, type Range } from "../insights/analysis.ts";

const DAY = 86_400_000;

const startOfDay = (d: Date) => new Date(d.getFullYear(), d.getMonth(), d.getDate());
const addDays = (d: Date, n: number) => new Date(d.getFullYear(), d.getMonth(), d.getDate() + n);
const startOfWeek = (d: Date) => addDays(startOfDay(d), -((d.getDay() + 6) % 7)); // Monday
const dayKey = (d: Date) =>
    `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;

const isCardioOnly = (w: LoggedWorkout) => {
    const sets = w.exercises.flatMap(e => e.sets);
    return sets.length > 0 && sets.every(s => s.type === "Cardio");
};

export function workoutVolumeKg(w: LoggedWorkout): number {
    return w.exercises.reduce((t, ex) =>
        t + ex.sets.reduce((s, set) => s + (set.type === "Cardio" ? 0 : toKg(set.weight, set.weightUnit) * (set.reps ?? 0)), 0), 0);
}

export function formatVolume(kg: number): { value: string; unit: string } {
    return kg >= 1000 ? { value: (kg / 1000).toFixed(1), unit: "tonnes" } : { value: String(Math.round(kg)), unit: "kg" };
}

// ── Stats ─────────────────────────────────────────────────────────────────────

export interface DashboardStats {
    /** Mon → Sun: whether you trained that day. */
    weekDays: boolean[];
    todayIndex: number;
    workoutsThisWeek: number;
    goal: number;
    /** Consecutive weeks reaching the goal (this week counts once it's reached). */
    weekStreak: number;
    bestWeekStreak: number;

    workoutsThisMonth: number;
    /** Last month up to the same day of the month, so early-month numbers compare fairly. */
    workoutsLastMonthToDate: number;
    volumeThisMonthKg: number;
    volumeLastMonthToDateKg: number;
    prsThisMonth: number;
    prsLastMonth: number;
    lastWorkoutAt: string | null;
    workoutsLastMonth: number;
}

export function calculateDashboardStats(workouts: LoggedWorkout[], goal: number, now = new Date()): DashboardStats {
    const today = startOfDay(now);
    const weekStart = startOfWeek(now);

    const weekDays = Array.from({ length: 7 }, () => false);
    const perWeek = new Map<number, number>();
    for (const w of workouts) {
        const d = new Date(w.loggedAt);
        const ws = startOfWeek(d).getTime();
        perWeek.set(ws, (perWeek.get(ws) ?? 0) + 1);
        if (ws === weekStart.getTime()) weekDays[(d.getDay() + 6) % 7] = true;
    }
    const workoutsThisWeek = perWeek.get(weekStart.getTime()) ?? 0;

    let weekStreak = workoutsThisWeek >= goal ? 1 : 0;
    for (let w = addDays(weekStart, -7); (perWeek.get(w.getTime()) ?? 0) >= goal; w = addDays(w, -7)) weekStreak++;

    let bestWeekStreak = 0;
    const weeks = [...perWeek.keys()].sort((a, b) => a - b);
    if (weeks.length) {
        let run = 0;
        for (let w = new Date(weeks[0]); w <= weekStart; w = addDays(w, 7)) {
            run = (perWeek.get(w.getTime()) ?? 0) >= goal ? run + 1 : 0;
            bestWeekStreak = Math.max(bestWeekStreak, run);
        }
    }

    const monthStart = new Date(now.getFullYear(), now.getMonth(), 1);
    const lastMonthStart = new Date(now.getFullYear(), now.getMonth() - 1, 1);
    // Same day-of-month last month (clamped: 31 March compares with 28/29 February).
    const lastMonthDays = new Date(now.getFullYear(), now.getMonth(), 0).getDate();
    const lastMonthCutoff = addDays(lastMonthStart, Math.min(now.getDate(), lastMonthDays));

    const between = (from: Date, to: Date) => workouts.filter(w => {
        const t = new Date(w.loggedAt);
        return t >= from && t < to;
    });
    const thisMonth = between(monthStart, addDays(today, 1));
    const lastMonthToDate = between(lastMonthStart, lastMonthCutoff);

    return {
        weekDays,
        todayIndex: (now.getDay() + 6) % 7,
        workoutsThisWeek,
        goal,
        weekStreak,
        bestWeekStreak,
        workoutsThisMonth: thisMonth.length,
        workoutsLastMonthToDate: lastMonthToDate.length,
        volumeThisMonthKg: thisMonth.reduce((t, w) => t + workoutVolumeKg(w), 0),
        volumeLastMonthToDateKg: lastMonthToDate.reduce((t, w) => t + workoutVolumeKg(w), 0),
        prsThisMonth: countRecords(workouts, monthStart, addDays(today, 1)),
        prsLastMonth: countRecords(workouts, lastMonthStart, monthStart),
        lastWorkoutAt: workouts.reduce<string | null>((l, w) => (!l || w.loggedAt > l ? w.loggedAt : l), null),
        workoutsLastMonth: between(lastMonthStart, monthStart).length,
    };
}

/** Exercises whose heaviest weight in [from, to) beats everything before `from`. */
function countRecords(workouts: LoggedWorkout[], from: Date, to: Date): number {
    const before = new Map<string, number>();
    const during = new Map<string, number>();
    for (const w of workouts) {
        const t = new Date(w.loggedAt);
        if (t >= to) continue;
        const target = t >= from ? during : before;
        for (const ex of w.exercises) {
            const max = Math.max(0, ...ex.sets.filter(s => s.type !== "Cardio" && s.marker !== "WarmUp").map(s => toKg(s.weight, s.weightUnit)));
            if (max > 0) target.set(ex.exerciseId, Math.max(target.get(ex.exerciseId) ?? 0, max));
        }
    }
    let n = 0;
    for (const [id, max] of during) if (before.has(id) && max > before.get(id)!) n++;
    return n;
}

// ── Activity (last 12 months, Monday-aligned weeks) ───────────────────────────

export type ActivityKind = "none" | "strength" | "cardio" | "both" | "future";

export interface ActivityDay { date: string; kind: ActivityKind; logId?: string; names: string[] }

export interface ActivityData {
    /** 53 columns of 7 days (Mon → Sun). */
    weeks: ActivityDay[][];
    months: { column: number; label: string }[];
    total: number;
    perWeek: number;
}

export function buildActivity(workouts: LoggedWorkout[], now = new Date()): ActivityData {
    const today = startOfDay(now);
    const first = addDays(startOfWeek(now), -52 * 7);

    const byDay = new Map<string, LoggedWorkout[]>();
    for (const w of workouts) {
        const k = dayKey(new Date(w.loggedAt));
        byDay.set(k, [...(byDay.get(k) ?? []), w]);
    }

    let total = 0;
    const weeks: ActivityDay[][] = [];
    const months: ActivityData["months"] = [];
    for (let c = 0; c < 53; c++) {
        const monday = addDays(first, c * 7);
        if (c === 0 || monday.getMonth() !== addDays(monday, -7).getMonth()) {
            // Skip a first label that would collide with the next month's.
            if (!(c === 0 && monday.getDate() > 20)) {
                months.push({ column: c, label: monday.toLocaleDateString("en-GB", { month: "short" }) });
            }
        }
        const week: ActivityDay[] = [];
        for (let r = 0; r < 7; r++) {
            const d = addDays(monday, r);
            const k = dayKey(d);
            if (d > today) { week.push({ date: k, kind: "future", names: [] }); continue; }
            const logs = byDay.get(k) ?? [];
            total += logs.length;
            const cardio = logs.some(isCardioOnly);
            const strength = logs.some(l => !isCardioOnly(l));
            week.push({
                date: k,
                kind: cardio && strength ? "both" : cardio ? "cardio" : strength ? "strength" : "none",
                logId: logs[0]?.id,
                names: logs.map(l => l.name),
            });
        }
        weeks.push(week);
    }
    const spanWeeks = (today.getTime() - first.getTime()) / (7 * DAY);
    return { weeks, months, total, perWeek: Math.round((total / spanWeeks) * 10) / 10 };
}

// ── Key lifts ─────────────────────────────────────────────────────────────────

export interface LiftPoint { t: number; value: number; reps: number; record: boolean }

export interface KeyLift {
    id: string;
    name: string;
    points: LiftPoint[];
    last: { weight: number; reps: number; date: string };
    record: { weight: number; date: string };
    lastWasRecord: boolean;
    sessions: number;
}

/** The most-trained lifts of the last six months, with every session's heaviest set. */
export function buildKeyLifts(workouts: LoggedWorkout[], exercises: Record<string, Exercise>, count = 6, now = new Date()): KeyLift[] {
    const range: Range = { start: new Date(now.getTime() - 182 * DAY), end: now, days: 182 };
    return exerciseProgress(workouts, exercises, range)
        .filter(p => p.sessions.length >= 2)
        .slice(0, count)
        .map(p => {
            const all = p.allSessions;
            const best = all.reduce((a, b) => (b.heaviest > a.heaviest ? b : a));
            const last = all[all.length - 1];
            return {
                id: p.id,
                name: p.name,
                points: p.sessions.map(s => ({ t: new Date(s.date).getTime(), value: s.heaviest, reps: s.heaviestReps, record: s.isWeightPr })),
                last: { weight: last.heaviest, reps: last.heaviestReps, date: last.date },
                record: { weight: best.heaviest, date: best.date },
                lastWasRecord: last.isWeightPr,
                sessions: p.sessions.length,
            };
        });
}

// ── Muscle focus (last 4 weeks) ───────────────────────────────────────────────

export interface MuscleFocus {
    muscles: MuscleVolume[];
    untrained: string[];
    strengthSessions: number;
    cardioSessions: number;
}

export function buildMuscleFocus(workouts: LoggedWorkout[], exercises: Record<string, Exercise>, now = new Date()): MuscleFocus {
    const range: Range = { start: new Date(now.getTime() - 28 * DAY), end: now, days: 28 };
    const recent = workouts.filter(w => new Date(w.loggedAt) >= range.start);
    const muscles = muscleBalance(recent, exercises, range);
    const trained = new Set(muscles.map(m => m.muscle));
    // Muscles you've trained at some point that got nothing in the last four weeks.
    const everTrained = new Set(workouts.flatMap(w => w.exercises.flatMap(e =>
        e.sets.some(s => s.type !== "Cardio") ? exercises[e.exerciseId]?.targetMuscles ?? [] : [])));
    return {
        muscles,
        untrained: [...everTrained].filter(m => !trained.has(m)).sort(),
        strengthSessions: recent.filter(w => !isCardioOnly(w)).length,
        cardioSessions: recent.filter(isCardioOnly).length,
    };
}

// ── Readiness & up next ───────────────────────────────────────────────────────

/** Days of rest after which a muscle counts as recovered (a common 48–72 h guideline, rounded up). */
export const RESTED_DAYS = 3;

export interface MuscleRest { muscle: string; days: number }

export interface UpNext {
    template: WorkoutTemplate;
    lastDone: string | null;
    rested: MuscleRest[];
    recovering: MuscleRest[];
}

/** Whole days since each muscle last got working sets. */
export function muscleRest(workouts: LoggedWorkout[], exercises: Record<string, Exercise>, now = new Date()): Map<string, number> {
    const last = new Map<string, number>();
    for (const w of workouts) {
        const t = startOfDay(new Date(w.loggedAt)).getTime();
        for (const ex of w.exercises) {
            if (!ex.sets.some(s => s.type !== "Cardio" && s.marker !== "WarmUp")) continue;
            for (const m of exercises[ex.exerciseId]?.targetMuscles ?? []) {
                if (t > (last.get(m) ?? 0)) last.set(m, t);
            }
        }
    }
    const today = startOfDay(now).getTime();
    return new Map([...last].map(([m, t]) => [m, Math.round((today - t) / DAY)]));
}

/**
 * Follows the rotation you actually train: of the templates whose muscles are
 * (nearly) all rested, the one you did longest ago. Templates you've never
 * logged come after those. If nothing is fully rested, the most rested wins.
 */
export function suggestNext(
    templates: WorkoutTemplate[], workouts: LoggedWorkout[], exercises: Record<string, Exercise>, now = new Date(),
): UpNext | null {
    const rest = muscleRest(workouts, exercises, now);
    const lastDoneByName = new Map<string, string>();
    for (const w of workouts) {
        const k = w.name.trim().toLowerCase();
        if (!lastDoneByName.has(k) || w.loggedAt > lastDoneByName.get(k)!) lastDoneByName.set(k, w.loggedAt);
    }

    const scored = templates
        .filter(t => t.exercises.length > 0)
        .map(t => {
            const weight = new Map<string, number>();
            for (const ex of t.exercises) {
                for (const m of exercises[ex.exerciseId]?.targetMuscles ?? []) {
                    weight.set(m, (weight.get(m) ?? 0) + Math.max(1, ex.sets.length));
                }
            }
            let total = 0, score = 0;
            const rested: MuscleRest[] = [], recovering: MuscleRest[] = [];
            for (const [m, w] of weight) {
                const days = rest.get(m) ?? Infinity;
                total += w;
                score += w * Math.min(1, days / RESTED_DAYS);
                (days >= RESTED_DAYS ? rested : recovering).push({ muscle: m, days });
            }
            const lastDone = lastDoneByName.get(t.name.trim().toLowerCase()) ?? null;
            const byWeight = (a: MuscleRest, b: MuscleRest) => (weight.get(b.muscle) ?? 0) - (weight.get(a.muscle) ?? 0);
            return { template: t, lastDone, rested: rested.sort(byWeight), recovering: recovering.sort(byWeight), score: total ? score / total : 0 };
        });

    const READY = 0.9; // sets-weighted share of the template's muscles that are rested
    const rank = (x: typeof scored[number]) => (x.score >= READY ? (x.lastDone ? 0 : 1) : 2);
    scored.sort((a, b) =>
        rank(a) - rank(b)
        || (rank(a) === 2 ? b.score - a.score : 0)
        || (a.lastDone ?? "").localeCompare(b.lastDone ?? ""));
    const best = scored[0];
    return best ? { template: best.template, lastDone: best.lastDone, rested: best.rested, recovering: best.recovering } : null;
}

// ── Recent workouts ───────────────────────────────────────────────────────────

export interface LogEntryData {
    id: string;
    name: string;
    date: string;
    /** One entry per exercise: drawn as the coloured bar strip. */
    exerciseTypes: ("strength" | "cardio")[];
}

export function buildRecentWorkouts(workouts: LoggedWorkout[], count = 5): LogEntryData[] {
    return [...workouts]
        .sort((a, b) => b.loggedAt.localeCompare(a.loggedAt))
        .slice(0, count)
        .map(w => ({
            id: w.id,
            name: w.name,
            date: w.loggedAt,
            exerciseTypes: w.exercises.map(e => (e.sets.length > 0 && e.sets.every(s => s.type === "Cardio") ? "cardio" : "strength")),
        }));
}
