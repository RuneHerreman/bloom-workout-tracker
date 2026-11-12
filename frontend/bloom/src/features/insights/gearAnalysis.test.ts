import { describe, expect, it } from "vitest";
import type { LoggedSet, LoggedWorkout } from "../../assets/js/data/apiTypes.ts";
import { cumulativeKm, gearUsage } from "./gearAnalysis.ts";
import type { Range } from "./analysis.ts";

const range: Range = { start: new Date(2026, 9, 1), end: new Date(2026, 9, 31, 23, 59), days: 31 };

const run = (km: number, minutes: number): LoggedSet => ({
    type: "Cardio", order: 0, duration: `00:${String(minutes).padStart(2, "0")}:00`, distance: km, distanceUnit: "km",
    reps: null, weight: null, weightUnit: null, rir: null,
});
const lift = (weight: number, reps: number, marker: LoggedSet["marker"] = null): LoggedSet => ({
    type: "Strength", order: 0, duration: null, distance: null, distanceUnit: null,
    reps, weight, weightUnit: "kg", rir: null, marker,
});
const log = (id: string, date: string, exercises: { exerciseId: string; gear: string[]; sets: LoggedSet[] }[]): LoggedWorkout => ({
    id, userId: "u", loggedAt: date, name: "Workout", note: null,
    exercises: exercises.map((e, i) => ({ ...e, order: i, gpxData: null })),
});

describe("gearUsage", () => {
    const logs = [
        log("a", "2026-10-05T07:00:00", [{ exerciseId: "run", gear: ["Pegasus"], sets: [run(10, 50)] }]),
        log("b", "2026-10-12T07:00:00", [
            { exerciseId: "run", gear: ["Pegasus"], sets: [run(5, 25)] },
            { exerciseId: "strides", gear: ["Pegasus"], sets: [run(1, 5)] },
        ]),
        log("c", "2026-10-14T18:00:00", [{ exerciseId: "squat", gear: ["Belt"], sets: [lift(60, 5, "WarmUp"), lift(100, 5), lift(100, 5)] }]),
        log("d", "2026-09-20T07:00:00", [{ exerciseId: "run", gear: ["Pegasus"], sets: [run(8, 40)] }]),
        log("e", "2026-10-20T07:00:00", [{ exerciseId: "run", gear: ["Old shoes"], sets: [run(3, 15)] }]),
    ];
    const items = gearUsage(logs, {}, range, ["Pegasus", "Belt", "Road bike"]);
    const byName = Object.fromEntries(items.map(i => [i.name, i]));

    it("adds up distance, time and pace for the period, and counts workouts once", () => {
        const p = byName.Pegasus.period;
        expect(p.km).toBe(16);
        expect(p.minutes).toBe(80);
        expect(p.sessions).toBe(3);
        expect(p.workouts).toBe(2);
        expect(p.pace).toBeCloseTo(5);
        expect(p.longestKm).toBe(10);
    });

    it("keeps all time and the period before apart from the period", () => {
        expect(byName.Pegasus.total.km).toBe(24);
        expect(byName.Pegasus.previous.km).toBe(8);
    });

    it("counts working sets and volume, leaving warm-ups out", () => {
        const p = byName.Belt.period;
        expect(p.sets).toBe(2);
        expect(p.reps).toBe(10);
        expect(p.volumeKg).toBe(1000);
    });

    it("lists unused gear, and gear only found in logs", () => {
        expect(byName["Road bike"].period.sessions).toBe(0);
        expect(byName["Road bike"].listed).toBe(true);
        expect(byName["Old shoes"].listed).toBe(false);
        expect(items.at(-1)?.name).toBe("Road bike");
    });

    it("builds the running total for wear", () => {
        expect(cumulativeKm(byName.Pegasus.sessions).map(p => p.km)).toEqual([8, 18, 23, 24]);
    });
});
