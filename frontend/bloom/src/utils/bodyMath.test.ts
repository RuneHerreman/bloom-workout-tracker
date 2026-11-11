import { describe, expect, it } from "vitest";
import {
    activityFromSessions, ageInYears, bodyComposition, bodyFatRatePerMonth, bmrKatch, bmrMifflin, calorieTarget, e1rm, estimateOneRepMax,
    macroTargets, roundDownToStep, weeklyRate, weightForReps, weightTrend, type WeighIn,
} from "./bodyMath.ts";

describe("estimateOneRepMax", () => {
    it.each([
        // weight, reps, epley, brzycki
        [100, 5, 116.667, 112.5],
        [100, 10, 133.333, 133.333],
        [80, 8, 101.333, 99.310],
        [60, 3, 66, 63.529],
    ])("%d kg × %d matches the published Epley and Brzycki values", (w, r, epley, brzycki) => {
        const est = estimateOneRepMax(w, r)!;
        expect(est.epley).toBeCloseTo(epley, 2);
        expect(est.brzycki).toBeCloseTo(brzycki, 2);
        expect(est.estimate).toBeCloseTo((epley + brzycki) / 2, 2);
        expect(est.low).toBeLessThanOrEqual(est.estimate);
        expect(est.high).toBeGreaterThanOrEqual(est.estimate);
    });

    it("treats a single rep as the max itself", () => {
        expect(estimateOneRepMax(140, 1)).toMatchObject({ estimate: 140, low: 140, high: 140, reliability: "exact" });
    });

    it("counts reps in reserve as extra reps", () => {
        expect(estimateOneRepMax(100, 5, 2)!.estimate).toBeCloseTo(estimateOneRepMax(100, 7)!.estimate, 10);
        expect(estimateOneRepMax(100, 5, null)!.estimate).toBeCloseTo(estimateOneRepMax(100, 5)!.estimate, 10);
        expect(estimateOneRepMax(100, 5, -3)!.estimate).toBeCloseTo(estimateOneRepMax(100, 5)!.estimate, 10);
    });

    it.each([
        [5, "good"], [10, "good"], [11, "rough"], [15, "rough"],
    ])("%d reps to failure is %s", (r, reliability) => {
        expect(estimateOneRepMax(100, r as number)!.reliability).toBe(reliability);
    });

    it.each([
        [0, 5], [100, 0], [-20, 5], [100, 16], [NaN, 5],
    ])("gives no estimate for %d kg × %d", (w, r) => {
        expect(estimateOneRepMax(w, r)).toBeNull();
    });
});

describe("e1rm", () => {
    it("matches the blended estimate", () => {
        expect(e1rm(100, 5, 1)).toBeCloseTo(estimateOneRepMax(100, 5, 1)!.estimate, 10);
    });

    it("scores sets past 15 reps as 15, so high-rep lifts still chart", () => {
        expect(e1rm(40, 25)).toBeCloseTo(e1rm(40, 15), 10);
    });

    it("is 0 for empty sets", () => {
        expect(e1rm(0, 5)).toBe(0);
        expect(e1rm(100, 0)).toBe(0);
    });

    it("never goes down as reps go up at the same weight", () => {
        for (let r = 1; r < 15; r++) expect(e1rm(100, r + 1)).toBeGreaterThan(e1rm(100, r));
    });
});

describe("weightForReps", () => {
    it.each([
        [100, 5, 0], [80, 8, 1], [140, 3, 2], [60, 1, 0], [100, 12, 0],
    ])("undoes e1rm for %d kg × %d @ %d RIR", (w, r, rir) => {
        expect(weightForReps(e1rm(w, r, rir), r, rir)).toBeCloseTo(w, 9);
    });

    it("gives the max itself for one rep at 0 RIR", () => {
        expect(weightForReps(120, 1)).toBe(120);
    });

    it("is 0 when the reps can't be estimated", () => {
        expect(weightForReps(100, 14, 3)).toBe(0);
        expect(weightForReps(0, 5)).toBe(0);
    });
});

