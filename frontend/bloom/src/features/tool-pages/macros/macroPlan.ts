// Puts the energy formulas together into one daily target: resting energy
// (Katch-McArdle with a body fat reading, Mifflin-St Jeor otherwise), times
// activity, plus or minus the chosen rate, then protein, fat and carbs.

import {
    activityFactor, bmrKatch, bmrMifflin, calorieTarget, macroTargets, roundTo,
    type ActivityKey, type Goal, type MacroTargets, type Sex,
} from "../../../utils/bodyMath.ts";

export const RATES: Record<Goal, number[]> = {
    lose: [0.25, 0.5, 0.75, 1],
    maintain: [0],
    gain: [0.1, 0.25, 0.5],
};
export const DEFAULT_RATE: Record<Goal, number> = { lose: 0.5, maintain: 0, gain: 0.25 };

/** Protein options in g per kg of body weight. */
export const PROTEIN_OPTIONS = [1.4, 1.6, 1.8, 2.0, 2.2, 2.4];

export interface MacroInputs {
    weightKg: number;
    heightCm: number;
    age: number;
    sex: Sex | null;
    /** A body fat reading, or null to use Mifflin-St Jeor. */
    bodyFatPct: number | null;
    activity: ActivityKey;
    goal: Goal;
    /** % of body weight per week, always positive; the goal gives the direction. */
    ratePct: number;
    proteinPerKg: number;
}

export interface MacroPlan {
    formula: "katch" | "mifflin";
    bmr: number;
    /** Maintenance, rounded to 10 kcal. */
    maintenance: number;
    kcal: number;
    /** Planned change per week in kg (negative when losing). */
    kgPerWeek: number;
    /** The rate would have gone under the floor; kcal is the floor instead. */
    capped: boolean;
    macros: MacroTargets;
}

/** The plan, or null when a needed value is missing (sex, without a body fat reading). */
export function macroPlan(i: MacroInputs): MacroPlan | null {
    if (!(i.weightKg > 0) || !(i.heightCm > 0) || !(i.age > 0)) return null;
    const useKatch = i.bodyFatPct !== null && i.bodyFatPct > 0;
    if (!useKatch && !i.sex) return null;

    const bmr = useKatch
        ? bmrKatch({ weightKg: i.weightKg, bodyFatPct: i.bodyFatPct! })
        : bmrMifflin({ weightKg: i.weightKg, heightCm: i.heightCm, age: i.age, sex: i.sex! });
    const tdee = bmr * activityFactor(i.activity);
    const signed = i.goal === "lose" ? -i.ratePct : i.goal === "gain" ? i.ratePct : 0;
    const target = calorieTarget({ tdee, bmr, weightKg: i.weightKg, ratePctPerWeek: signed });
    const macros = macroTargets({ kcal: target.kcal, weightKg: i.weightKg, goal: i.goal, proteinPerKg: i.proteinPerKg });

    return {
        formula: useKatch ? "katch" : "mifflin",
        bmr,
        maintenance: roundTo(tdee, 10),
        kcal: target.kcal,
        kgPerWeek: (i.weightKg * signed) / 100,
        capped: target.capped,
        macros,
    };
}
