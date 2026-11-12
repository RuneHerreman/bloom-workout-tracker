/**
 * A gear item's kind: picks its icon, and tells which items wear out with
 * distance. You can pick one per item; otherwise it's read from the name.
 * Keep in step with GearKinds on the server.
 */
export const GEAR_KINDS = ["shoe", "bike", "belt", "wrap", "watch", "swim", "weights", "racket", "other"] as const;
export type GearKind = (typeof GEAR_KINDS)[number];

export const KIND_LABELS: Record<GearKind, string> = {
    shoe: "Shoes", bike: "Bike", belt: "Belt", wrap: "Wraps & straps", watch: "Watch & HR",
    swim: "Swim", weights: "Weights", racket: "Racket", other: "Other",
};

const RULES: [GearKind, RegExp][] = [
    // Heart-rate straps before wraps and straps, watches before anything with "strap".
    ["watch", /\b(watch|garmin|forerunner|fenix|coros|polar|suunto|apple watch|heart|hrm|hr strap|chest strap|whoop|wahoo tickr|power ?meter)\b/i],
    ["shoe", /\b(shoes?|sneakers?|trainers?|runners?|spikes?|boots?|pegasus|vaporfly|alphafly|clifton|bondi|speedgoat|ghost|kayano|novablast|endorphin|cloud\w*|ultraboost|adizero|nimbus)\b/i],
    ["bike", /\b(bikes?|bicycles?|cycles?|cycling|canyon|trek|specialized|cervelo|cervélo|giant|cannondale|pinarello|bianchi|scott|bmc|turbo|trainer bike|zwift)\b/i],
    ["belt", /\bbelts?\b/i],
    ["wrap", /\b(wraps?|straps?|sleeves?|chalk|gloves?|grips?)\b/i],
    ["swim", /\b(swim\w*|goggles?|wetsuit|paddles?|fins?|pull ?buoy)\b/i],
    ["weights", /\b(dumbbells?|kettlebells?|barbells?|plates?|weights?|vest|bands?)\b/i],
    ["racket", /\b(rackets?|racquets?|padel|tennis|squash|badminton|bats?|clubs?)\b/i],
];

/** The kind guessed from a name. */
export function gearKind(name: string): GearKind {
    return RULES.find(([, re]) => re.test(name))?.[0] ?? "other";
}

const isKind = (k: string | undefined): k is GearKind => !!k && (GEAR_KINDS as readonly string[]).includes(k);

/** The kind you picked for an item, or the one guessed from its name. */
export function resolveKind(name: string, picked?: Record<string, string>): GearKind {
    const k = picked?.[name];
    return isKind(k) ? k : gearKind(name);
}

/** A running shoe's usual life, in km: the wear bar fills towards this. */
export const SHOE_LIFE_KM = 700;
