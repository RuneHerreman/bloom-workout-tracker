import { useEffect, useState } from "react";
import { getLogs, peekLogs, type LoggedWorkout } from "../../logbook/api.ts";

export interface ExerciseUsage {
    /** Exercises you've logged, most recently logged first. */
    recentIds: string[];
    /** How many logs each exercise appears in. */
    counts: Map<string, number>;
}

const EMPTY: ExerciseUsage = { recentIds: [], counts: new Map() };

function usageFrom(logs: LoggedWorkout[] | undefined): ExerciseUsage {
    if (!logs?.length) return EMPTY;
    const sorted = [...logs].sort((a, b) => new Date(b.loggedAt).getTime() - new Date(a.loggedAt).getTime());
    const recentIds: string[] = [];
    const counts = new Map<string, number>();
    for (const log of sorted) {
        for (const id of new Set(log.exercises.map(e => e.exerciseId))) {
            if (!counts.has(id)) recentIds.push(id);
            counts.set(id, (counts.get(id) ?? 0) + 1);
        }
    }
    return { recentIds, counts };
}

/**
 * What the user has actually logged, from their logbook rather than this browser,
 * so a new device still knows their exercises. Uses the shared logs cache.
 */
export function useLoggedExerciseUsage(): ExerciseUsage {
    const [usage, setUsage] = useState<ExerciseUsage>(() => usageFrom(peekLogs()));

    useEffect(() => {
        let alive = true;
        getLogs().then(logs => { if (alive) setUsage(usageFrom(logs)); }).catch(() => {});
        return () => { alive = false; };
    }, []);

    return usage;
}
