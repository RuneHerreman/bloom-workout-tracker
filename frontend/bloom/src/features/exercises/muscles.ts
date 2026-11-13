/**
 * The muscle tree, as the API's MuscleTree has it: body areas top to bottom, each
 * with the finer muscles in it. An exercise lists an area on its own ("Back": the
 * area in general) or muscles in it ("Lats"); a listed muscle implies its area.
 * A name outside the tree (a custom exercise's) is an area of its own.
 */
export const MUSCLE_TREE: { area: string; muscles: string[] }[] = [
    { area: "Chest", muscles: [] },
    { area: "Back", muscles: ["Lats", "Traps", "Mid back", "Lower back"] },
    { area: "Shoulders", muscles: ["Front delts", "Side delts", "Rear delts"] },
    { area: "Arms", muscles: ["Biceps", "Triceps", "Forearms"] },
    { area: "Core", muscles: ["Abs", "Obliques"] },
    { area: "Legs", muscles: ["Quads", "Hamstrings", "Glutes", "Adductors", "Abductors", "Calves"] },
];

/** What a muscle is for: shown under its name in the picker. */
export const MUSCLE_HINTS: Record<string, string> = {
    "Mid back": "Rhomboids, middle and lower traps",
    "Lower back": "Spinal erectors",
    "Traps": "Upper traps",
};

/** Older names (exports, imports from other apps), mapped onto the tree. */
const ALIASES: Record<string, string> = {
    quadriceps: "Quads",
    "upper chest": "Chest",
    "lower chest": "Chest",
    "upper back": "Mid back",
    rhomboids: "Mid back",
    trapezius: "Traps",
};

const AREA_BY_NAME = new Map<string, string>(
    MUSCLE_TREE.flatMap(({ area, muscles }) => [[area.toLowerCase(), area], ...muscles.map(m => [m.toLowerCase(), area] as [string, string])]),
);
const SPELLING = new Map<string, string>(
    MUSCLE_TREE.flatMap(({ area, muscles }) => [area, ...muscles]).map(n => [n.toLowerCase(), n]),
);

/** The tree's spelling of a name; a name outside the tree comes back trimmed. */
export function canonicalMuscle(name: string): string {
    const key = name.trim().toLowerCase();
    return ALIASES[key] ?? SPELLING.get(key) ?? name.trim();
}

/** The area a name belongs to; a name outside the tree is its own area. */
export function areaOf(name: string): string {
    const c = canonicalMuscle(name);
    return AREA_BY_NAME.get(c.toLowerCase()) ?? c;
}

export const isArea = (name: string) => MUSCLE_TREE.some(a => a.area === canonicalMuscle(name));
export const inTree = (name: string) => AREA_BY_NAME.has(canonicalMuscle(name).toLowerCase());

/** Where an area sits top to bottom; names outside the tree go last. */
export function areaOrder(area: string): number {
    const i = MUSCLE_TREE.findIndex(a => a.area === area);
    return i < 0 ? MUSCLE_TREE.length : i;
}

/**
 * Tidies a list: tree spelling, no duplicates, and an area dropped when a muscle in
 * it is listed (the muscle already implies it).
 */
export function normalizeMuscles(names: string[]): string[] {
    const list: string[] = [];
    for (const n of names.map(canonicalMuscle)) if (n && !list.includes(n)) list.push(n);
    return list.filter(n => !isArea(n) || !list.some(o => !isArea(o) && areaOf(o) === n));
}

/** The muscles an exercise lists, tidied. Each gets a full working set. */
export const musclesOf = (ex: { targetMuscles: string[] } | undefined): string[] =>
    ex ? normalizeMuscles(ex.targetMuscles) : [];

/** The areas an exercise touches, each once. */
export const areasOf = (ex: { targetMuscles: string[] } | undefined): string[] =>
    [...new Set(musclesOf(ex).map(areaOf))];

/**
 * True when work listed as `trained` counts for `name`: the same name, a muscle in
 * that area, or that muscle's area in general.
 */
export function covers(trained: string, name: string): boolean {
    const t = canonicalMuscle(trained), n = canonicalMuscle(name);
    return t === n || (isArea(n) && areaOf(t) === n) || (isArea(t) && areaOf(n) === t);
}
