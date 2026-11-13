import { useMemo, useState } from "react";
import { Check, ChevronDown, Minus, Plus, X } from "lucide-react";
import { MUSCLE_HINTS, MUSCLE_TREE, areaOf, canonicalMuscle, inTree, normalizeMuscles } from "../muscles.ts";

interface MuscleTreePickerProps {
    /** What's picked: areas on their own ("Back") and muscles ("Lats"); a muscle implies its area. */
    value: string[];
    onChange: (muscles: string[]) => void;
    /** Names outside the tree used by other custom exercises, offered under "Other". */
    otherSuggestions?: string[];
    error?: string | null;
}

type AreaState = "off" | "general" | "some" | "all";

/**
 * Body areas as rows that unfold to their muscles. Ticking an area means "the area in
 * general"; ticking a muscle ticks its area too; unticking an area clears it and its
 * muscles. Only what was picked is stored.
 */
function MuscleTreePicker({ value, onChange, otherSuggestions = [], error }: MuscleTreePickerProps) {
    const picked = useMemo(() => new Set(value), [value]);
    const stateOf = (area: string, muscles: string[]): AreaState => {
        const n = muscles.filter(m => picked.has(m)).length;
        if (n > 0 && n === muscles.length) return "all";
        if (n > 0) return "some";
        return picked.has(area) ? "general" : "off";
    };
    const [open, setOpen] = useState<Set<string>>(() => new Set(value.filter(inTree).map(areaOf)));
    const [other, setOther] = useState("");

    const setOpenArea = (area: string, isOpen: boolean) =>
        setOpen(prev => { const next = new Set(prev); if (isOpen) next.add(area); else next.delete(area); return next; });

    function toggleArea(area: string, muscles: string[]) {
        if (stateOf(area, muscles) === "off") {
            onChange(normalizeMuscles([...value, area]));
            setOpenArea(area, true);
        } else {
            onChange(value.filter(m => m !== area && !muscles.includes(m)));
        }
    }

    function toggleMuscle(muscle: string) {
        onChange(picked.has(muscle) ? value.filter(m => m !== muscle) : normalizeMuscles([...value, muscle]));
    }

    const others = value.filter(m => !inTree(m));
    const otherQuery = other.trim().toLowerCase();
    const otherPicks = otherSuggestions
        .filter(m => !inTree(m) && !picked.has(m))
        .filter(m => !otherQuery || m.toLowerCase().includes(otherQuery))
        .slice(0, 8);

    function addOther(raw: string) {
        const m = canonicalMuscle(raw.replace(/\s+/g, " "));
        if (!m) return;
        const known = otherSuggestions.find(s => s.toLowerCase() === m.toLowerCase());
        const name = inTree(m) ? m : known ?? m.charAt(0).toUpperCase() + m.slice(1);
        onChange(normalizeMuscles([...value, name]));
        if (inTree(name)) setOpenArea(areaOf(name), true);
        setOther("");
    }

    return (
        <fieldset className="exercise-form-field muscle-tree" aria-describedby="muscle-tree-hint">
            <legend>Muscles</legend>
            <small id="muscle-tree-hint" className="exercise-form-hint">
                Tick an area for the area in general, or open it to pick the muscles.
            </small>
            <ul className="muscle-tree-areas">
                {MUSCLE_TREE.map(({ area, muscles }) => {
                    const state = stateOf(area, muscles);
                    const isOpen = open.has(area);
                    const chosen = muscles.filter(m => picked.has(m));
                    // An area without muscles (Chest) is just on or off: nothing to sum up.
                    const summary = !muscles.length ? "" : state === "general" ? "In general" : chosen.join(", ");
                    return (
                        <li key={area} className={`muscle-tree-area ${state}${isOpen ? " open" : ""}`}>
                            <div className="muscle-tree-row">
                                <button
                                    type="button"
                                    role="checkbox"
                                    aria-checked={state === "off" ? false : state === "some" ? "mixed" : true}
                                    className="muscle-tree-check"
                                    onClick={() => toggleArea(area, muscles)}
                                >
                                    <span className="muscle-tree-box" aria-hidden>
                                        {state === "some" ? <Minus size={12} /> : state !== "off" ? <Check size={12} /> : null}
                                    </span>
                                    <span className="muscle-tree-name">{area}</span>
                                    {summary && <span className="muscle-tree-summary">{summary}</span>}
                                </button>
                                {muscles.length > 0 && <button
                                    type="button"
                                    className="muscle-tree-expand"
                                    aria-expanded={isOpen}
                                    aria-label={`${isOpen ? "Hide" : "Show"} muscles in ${area}`}
                                    onClick={() => setOpenArea(area, !isOpen)}
                                >
                                    <ChevronDown size={16} />
                                </button>}
                            </div>
                            {isOpen && muscles.length > 0 && (
                                <div className="muscle-tree-muscles">
                                    {muscles.map(m => (
                                        <button
                                            key={m}
                                            type="button"
                                            role="checkbox"
                                            aria-checked={picked.has(m)}
                                            className={`muscle-tree-muscle${picked.has(m) ? " selected" : ""}`}
                                            title={MUSCLE_HINTS[m]}
                                            onClick={() => toggleMuscle(m)}
                                        >
                                            {picked.has(m) ? <Check size={11} /> : <Plus size={11} />}
                                            <span>{m}</span>
                                            {MUSCLE_HINTS[m] && <small>{MUSCLE_HINTS[m]}</small>}
                                        </button>
                                    ))}
                                </div>
                            )}
                        </li>
                    );
                })}
                <li className={`muscle-tree-area other${others.length ? " some" : ""}`}>
                    <div className="muscle-tree-other">
                        <span className="muscle-tree-name">Other</span>
                        <div className="exercise-form-muscles">
                            {others.map(m => (
                                <span key={m} className="exercise-form-muscle selected">
                                    {m}
                                    <button type="button" aria-label={`Remove ${m}`} onClick={() => onChange(value.filter(x => x !== m))}>
                                        <X size={10} />
                                    </button>
                                </span>
                            ))}
                            <input
                                className="exercise-form-muscle-input"
                                value={other}
                                placeholder="e.g. Neck"
                                aria-label="Add a muscle that isn't listed"
                                onChange={e => setOther(e.target.value)}
                                onKeyDown={e => {
                                    if (e.key === "Enter" || e.key === ",") { e.preventDefault(); addOther(other); }
                                    if (e.key === "Backspace" && other === "" && others.length) onChange(value.filter(x => x !== others[others.length - 1]));
                                }}
                            />
                        </div>
                        {otherPicks.length > 0 && (
                            <div className="exercise-form-suggestions">
                                {otherPicks.map(m => (
                                    <button key={m} type="button" className="exercise-library-chip" onClick={() => addOther(m)}>
                                        <Plus size={10} /> {m}
                                    </button>
                                ))}
                            </div>
                        )}
                    </div>
                </li>
            </ul>
            {error && <small className="exercise-form-error">{error}</small>}
        </fieldset>
    );
}

export default MuscleTreePicker;
