import type { Exercise } from "./api.ts";

/** What one working set counts for in a muscle that only assists. */
export const SECONDARY_SHARE = 0.5;

/**
 * Each muscle an exercise works with its share of a working set: 1 for the
 * primary movers, ½ for the muscles that assist (a bench press is a full set
 * for chest, half a set for triceps and front delts).
 */
export function muscleShares(ex: Pick<Exercise, "targetMuscles" | "secondaryMuscles"> | undefined): [muscle: string, share: number][] {
    if (!ex) return [];
    const shares = new Map<string, number>();
    for (const m of ex.targetMuscles) shares.set(m, 1);
    for (const m of ex.secondaryMuscles ?? []) if (!shares.has(m)) shares.set(m, SECONDARY_SHARE);
    return [...shares];
}

/** Every muscle an exercise works, primary first. */
export const allMuscles = (ex: Pick<Exercise, "targetMuscles" | "secondaryMuscles"> | undefined): string[] =>
    muscleShares(ex).map(([m]) => m);
