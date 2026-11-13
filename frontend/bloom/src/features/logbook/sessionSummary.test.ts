import { describe, expect, it } from "vitest";
import type { LoggedExercise, LoggedSet, LoggedWorkout } from "./api.ts";
import type { Exercise } from "../exercises/api.ts";
import { summarizeSession } from "./sessionSummary.ts";

const set = (reps: number, weight: number, extra: Partial<LoggedSet> = {}): LoggedSet => ({
    type: "Strength", order: 1, reps, weight, weightUnit: "kg", rir: null, marker: null,
    duration: null, distance: null, distanceUnit: null, ...extra,
});
const ex = (exerciseId: string, sets: LoggedSet[]): LoggedExercise => ({ exerciseId, order: 1, sets, gpxData: null, note: null, gear: [] });
const log = (id: string, name: string, loggedAt: string, exercises: LoggedExercise[]): LoggedWorkout =>
    ({ id, userId: "u", name, loggedAt, note: null, exercises });

const exercise = (id: string, name: string, targetMuscles: string[]): Exercise =>
    ({ id, name, description: "", type: "Strength", targetMuscles });

const info: Record<string, Exercise> = {
    row: exercise("row", "Barbell Row", ["Mid back"]),
    curl: exercise("curl", "Barbell Curl", ["Biceps"]),
    pulldown: exercise("pulldown", "Lat Pulldown", ["Lats"]),
    pullover: exercise("pullover", "Pullover", ["Lats", "Chest"]),
    shrug: exercise("shrug", "Shrug", ["Traps"]),
    custom: exercise("custom", "Neck Curl", ["Neck"]),
    twoArm: exercise("twoArm", "Curl to Press", ["Biceps", "Triceps"]),
    general: exercise("general", "Back Day Thing", ["Back"]),
    old: exercise("old", "Old Import", ["Upper back", "Quadriceps"]),
    bench: exercise("bench", "Bench Press", ["Chest", "Triceps", "Front delts"]),
    tRow: exercise("tRow", "T-bar Row", ["Mid back", "Lats", "Biceps", "Rear delts", "Lower back"]),
};

