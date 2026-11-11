// Every training and body formula in Bloom lives here, so Insights, the dashboard
// and the calculators can never disagree about the same number. Pure functions:
// plain numbers in, plain numbers out, no React and no Date parsing.
//
// Sources and reasoning: docs/plans/body-weight-and-calculators.md

// ── Rounding ──────────────────────────────────────────────────────────────────

export const round1 = (n: number) => Math.round(n * 10) / 10;
export const roundTo = (n: number, step: number) => Math.round(n / step) * step;

/** Down to the nearest plate step: the safe side for a working weight. */
export function roundDownToStep(kg: number, step = 2.5): number {
    if (step <= 0) return kg;
    // The epsilon keeps 80 from becoming 77.5 when float maths gives 79.99999.
    return Math.floor(kg / step + 1e-9) * step;
}

// ── One rep max ───────────────────────────────────────────────────────────────

/** Up to this many reps to failure, the estimate is reliable. */
export const RELIABLE_REPS = 10;
/** Above this many reps to failure, no estimate is given at all. */
export const MAX_REPS = 15;

export type Reliability = "exact" | "good" | "rough";

export interface OneRepMaxEstimate {
    /** Blended estimate (mean of Epley and Brzycki). */
    estimate: number;
    epley: number;
    brzycki: number;
    /** Lower and upper of the two formulas. */
    low: number;
    high: number;
    /** Reps plus reps in reserve: what the formulas actually use. */
    repsToFailure: number;
    reliability: Reliability;
}

const epleyFactor = (r: number) => 1 + r / 30;
const brzyckiFactor = (r: number) => 36 / (37 - r);
/** Multiplier from a set of r reps to failure to its 1RM. One rep is the 1RM itself. */
const blendFactor = (r: number) => (r <= 1 ? 1 : (epleyFactor(r) + brzyckiFactor(r)) / 2);

/** Reps to failure for a set: reps plus RIR. A set without RIR counts as taken to failure. */
export function repsToFailure(reps: number, rir?: number | null): number {
    return reps + Math.max(0, rir ?? 0);
}

/**
 * Estimated one rep max from a set. Returns null when the set can't give one:
 * no weight, no reps, or more than MAX_REPS reps to failure.
 */
export function estimateOneRepMax(weightKg: number, reps: number, rir?: number | null): OneRepMaxEstimate | null {
    if (!(weightKg > 0) || !(reps > 0)) return null;
    const r = repsToFailure(reps, rir);
    if (r > MAX_REPS) return null;
    if (r <= 1) {
        return { estimate: weightKg, epley: weightKg, brzycki: weightKg, low: weightKg, high: weightKg, repsToFailure: r, reliability: "exact" };
    }
    const epley = weightKg * epleyFactor(r);
    const brzycki = weightKg * brzyckiFactor(r);
    return {
        estimate: weightKg * blendFactor(r),
        epley,
        brzycki,
        low: Math.min(epley, brzycki),
        high: Math.max(epley, brzycki),
        repsToFailure: r,
        reliability: r <= RELIABLE_REPS ? "good" : "rough",
    };
}

/**
 * The blended 1RM as a plain number, for charts and records. Sets past MAX_REPS
 * are scored at MAX_REPS, so a high-rep exercise still gets a (conservative) line.
 */
export function e1rm(weightKg: number, reps: number, rir?: number | null): number {
    if (!(weightKg > 0) || !(reps > 0)) return 0;
    return weightKg * blendFactor(Math.min(repsToFailure(reps, rir), MAX_REPS));
}

/** The weight that should allow `reps` reps with `rir` left, from a 1RM. Exact inverse of e1rm. */
export function weightForReps(oneRepMaxKg: number, reps: number, rir = 0): number {
    if (!(oneRepMaxKg > 0) || !(reps > 0)) return 0;
    const r = repsToFailure(reps, rir);
    if (r > MAX_REPS) return 0;
    return oneRepMaxKg / blendFactor(r);
}

