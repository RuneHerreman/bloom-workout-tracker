import { describe, expect, it } from "vitest";
import { macroPlan, type MacroInputs } from "./macroPlan.ts";

const base: MacroInputs = {
    weightKg: 80, heightCm: 180, age: 30, sex: "male", bodyFatPct: null,
    activity: "moderate", goal: "maintain", ratePct: 0, proteinPerKg: 1.6,
};

describe("macroPlan", () => {
    it("uses Mifflin-St Jeor without a body fat reading", () => {
        const p = macroPlan(base)!;
        expect(p.formula).toBe("mifflin");
        expect(p.bmr).toBeCloseTo(1780, 6); // 800 + 1125 − 150 + 5
        expect(p.maintenance).toBe(2760); // 1780 × 1.55 = 2759
        expect(p.kcal).toBe(2760);
        expect(p.kgPerWeek).toBe(0);
    });

    it("uses Katch-McArdle with a body fat reading, and then doesn't need sex", () => {
        const p = macroPlan({ ...base, sex: null, bodyFatPct: 15 })!;
        expect(p.formula).toBe("katch");
        expect(p.bmr).toBeCloseTo(370 + 21.6 * 68, 6);
    });

    it("needs sex when there's no body fat reading", () => {
        expect(macroPlan({ ...base, sex: null })).toBeNull();
    });

    it("is null for missing measurements", () => {
        expect(macroPlan({ ...base, weightKg: 0 })).toBeNull();
        expect(macroPlan({ ...base, heightCm: 0 })).toBeNull();
        expect(macroPlan({ ...base, age: 0 })).toBeNull();
    });

    it("subtracts for losing and adds for gaining, by % of body weight per week", () => {
        const lose = macroPlan({ ...base, goal: "lose", ratePct: 0.5, proteinPerKg: 2.2 })!;
        expect(lose.kgPerWeek).toBeCloseTo(-0.4, 9);
        expect(lose.kcal).toBe(2320); // 2759 − 440, rounded
        const gain = macroPlan({ ...base, goal: "gain", ratePct: 0.25 })!;
        expect(gain.kcal).toBe(2980); // 2759 + 220
    });

    it("macros add back up to the target", () => {
        for (const goal of ["lose", "maintain", "gain"] as const) {
            const p = macroPlan({ ...base, goal, ratePct: goal === "maintain" ? 0 : 0.5 })!;
            const { proteinG, fatG, carbsG } = p.macros;
            expect(Math.abs(proteinG * 4 + fatG * 9 + carbsG * 4 - p.kcal)).toBeLessThanOrEqual(10);
        }
    });

    it("caps a steep deficit at the floor", () => {
        const p = macroPlan({ ...base, activity: "sedentary", goal: "lose", ratePct: 1 })!;
        expect(p.capped).toBe(true);
        expect(p.kcal).toBe(1960); // 1780 × 1.1
    });
});
