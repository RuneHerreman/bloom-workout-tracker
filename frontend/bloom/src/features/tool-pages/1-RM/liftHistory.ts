import type { LoggedWorkout } from "../../../assets/js/data/apiTypes.ts";
import { e1rm } from "../../../utils/bodyMath.ts";
import { toKg } from "../../insights/analysis.ts";

export interface BestSet {
    weightKg: number;
    reps: number;
    rir: number | null;
    e1rm: number;
    date: string;
}

export interface LiftHistory {
    exerciseId: string;
    lastDate: string;
    /** Highest estimate in the recent window; falls back to the last session's best when the window is empty. */
    recent: BestSet;
    /** Highest estimate ever. */
    allTime: BestSet;
    /** Each session's best set, oldest first. */
    sessions: BestSet[];
}

/** How far back "best recent set" looks. */
export const RECENT_DAYS = 56;

/**
 * Best sets per strength exercise: warm-ups and empty sets are skipped, RIR counts
 * as reps in reserve. Sorted by most recently trained.
 */
export function liftHistories(logs: LoggedWorkout[], now = new Date()): LiftHistory[] {
    const since = now.getTime() - RECENT_DAYS * 86_400_000;
    const sorted = [...logs].sort((a, b) => new Date(a.loggedAt).getTime() - new Date(b.loggedAt).getTime());
    const map = new Map<string, { lastDate: string; lastBest: BestSet; recent: BestSet | null; allTime: BestSet; sessions: BestSet[] }>();

    for (const log of sorted) {
        const t = new Date(log.loggedAt).getTime();
        for (const ex of log.exercises) {
            let sessionBest: BestSet | null = null;
            for (const s of ex.sets) {
                if (s.type === "Cardio" || s.marker === "WarmUp") continue;
                const kg = toKg(s.weight, s.weightUnit);
                if (!(kg > 0) || !((s.reps ?? 0) > 0)) continue;
                const est = e1rm(kg, s.reps!, s.rir);
                if (!sessionBest || est > sessionBest.e1rm) {
                    sessionBest = { weightKg: Math.round(kg * 100) / 100, reps: s.reps!, rir: s.rir, e1rm: est, date: log.loggedAt };
                }
            }
            if (!sessionBest) continue;
            const prev = map.get(ex.exerciseId);
            const inWindow = t >= since;
            if (!prev) {
                map.set(ex.exerciseId, { lastDate: log.loggedAt, lastBest: sessionBest, recent: inWindow ? sessionBest : null, allTime: sessionBest, sessions: [sessionBest] });
                continue;
            }
            prev.lastDate = log.loggedAt;
            prev.sessions.push(sessionBest);
            prev.lastBest = sessionBest;
            if (inWindow && (!prev.recent || sessionBest.e1rm > prev.recent.e1rm)) prev.recent = sessionBest;
            if (sessionBest.e1rm > prev.allTime.e1rm) prev.allTime = sessionBest;
        }
    }

    return [...map.entries()]
        .map(([exerciseId, h]) => ({ exerciseId, lastDate: h.lastDate, recent: h.recent ?? h.lastBest, allTime: h.allTime, sessions: h.sessions }))
        .sort((a, b) => b.lastDate.localeCompare(a.lastDate));
}
