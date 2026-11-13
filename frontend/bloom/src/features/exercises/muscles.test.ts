import { describe, expect, it } from "vitest";
import { areaOf, covers, normalizeMuscles } from "./muscles.ts";

describe("muscle tree", () => {
    it("finds the area of a muscle, an area, an old name and a custom name", () => {
        expect(areaOf("Lats")).toBe("Back");
        expect(areaOf("Back")).toBe("Back");
        expect(areaOf("quadriceps")).toBe("Legs");
        expect(areaOf("Neck")).toBe("Neck");
    });

    it("tidies a list: tree spelling, no duplicates, no area next to its own muscle", () => {
        expect(normalizeMuscles(["back", "Lats", "Upper back", "Rhomboids", "Chest", "Neck"]))
            .toEqual(["Lats", "Mid back", "Chest", "Neck"]);
    });

    it("lets an area and its muscles stand in for each other", () => {
        expect(covers("Lats", "Back")).toBe(true);
        expect(covers("Back", "Lats")).toBe(true);
        expect(covers("Lats", "Traps")).toBe(false);
        expect(covers("Chest", "Back")).toBe(false);
    });
});