describe("roundDownToStep", () => {
    it.each([
        [81.9, 2.5, 80], [80, 2.5, 80], [80 - 1e-12, 2.5, 80], [79.9, 2.5, 77.5], [82.4, 1.25, 81.25], [0.5, 2.5, 0],
    ])("%d to a %d step is %d", (kg, step, out) => {
        expect(roundDownToStep(kg, step)).toBeCloseTo(out, 9);
    });
});

describe("ageInYears", () => {
    it.each([
        [[1996, 11, 12], [2026, 11, 11], 29],
        [[1996, 11, 12], [2026, 11, 12], 30],
        [[1996, 2, 29], [2026, 2, 28], 29],
        [[1996, 2, 29], [2026, 3, 1], 30],
    ] as const)("born %j, on %j is %d", (birth, on, age) => {
        expect(ageInYears([...birth], [...on])).toBe(age);
    });
});

describe("resting energy", () => {
    it.each([
        // weight, height, age, sex, kcal (worked by hand from the equation)
        [80, 180, 30, "male", 1780],
        [60, 165, 25, "female", 1345.25],
        [100, 190, 45, "male", 1967.5],
    ] as const)("Mifflin-St Jeor: %d kg, %d cm, %d y, %s", (weightKg, heightCm, age, sex, kcal) => {
        expect(bmrMifflin({ weightKg, heightCm, age, sex })).toBeCloseTo(kcal, 6);
    });

    it("Katch-McArdle uses lean mass", () => {
        expect(bmrKatch({ weightKg: 80, bodyFatPct: 15 })).toBeCloseTo(370 + 21.6 * 68, 6);
        expect(bmrKatch({ weightKg: 80, bodyFatPct: 0 })).toBeCloseTo(370 + 21.6 * 80, 6);
    });
});

describe("activityFromSessions", () => {
    it.each([
        [0, "sedentary"], [1, "light"], [2, "light"], [3, "moderate"], [4, "moderate"], [5, "very"], [9, "very"],
    ])("%d sessions a week is %s", (n, key) => {
        expect(activityFromSessions(n)).toBe(key);
    });
});

describe("calorieTarget", () => {
    const base = { tdee: 2700, bmr: 1780, weightKg: 80 };

    it("maintenance is the TDEE", () => {
        expect(calorieTarget({ ...base, ratePctPerWeek: 0 })).toEqual({ kcal: 2700, delta: 0, capped: false });
    });

    it("0.5% a week at 80 kg is a 440 kcal deficit", () => {
        const t = calorieTarget({ ...base, ratePctPerWeek: -0.5 });
        expect(t.delta).toBeCloseTo(-440, 6);
        expect(t.kcal).toBe(2260);
        expect(t.capped).toBe(false);
    });

    it("a gain rate adds a surplus", () => {
        expect(calorieTarget({ ...base, ratePctPerWeek: 0.25 }).kcal).toBe(2920);
    });

    it("never goes below 1.1 × resting energy", () => {
        const t = calorieTarget({ ...base, ratePctPerWeek: -1 });
        expect(t.capped).toBe(true);
        expect(t.kcal).toBe(1960);
    });
});

describe("macroTargets", () => {
    it.each([
        [2260, 80, "lose"], [2700, 80, "maintain"], [2920, 80, "gain"], [1960, 55, "lose"], [3600, 110, "gain"],
    ] as const)("%d kcal at %d kg (%s) adds back up to the target", (kcal, weightKg, goal) => {
        const m = macroTargets({ kcal, weightKg, goal });
        expect(Math.abs(m.proteinG * 4 + m.fatG * 9 + m.carbsG * 4 - kcal)).toBeLessThanOrEqual(10);
        for (const g of [m.proteinG, m.fatG, m.carbsG]) expect(g % 5).toBe(0);
    });

    it("puts protein first, by goal", () => {
        expect(macroTargets({ kcal: 2260, weightKg: 80, goal: "lose" }).proteinG).toBe(175);
        expect(macroTargets({ kcal: 2700, weightKg: 80, goal: "maintain" }).proteinG).toBe(130);
        expect(macroTargets({ kcal: 2700, weightKg: 80, goal: "maintain", proteinPerKg: 2 }).proteinG).toBe(160);
    });

    it("keeps fat at 25% of kcal, but never under 0.6 g/kg", () => {
        expect(macroTargets({ kcal: 2700, weightKg: 80, goal: "maintain" }).fatG).toBe(75);
        // 25% of 1200 kcal is 33 g, under 0.6 × 100 kg = 60 g.
        expect(macroTargets({ kcal: 1200, weightKg: 100, goal: "lose" }).fatG).toBe(60);
    });

    it("flags low carbs and never goes negative", () => {
        const m = macroTargets({ kcal: 1200, weightKg: 100, goal: "lose" });
        expect(m.carbsG).toBe(0);
        expect(m.lowCarb).toBe(true);
        expect(macroTargets({ kcal: 2700, weightKg: 80, goal: "maintain" }).lowCarb).toBe(false);
    });
});

