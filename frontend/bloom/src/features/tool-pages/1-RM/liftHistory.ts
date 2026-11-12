import type { LoggedWorkout } from "../../../assets/js/data/apiTypes.ts";
import { e1rm, repsToFailure, RELIABLE_REPS } from "../../../utils/bodyMath.ts";
import { toKg } from "../../insights/analysis.ts";

export interface BestSet {
    weightKg: number;
    reps: number;
    rir: number | null;
    e1rm: number;
    date: string;
    /** From a set past RELIABLE_REPS to failure: the estimate is rough. */
    rough: boolean;
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
    /** Only ever trained past RELIABLE_REPS: every estimate for it is rough. */
    rough: boolean;
}

/** How far back "best recent set" looks. */
export const RECENT_DAYS = 56;

const best = (sets: BestSet[]) => sets.reduce((a, b) => (b.e1rm > a.e1rm ? b : a));

/**
 * Best sets per strength exercise, sorted by most recently trained. Warm-ups and
 * empty sets are skipped; RIR counts as reps in reserve, and a set logged without
 * RIR counts as taken to failure.
 *
 * Estimates come from sets of up to RELIABLE_REPS reps to failure: that's where
 * the formulas hold, and where an untracked RIR changes the least. A session with
 * only longer sets is left out, unless the lift is never trained heavier; then
 * every session counts and the lift is marked rough.
 */
export function liftHistories(logs: LoggedWorkout[], now = new Date()): LiftHistory[] {
    const since = now.getTime() - RECENT_DAYS * 86_400_000;
    const sorted = [...logs].sort((a, b) => new Date(a.loggedAt).getTime() - new Date(b.loggedAt).getTime());
    const byLift = new Map<string, { lastDate: string; sessions: BestSet[] }>();

    for (const log of sorted) {
        for (const ex of log.exercises) {
            let good: BestSet | null = null;
            let rough: BestSet | null = null;
            for (const s of ex.sets) {
                if (s.type === "Cardio" || s.marker === "WarmUp") continue;
                const kg = toKg(s.weight, s.weightUnit);
                if (!(kg > 0) || !((s.reps ?? 0) > 0)) continue;
                const isRough = repsToFailure(s.reps!, s.rir) > RELIABLE_REPS;
                const set: BestSet = { weightKg: Math.round(kg * 100) / 100, reps: s.reps!, rir: s.rir, e1rm: e1rm(kg, s.reps!, s.rir), date: log.loggedAt, rough: isRough };
                if (isRough) { if (!rough || set.e1rm > rough.e1rm) rough = set; }
                else if (!good || set.e1rm > good.e1rm) good = set;
            }
            const sessionBest = good ?? rough;
            if (!sessionBest) continue;
            const entry = byLift.get(ex.exerciseId) ?? { lastDate: log.loggedAt, sessions: [] };
            entry.lastDate = log.loggedAt;
            entry.sessions.push(sessionBest);
            byLift.set(ex.exerciseId, entry);
        }
    }

    const out: LiftHistory[] = [];
    for (const [exerciseId, { lastDate, sessions: all }] of byLift) {
        const reliable = all.filter(s => !s.rough);
        const sessions = reliable.length ? reliable : all;
        const recentSets = sessions.filter(s => new Date(s.date).getTime() >= since);
        out.push({
            exerciseId,
            lastDate,
            recent: recentSets.length ? best(recentSets) : sessions[sessions.length - 1],
            allTime: best(sessions),
            sessions,
            rough: reliable.length === 0,
        });
    }
    return out.sort((a, b) => b.lastDate.localeCompare(a.lastDate));
}
