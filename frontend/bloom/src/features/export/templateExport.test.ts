import { describe, it, expect } from "vitest";
import { buildTemplatesJson, templatesFilename } from "./exportData.ts";
import { parseImportFile } from "./importData.ts";
import type { Exercise, WorkoutTemplate } from "../../assets/js/data/apiTypes.ts";

const exercises: Record<string, Exercise> = {
    squat: { id: "squat", name: "Squat", description: "", type: "Strength", targetMuscles: ["Quads"] },
    mine: { id: "mine", name: "Pistol Squat", description: "", type: "Strength", isCustom: true, targetMuscles: ["Quads"] },
    other: { id: "other", name: "Unused Custom", description: "", type: "Strength", isCustom: true, targetMuscles: [] },
};
const set = (order: number, reps: number) => ({ type: "Strength" as const, order, reps, duration: null, distance: null, distanceUnit: null });
const legs: WorkoutTemplate = {
    id: "t1", userId: "u", name: "Legs Day!",
    exercises: [
        { exerciseId: "mine", order: 2, sets: [set(1, 8)] },
        { exerciseId: "squat", order: 1, note: "Belt on", sets: [set(1, 5), set(2, 5)] },
    ],
};

describe("buildTemplatesJson", () => {
    it("reads back through the importer, in order, with the custom exercises it uses", async () => {
        const blob = buildTemplatesJson([legs], exercises);
        const text = await blob.text();
        const json = JSON.parse(text);
        expect(json.customExercises.map((e: Exercise) => e.id)).toEqual(["mine"]);

        const parsed = await parseImportFile(new File([text], "t.json", { type: "application/json" }));
        expect(parsed.workouts).toEqual([]);
        expect(parsed.templates).toHaveLength(1);
        expect(parsed.templates[0]!.name).toBe("Legs Day!");
        expect(parsed.templates[0]!.exercises.map(e => e.ref.name)).toEqual(["Squat", "Pistol Squat"]);
        expect(parsed.templates[0]!.exercises[0]!.note).toBe("Belt on");
        expect(parsed.templates[0]!.exercises[0]!.sets).toHaveLength(2);
    });

    it("names the file after a single template", () => {
        const now = new Date(2026, 10, 18);
        expect(templatesFilename([legs], now)).toBe("bloom-legs-day-2026-11-18.json");
        expect(templatesFilename([legs, legs], now)).toBe("bloom-templates-2026-11-18.json");
    });
});
