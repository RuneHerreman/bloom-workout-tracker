import { describe, expect, it } from "vitest";
import { matchScore } from "./exerciseSearch.ts";

describe("matchScore", () => {
    it("understands gym shorthand", () => {
        expect(matchScore("Romanian Deadlift", "rdl")).toBe(1);
        expect(matchScore("Dumbbell Romanian Deadlift", "rdl")).toBeGreaterThan(1);
        expect(matchScore("Overhead Press", "ohp")).toBe(1);
        expect(matchScore("Single Arm Dumbbell Row", "db row")).toBeLessThan(2);
        expect(matchScore("T-bar Row", "tbar")).toBeLessThan(2);
        expect(matchScore("Push Up", "pushup")).toBe(1);
    });

    it("ranks a name that starts with the query first and loose letter matches last", () => {
        expect(matchScore("Squat", "squ")).toBe(0);
        expect(matchScore("Front Squat", "squ")).toBeLessThan(2);
        expect(matchScore("Hurdle Jump", "rdl")).toBe(2);
        expect(matchScore("Romanian Deadlift", "rdl")!).toBeLessThan(matchScore("Dumbbell Romanian Deadlift", "rdl")!);
        expect(matchScore("Bench Press", "rdl")).toBeNull();
    });
});