// ── Energy ────────────────────────────────────────────────────────────────────

export type Sex = "male" | "female";

/** Whole years between a birth date and a day, both as [year, month (1-12), day]. */
export function ageInYears(birth: [number, number, number], on: [number, number, number]): number {
    const [by, bm, bd] = birth, [y, m, d] = on;
    let age = y - by;
    if (m < bm || (m === bm && d < bd)) age--;
    return age;
}

/** Mifflin-St Jeor resting energy, kcal/day. */
export function bmrMifflin(p: { weightKg: number; heightCm: number; age: number; sex: Sex }): number {
    return 10 * p.weightKg + 6.25 * p.heightCm - 5 * p.age + (p.sex === "male" ? 5 : -161);
}

/** Katch-McArdle resting energy from lean mass, kcal/day. Body fat as a percentage (18, not 0.18). */
export function bmrKatch(p: { weightKg: number; bodyFatPct: number }): number {
    const lean = p.weightKg * (1 - p.bodyFatPct / 100);
    return 370 + 21.6 * lean;
}

export const ACTIVITY_LEVELS = [
    { key: "sedentary", label: "Sedentary", factor: 1.2 },
    { key: "light", label: "Light", factor: 1.375 },
    { key: "moderate", label: "Moderate", factor: 1.55 },
    { key: "very", label: "Very active", factor: 1.725 },
    { key: "extra", label: "Extra active", factor: 1.9 },
] as const;

export type ActivityKey = (typeof ACTIVITY_LEVELS)[number]["key"];

/** Default activity level from logged training sessions per week. "Extra" is only ever a manual pick. */
export function activityFromSessions(sessionsPerWeek: number): ActivityKey {
    if (sessionsPerWeek < 0.5) return "sedentary";
    if (sessionsPerWeek < 2.5) return "light";
    if (sessionsPerWeek < 4.5) return "moderate";
    return "very";
}

export function activityFactor(key: ActivityKey): number {
    return ACTIVITY_LEVELS.find(a => a.key === key)!.factor;
}

/** Energy in one kg of body weight change. Fine for weekly targets, not for long-term projections. */
export const KCAL_PER_KG = 7700;
/** The target never drops below resting energy times this. */
export const MIN_INTAKE_FACTOR = 1.1;

export interface CalorieTarget {
    /** Rounded to 10 kcal. */
    kcal: number;
    /** Change from maintenance, kcal/day (negative = deficit), before the floor. */
    delta: number;
    /** The chosen rate would have gone below the floor. */
    capped: boolean;
}

/** Daily target for a rate of change in % body weight per week (−0.5 = lose half a percent a week). */
export function calorieTarget(p: { tdee: number; bmr: number; weightKg: number; ratePctPerWeek: number }): CalorieTarget {
    const kgPerWeek = (p.weightKg * p.ratePctPerWeek) / 100;
    const delta = (kgPerWeek * KCAL_PER_KG) / 7;
    const floor = p.bmr * MIN_INTAKE_FACTOR;
    const raw = p.tdee + delta;
    const capped = raw < floor;
    return { kcal: roundTo(capped ? floor : raw, 10), delta, capped };
}

export type Goal = "lose" | "maintain" | "gain";

export const PROTEIN_DEFAULT: Record<Goal, number> = { lose: 2.2, maintain: 1.6, gain: 1.6 };
const FAT_SHARE = 0.25;
const FAT_MIN_PER_KG = 0.6;
const LOW_CARB_PER_KG = 2;

export interface MacroTargets {
    proteinG: number;
    fatG: number;
    carbsG: number;
    /** Carbs ended up under 2 g/kg. */
    lowCarb: boolean;
}

/**
 * Grams per day: protein first (g/kg), then fat (25% of kcal, at least 0.6 g/kg),
 * carbs take the rest. Protein and fat round to 5 g; carbs are worked out from the
 * rounded values so the three still add up to the target.
 */
