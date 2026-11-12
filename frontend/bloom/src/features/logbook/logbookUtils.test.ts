import { describe, expect, it } from "vitest";
import type { LoggedExercise, LoggedSet } from "./api.ts";
import { defaultSet, duplicateSet, findMissingReps, prefilledFirstSet, resolveHints } from "./logbookUtils.ts";

const strength = (reps: number | null, weight: number | null, extra: Partial<LoggedSet> = {}): LoggedSet => ({
    type: "Strength", order: 1, reps, weight, weightUnit: "kg", rir: null, marker: null,
    duration: null, distance: null, distanceUnit: null, ...extra,
});
const exercise = (sets: LoggedSet[]): LoggedExercise => ({ exerciseId: "squat", order: 1, sets, gpxData: null, note: null, gear: [] });

describe("defaultSet", () => {
    it("makes nothing up for an exercise you've never logged", () => {
        expect(defaultSet("Strength")).toMatchObject({ reps: null, weight: null });
        expect(defaultSet("Cardio")).toMatchObject({ duration: null, distance: null });
    });
});

describe("prefilledFirstSet / duplicateSet", () => {
    it("leaves the fields empty and shows last time as a hint", () => {
        const set = prefilledFirstSet("Strength", [strength(5, 60, { rir: 2 })]);
        expect(set).toMatchObject({ reps: null, weight: null, rir: null, hint: { reps: 5, weight: 60 } });
    });

    it("hints the row above with what it shows: typed values, else its own hint", () => {
        const above = strength(null, 62.5, { hint: { reps: 5, weight: 60 } });
        expect(duplicateSet(above, 2)).toMatchObject({ order: 2, reps: null, weight: null, hint: { reps: 5, weight: 62.5 } });
    });
});

describe("resolveHints", () => {
    it("fills empty fields from the hint and never sends the hint", () => {
        const [ex] = resolveHints([exercise([strength(null, 70, { hint: { reps: 5, weight: 60 } })])]);
        expect(ex.sets[0]).toMatchObject({ reps: 5, weight: 70 });
        expect(ex.sets[0]).not.toHaveProperty("hint");
    });

    it("saves an empty weight with no hint as bodyweight", () => {
        expect(resolveHints([exercise([strength(10, null)])])[0].sets[0].weight).toBe(0);
    });
});

describe("findMissingReps", () => {
    it("finds a set with no reps typed and none from last time", () => {
        const ex = exercise([strength(5, 60), strength(null, 60)]);
        expect(findMissingReps([ex])).toEqual({ exercise: ex, setIndex: 1 });
        expect(findMissingReps([exercise([strength(null, 60, { hint: { reps: 8 } })])])).toBeNull();
    });
});