describe("summarizeSession", () => {
    it("counts working sets and volume, leaving warm-ups out", () => {
        const s = summarizeSession("Pull", "2026-11-10T18:00:00Z", [
            ex("row", [set(5, 20, { marker: "WarmUp" }), set(8, 60), set(8, 60)]),
            ex("curl", [set(10, 20)]),
        ], info, []);
        expect(s.workingSets).toBe(3);
        expect(s.strengthExercises).toBe(2);
        expect(s.volumeKg).toBe(8 * 60 * 2 + 10 * 20);
    });

    it("adds up sets per muscle, most first", () => {
        const s = summarizeSession("Pull", "2026-11-10T18:00:00Z", [
            ex("curl", [set(10, 20)]),
            ex("row", [set(8, 60), set(8, 60)]),
            ex("pulldown", [set(10, 50)]),
            ex("general", [set(10, 50)]),
        ], info, []);
        expect(s.muscles).toEqual([
            { muscle: "Mid back", sets: 2 }, { muscle: "Back", sets: 1 }, { muscle: "Biceps", sets: 1 }, { muscle: "Lats", sets: 1 },
        ]);
        // The area tagged on its own counts for the area, as its "general" part.
        expect(s.areas[0]).toMatchObject({ area: "Back", sets: 4 });
    });

    it("compares with the last earlier session of the same name", () => {
        const history = [
            log("a", "pull ", "2026-11-01T18:00:00Z", [ex("row", [set(8, 50)])]),
            log("b", "Push", "2026-11-05T18:00:00Z", [ex("row", [set(8, 70)])]),
            log("c", "Pull", "2026-11-20T18:00:00Z", [ex("row", [set(8, 90)])]), // later: ignored
        ];
        const s = summarizeSession("Pull", "2026-11-10T18:00:00Z", [ex("row", [set(8, 60)])], info, history);
        expect(s.previous).toMatchObject({ loggedAt: "2026-11-01T18:00:00Z", volumeKg: 400 });
    });

    it("marks a heavier working set than ever before as a record, but not a first time", () => {
        const history = [log("a", "Pull", "2026-11-01T18:00:00Z", [ex("row", [set(8, 60), set(3, 100, { marker: "WarmUp" })])])];
        const s = summarizeSession("Pull", "2026-11-10T18:00:00Z", [
            ex("row", [set(6, 65)]),
            ex("curl", [set(10, 30)]),
        ], info, history);
        expect(s.records).toEqual([{ exerciseId: "row", name: "Barbell Row", weightKg: 65, previousKg: 60 }]);
    });

    it("totals cardio distance and time", () => {
        const s = summarizeSession("Run", "2026-11-10T18:00:00Z", [
            ex("run", [{ ...set(0, 0), type: "Cardio", reps: null, weight: null, distance: 5, distanceUnit: "km", duration: "00:25:30" }]),
        ], info, []);
        expect(s.distanceKm).toBe(5);
        expect(s.minutes).toBeCloseTo(25.5);
        expect(s.workingSets).toBe(0);
    });

    it("groups muscles into areas, parts inside, counting an exercise once per area", () => {
        const s = summarizeSession("Pull", "2026-11-10T18:00:00Z", [
            ex("row", [set(8, 60), set(8, 60)]),
            ex("pullover", [set(10, 20)]),
            ex("shrug", [set(12, 40), set(12, 40)]),
            ex("twoArm", [set(10, 10), set(10, 10)]),
            ex("custom", [set(15, 5)]),
        ], info, []);
        expect(s.areas.map(a => [a.area, a.sets])).toEqual([["Back", 5], ["Arms", 2], ["Chest", 1], ["Neck", 1]]);
        expect(s.areas[0].parts).toEqual([{ muscle: "Mid back", sets: 2 }, { muscle: "Traps", sets: 2 }, { muscle: "Lats", sets: 1 }]);
        // Biceps and triceps from one exercise: 2 sets for Arms, not 4.
        expect(s.areas[1].parts.map(p => p.sets)).toEqual([2, 2]);
    });

    it("counts every listed muscle fully, and each area once", () => {
        const s = summarizeSession("Push", "2026-11-10T18:00:00Z", [
            ex("bench", [set(5, 100), set(5, 100), set(5, 100)]),
        ], info, []);
        expect(s.muscles).toEqual([{ muscle: "Chest", sets: 3 }, { muscle: "Front delts", sets: 3 }, { muscle: "Triceps", sets: 3 }]);
        expect(s.areas.map(a => [a.area, a.sets])).toEqual([["Chest", 3], ["Shoulders", 3], ["Arms", 3]]);
        expect(s.workingSets).toBe(3);
    });

    it("counts an area once even when the exercise lists several muscles in it", () => {
        const s = summarizeSession("Pull", "2026-11-10T18:00:00Z", [ex("tRow", [set(8, 60), set(8, 60)])], info, []);
        expect(s.areas.map(a => [a.area, a.sets])).toEqual([["Back", 2], ["Shoulders", 2], ["Arms", 2]]);
        expect(s.areas[0].parts).toEqual([{ muscle: "Lats", sets: 2 }, { muscle: "Lower back", sets: 2 }, { muscle: "Mid back", sets: 2 }]);
    });

    it("reads older names through the tree", () => {
        const s = summarizeSession("Mixed", "2026-11-10T18:00:00Z", [ex("old", [set(8, 60)])], info, []);
        expect(s.muscles).toEqual([{ muscle: "Mid back", sets: 1 }, { muscle: "Quads", sets: 1 }]);
        expect(s.areas.map(a => a.area)).toEqual(["Back", "Legs"]);
    });
});