export function macroTargets(p: { kcal: number; weightKg: number; goal: Goal; proteinPerKg?: number }): MacroTargets {
    const proteinG = roundTo(p.weightKg * (p.proteinPerKg ?? PROTEIN_DEFAULT[p.goal]), 5);
    const fatG = roundTo(Math.max((p.kcal * FAT_SHARE) / 9, p.weightKg * FAT_MIN_PER_KG), 5);
    const carbsG = Math.max(0, roundTo((p.kcal - proteinG * 4 - fatG * 9) / 4, 5));
    return { proteinG, fatG, carbsG, lowCarb: carbsG < p.weightKg * LOW_CARB_PER_KG };
}

// ── Body weight trend ─────────────────────────────────────────────────────────

/** A weigh-in. `day` is any whole-day index (days since an epoch), so gaps are just differences. */
export interface WeighIn { day: number; kg: number }

/** Share of a new reading that goes into the trend (≈ 20-day lag). */
export const TREND_ALPHA = 0.1;

/**
 * Exponentially smoothed trend, one value per weigh-in. A gap of N days counts as N
 * daily steps toward the new reading, so a week away doesn't freeze the trend.
 */
export function weightTrend(points: WeighIn[], alpha = TREND_ALPHA): number[] {
    const sorted = [...points].sort((a, b) => a.day - b.day);
    const out: number[] = [];
    let trend = 0, last = 0;
    sorted.forEach((p, i) => {
        if (i === 0) trend = p.kg;
        else {
            const gap = Math.max(1, p.day - last);
            trend += (1 - (1 - alpha) ** gap) * (p.kg - trend);
        }
        last = p.day;
        out.push(trend);
    });
    return out;
}

export const RATE_MIN_DAYS = 14;
export const RATE_MIN_ENTRIES = 6;

export interface WeightRate { kgPerWeek: number; pctPerWeek: number }

/** Body fat is sparser and noisier than weight: a slower trend and a stricter bar for a rate. */
export const BODY_FAT_TREND_ALPHA = 0.05;
export const BODY_FAT_RATE_MIN = { entries: 4, days: 28 };

/**
 * Body fat change in percentage points per month (30 days), or null when there
 * aren't enough readings. `kg` on each point holds the body fat %.
 */
export function bodyFatRatePerMonth(points: WeighIn[]): number | null {
    const r = weeklyRate(points, BODY_FAT_RATE_MIN);
    return r ? (r.kgPerWeek / 7) * 30 : null;
}

/** Lean and fat mass in kg from a weight and a body fat %. */
export function bodyComposition(weightKg: number, bodyFatPct: number): { leanKg: number; fatKg: number } {
    const fatKg = (weightKg * bodyFatPct) / 100;
    return { leanKg: weightKg - fatKg, fatKg };
}

/**
 * Weekly rate of change: least-squares slope through the weigh-ins. Null until there
 * are at least RATE_MIN_ENTRIES weigh-ins spanning RATE_MIN_DAYS days, since a slope
 * through a handful of points is mostly water.
 */
export function weeklyRate(points: WeighIn[], min: { entries: number; days: number } = { entries: RATE_MIN_ENTRIES, days: RATE_MIN_DAYS }): WeightRate | null {
    if (points.length < min.entries) return null;
    const days = points.map(p => p.day);
    if (Math.max(...days) - Math.min(...days) < min.days) return null;
    const n = points.length;
    const mx = days.reduce((a, b) => a + b, 0) / n;
    const my = points.reduce((a, p) => a + p.kg, 0) / n;
    let num = 0, den = 0;
    for (const p of points) { num += (p.day - mx) * (p.kg - my); den += (p.day - mx) ** 2; }
    const kgPerWeek = (num / den) * 7;
    return { kgPerWeek, pctPerWeek: (kgPerWeek / my) * 100 };
}
