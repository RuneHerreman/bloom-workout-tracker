import type { LoggedWorkout } from "../../assets/js/data/apiTypes.ts";
import type { Exercise } from "../exercises/api.ts";
import { buckets, inRange, isWorkingStrengthSet, toKg, toKm, toMinutes, type Range } from "./analysis.ts";

/** One exercise in one workout that used the item. */
export interface GearSession {
    logId: string;
    date: string;
    exerciseId: string;
    km: number;
    minutes: number;
    /** Working sets (no warm-ups). */
    sets: number;
    reps: number;
    /** Weight × reps over working strength sets, in kg. */
    volumeKg: number;
}

export interface GearTotals {
    sessions: number;
    /** Distinct workouts: two exercises with the same shoes in one run count once. */
    workouts: number;
    km: number;
    minutes: number;
    sets: number;
    reps: number;
    volumeKg: number;
    /** Minutes per km over sessions that logged both, or null. */
    pace: number | null;
    longestKm: number;
    heaviestVolumeKg: number;
}

export interface GearUse {
    name: string;
    /** Still in your gear list; false for items only found in old logs. */
    listed: boolean;
    /** All sessions, oldest first. */
    sessions: GearSession[];
    period: GearTotals;
    total: GearTotals;
    /** Same length of time just before the period, for the change. */
    previous: GearTotals;
    firstUsed: string | null;
    lastUsed: string | null;
    /** Exercises it was used for in the period, most used first. */
    usedFor: { exerciseId: string; name: string; sessions: number }[];
}

const EMPTY: GearTotals = { sessions: 0, workouts: 0, km: 0, minutes: 0, sets: 0, reps: 0, volumeKg: 0, pace: null, longestKm: 0, heaviestVolumeKg: 0 };

function totals(sessions: GearSession[]): GearTotals {
    if (!sessions.length) return EMPTY;
    let km = 0, minutes = 0, sets = 0, reps = 0, volumeKg = 0, paceKm = 0, paceMin = 0, longestKm = 0, heaviestVolumeKg = 0;
    for (const s of sessions) {
        km += s.km; minutes += s.minutes; sets += s.sets; reps += s.reps; volumeKg += s.volumeKg;
        if (s.km > 0.2 && s.minutes > 0) { paceKm += s.km; paceMin += s.minutes; }
        longestKm = Math.max(longestKm, s.km);
        heaviestVolumeKg = Math.max(heaviestVolumeKg, s.volumeKg);
    }
    return {
        sessions: sessions.length,
        workouts: new Set(sessions.map(s => s.logId)).size,
        km, minutes, sets, reps, volumeKg,
        pace: paceKm > 0 ? paceMin / paceKm : null,
        longestKm, heaviestVolumeKg,
    };
}

/**
 * How each gear item has been used: distance, time, sets, reps and volume,
 * for the period, the period before it, and all time. Items in your gear list
 * show up even when unused; items only found in logs (since removed from the
 * list) show up too, so their history isn't lost.
 */
export function gearUsage(logs: LoggedWorkout[], exercises: Record<string, Exercise>, range: Range, gearList: string[]): GearUse[] {
    const byName = new Map<string, GearSession[]>();
    for (const name of gearList) byName.set(name, []);

    for (const log of logs) {
        for (const ex of log.exercises) {
            if (!ex.gear?.length) continue;
            let km = 0, minutes = 0, sets = 0, reps = 0, volumeKg = 0;
            for (const s of ex.sets) {
                km += toKm(s.distance, s.distanceUnit);
                minutes += toMinutes(s.duration);
                if (s.marker === "WarmUp") continue;
                sets += 1;
                reps += s.reps ?? 0;
                if (isWorkingStrengthSet(s) && s.reps) volumeKg += toKg(s.weight, s.weightUnit) * s.reps;
            }
            const session: GearSession = { logId: log.id, date: log.loggedAt, exerciseId: ex.exerciseId, km, minutes, sets, reps, volumeKg };
            for (const g of new Set(ex.gear)) {
                const list = byName.get(g) ?? [];
                list.push(session);
                byName.set(g, list);
            }
        }
    }

    const span = range.end.getTime() - range.start.getTime();
    const prev: Range = { start: new Date(range.start.getTime() - span), end: new Date(range.start.getTime() - 1), days: range.days };
    const listed = new Set(gearList);

    return [...byName.entries()].map(([name, all]) => {
        const sessions = [...all].sort((a, b) => a.date.localeCompare(b.date));
        const now = sessions.filter(s => inRange(s.date, range));
        const counts = new Map<string, number>();
        for (const s of now) counts.set(s.exerciseId, (counts.get(s.exerciseId) ?? 0) + 1);
        return {
            name,
            listed: listed.has(name),
            sessions,
            period: totals(now),
            total: totals(sessions),
            previous: totals(sessions.filter(s => inRange(s.date, prev))),
            firstUsed: sessions[0]?.date ?? null,
            lastUsed: sessions.at(-1)?.date ?? null,
            usedFor: [...counts.entries()]
                .map(([exerciseId, n]) => ({ exerciseId, name: exercises[exerciseId]?.name ?? "Unknown exercise", sessions: n }))
                .sort((a, b) => b.sessions - a.sessions),
        };
    }).sort((a, b) =>
        // Used in the period first (most sessions first), then by last use, then the unused ones.
        b.period.sessions - a.period.sessions || (b.lastUsed ?? "").localeCompare(a.lastUsed ?? "") || a.name.localeCompare(b.name));
}

/** Per week or month across the period: what the chart shows. */
export function gearByBucket(sessions: GearSession[], range: Range) {
    const { unit, items } = buckets(range);
    const points = items.map(b => ({ key: b.key, label: b.label, longLabel: b.longLabel, km: 0, minutes: 0, sets: 0, volumeKg: 0, sessions: 0 }));
    for (const s of sessions) {
        const t = new Date(s.date).getTime();
        const i = items.findIndex(b => t >= b.start.getTime() && t <= b.end.getTime());
        if (i < 0) continue;
        const p = points[i];
        p.km += s.km; p.minutes += s.minutes; p.sets += s.sets; p.volumeKg += s.volumeKg; p.sessions += 1;
    }
    for (const p of points) { p.km = Math.round(p.km * 10) / 10; p.volumeKg = Math.round(p.volumeKg); p.minutes = Math.round(p.minutes); }
    return { unit, points };
}

/** Total distance over time, for wear: one point per session that covered distance. */
export function cumulativeKm(sessions: GearSession[]): { t: number; km: number; added: number; date: string }[] {
    let km = 0;
    const out: { t: number; km: number; added: number; date: string }[] = [];
    for (const s of sessions) {
        if (s.km <= 0) continue;
        km += s.km;
        out.push({ t: new Date(s.date).getTime(), km: Math.round(km * 10) / 10, added: Math.round(s.km * 10) / 10, date: s.date });
    }
    return out;
}
