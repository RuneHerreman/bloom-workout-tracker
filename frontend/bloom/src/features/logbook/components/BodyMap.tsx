import type { MuscleSets } from "../sessionSummary.ts";
import { BODY_BACK, BODY_FRONT, BODY_OUTLINE, type BodyPart } from "./bodyMapPaths.ts";

/**
 * Front-and-back anatomy, each muscle shaded by how many working sets the
 * session gave its group. The drawing is react-muscle-highlighter's (see
 * bodyMapPaths.ts); shading, hover and taps are ours.
 */

/** Which of our muscle groups (as named on exercises) each drawn muscle stands for; the first trained one wins. */
const GROUPS: Record<string, string[]> = {
    chest: ["Chest"],
    deltoids: ["Front delts", "Shoulders"],
    biceps: ["Biceps"],
    triceps: ["Triceps"],
    forearm: ["Forearms"],
    abs: ["Abs", "Core"],
    obliques: ["Obliques", "Core"],
    quadriceps: ["Quadriceps"],
    adductors: ["Adductors"],
    hamstring: ["Hamstrings"],
    gluteal: ["Glutes"],
    calves: ["Calves"],
    trapezius: ["Traps", "Back"],
    "upper-back": ["Lats", "Upper back", "Rhomboids", "Back"],
    "lower-back": ["Lower back", "Back"],
};

/**
 * Where the back view differs: its deltoids are the rear delts, and its trapezius
 * takes in the mid traps of the upper back. Side delts have no shape of their own
 * and show in the list only.
 */
const BACK_GROUPS: Record<string, string[]> = {
    deltoids: ["Rear delts", "Shoulders"],
    trapezius: ["Traps", "Upper back", "Back"],
};

type Side = "front" | "back";
const groupsOf = (p: BodyPart, side: Side) => (side === "back" ? BACK_GROUPS[p.slug] : undefined) ?? GROUPS[p.slug] ?? [];

/** Body, not muscle: drawn plain and never highlighted. */
const NEUTRAL = new Set(["head", "hair", "neck", "hands", "feet", "knees", "ankles", "tibialis"]);

const VIEWS = {
    front: { parts: BODY_FRONT, outline: BODY_OUTLINE.front, viewBox: "0 0 724 1448", label: "Front" },
    back: { parts: BODY_BACK, outline: BODY_OUTLINE.back, viewBox: "724 0 724 1448", label: "Back" },
} as const;

interface BodyMapProps {
    muscles: MuscleSets[];
    /** Muscle groups to pick out; the rest fade. */
    highlighted: string[];
    /** A muscle group was hovered (or left, with null) by a mouse, or clicked/tapped. */
    onPick: (muscle: string | null, how: "hover" | "click", e?: Event) => void;
}

function BodyMap({ muscles, highlighted, onPick }: BodyMapProps) {
    const sets = new Map(muscles.map(m => [m.muscle, m.sets]));
    const max = Math.max(1, ...muscles.map(m => m.sets));

    const isPicked = (p: BodyPart, side: Side) => groupsOf(p, side).some(g => highlighted.includes(g));

    function part(p: BodyPart, side: Side) {
        if (NEUTRAL.has(p.slug)) {
            return <g key={p.slug} className={p.slug === "hair" ? "bm-hair" : "bm-body"}>{p.paths.map((d, i) => <path key={i} d={d} />)}</g>;
        }
        const groups = groupsOf(p, side);
        const group = groups.find(g => sets.has(g)) ?? null;
        const n = group ? sets.get(group)! : 0;
        // One hue, light to dark, with a floor so a single set still reads as trained.
        const strength = n ? Math.round(30 + 70 * (n / max)) : 0;
        const isActive = isPicked(p, side);
        return (
            <g
                key={p.slug}
                className={`bm-muscle${n ? " trained" : ""}${isActive ? " active" : ""}`}
                style={n ? { fill: `color-mix(in srgb, var(--sum-bar) ${strength}%, var(--bm-empty))` } : undefined}
                // Hover is for a mouse; on touch the tap below toggles instead.
                onPointerEnter={e => e.pointerType === "mouse" && group && onPick(group, "hover")}
                onPointerLeave={e => e.pointerType === "mouse" && group && onPick(null, "hover")}
                onClick={e => group && onPick(group, "click", e.nativeEvent)}
            >
                <title>{group ? `${group}: ${n} working set${n !== 1 ? "s" : ""}` : groups[0] ?? p.slug}</title>
                {p.paths.map((d, i) => <path key={i} d={d} />)}
            </g>
        );
    }

    return (
        <div className={`body-map${highlighted.length ? " has-active" : ""}`}>
            {(["front", "back"] as const).map(side => {
                const v = VIEWS[side];
                return (
                    <figure key={side} className="bm-figure">
                        <svg viewBox={v.viewBox} role="img" aria-label={`${v.label} of the body`}>
                            {/* The picked muscle is drawn last, so it grows over its neighbours, not under them. */}
                            {[...v.parts]
                                .sort((a, b) => Number(isPicked(a, side)) - Number(isPicked(b, side)))
                                .map(p => part(p, side))}
                            <path className="bm-outline" d={v.outline} />
                        </svg>
                        <figcaption>{v.label}</figcaption>
                    </figure>
                );
            })}
        </div>
    );
}

export default BodyMap;