describe("weightTrend", () => {
    const daily = (kgs: number[]): WeighIn[] => kgs.map((kg, day) => ({ day, kg }));

    it("is flat for a constant weight", () => {
        expect(weightTrend(daily([80, 80, 80, 80]))).toEqual([80, 80, 80, 80]);
    });

    it("moves a tenth of the way toward a one-off spike", () => {
        expect(weightTrend(daily([80, 80, 83])).at(-1)).toBeCloseTo(80.3, 9);
    });

    it("treats a gap of N days as N daily steps", () => {
        const gap = weightTrend([{ day: 0, kg: 80 }, { day: 5, kg: 85 }]).at(-1)!;
        let stepped = 80;
        for (let i = 0; i < 5; i++) stepped += 0.1 * (85 - stepped);
        expect(gap).toBeCloseTo(stepped, 9);
    });

    it("sorts weigh-ins by day", () => {
        expect(weightTrend([{ day: 1, kg: 83 }, { day: 0, kg: 80 }])).toEqual(weightTrend(daily([80, 83])));
    });

    it("is empty for no weigh-ins", () => {
        expect(weightTrend([])).toEqual([]);
    });
});

describe("weeklyRate", () => {
    const line = (days: number, perDay: number, start = 80): WeighIn[] =>
        Array.from({ length: days }, (_, day) => ({ day, kg: start + perDay * day }));

    it("finds −0.7 kg/week on a steady −0.1 kg/day", () => {
        const r = weeklyRate(line(21, -0.1))!;
        expect(r.kgPerWeek).toBeCloseTo(-0.7, 9);
        expect(r.pctPerWeek).toBeCloseTo((-0.7 / 79) * 100, 9); // mean weight over the line is 79 kg
    });

    it("is 0 for a flat weight", () => {
        expect(weeklyRate(line(20, 0))!.kgPerWeek).toBeCloseTo(0, 9);
    });

    it("sees through noise on the way down", () => {
        const noise = [0.6, -0.4, 0.2, -0.7, 0.5, -0.1, 0.3, -0.5, 0.4, -0.3];
        const pts = Array.from({ length: 30 }, (_, day) => ({ day, kg: 85 - 0.05 * day + noise[day % noise.length] }));
        expect(weeklyRate(pts)!.kgPerWeek).toBeCloseTo(-0.35, 1);
    });

    it("waits for at least 6 weigh-ins over 14 days", () => {
        expect(weeklyRate(line(5, -0.1).map(p => ({ ...p, day: p.day * 5 })))).toBeNull(); // 5 entries, 20 days
        expect(weeklyRate(line(14, -0.1))).toBeNull(); // 14 entries, 13 days
        expect(weeklyRate(line(15, -0.1))).not.toBeNull();
    });
});

describe("body fat", () => {
    it("rate is in points per month and needs 4 readings over 28 days", () => {
        const weekly = (n: number) => Array.from({ length: n }, (_, i) => ({ day: i * 7, kg: 20 - 0.25 * i }));
        expect(bodyFatRatePerMonth(weekly(4))).toBeNull(); // 21 days
        expect(bodyFatRatePerMonth(weekly(5))).toBeCloseTo((-0.25 / 7) * 30, 9);
        expect(bodyFatRatePerMonth([{ day: 0, kg: 20 }, { day: 40, kg: 19 }, { day: 60, kg: 18 }])).toBeNull(); // 3 readings
    });

    it("splits weight into lean and fat mass", () => {
        const c = bodyComposition(80, 20);
        expect(c.fatKg).toBeCloseTo(16, 9);
        expect(c.leanKg).toBeCloseTo(64, 9);
    });
});
