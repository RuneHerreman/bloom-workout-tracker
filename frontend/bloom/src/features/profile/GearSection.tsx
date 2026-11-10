import { useEffect, useMemo, useState } from "react";
import { Backpack, Plus, X } from "lucide-react";
import { updateGear } from "../auth/api.ts";
import { getLogs } from "../logbook/api.ts";
import type { LoggedWorkout } from "../../assets/js/data/apiTypes.ts";
import { relativeDate } from "../../utils/workoutUtils.ts";
import { computeGearUsage } from "./gearStats.ts";

const SUGGESTIONS = ["Running shoes", "Road bike", "Trail shoes", "Lifting belt", "Wrist wraps", "Heart-rate strap"];

function lastUsedLabel(iso: string): string {
    const r = relativeDate(iso);
    return r === "Today" || r === "Yesterday" ? r.toLowerCase() : r;
}

interface GearSectionProps {
    initialGear: string[];
}

/**
 * Gear closet: add/remove items (saved immediately) and see how much each has
 * been used — handy for knowing when running shoes are due for replacement.
 */
function GearSection({ initialGear }: GearSectionProps) {
    const [gear, setGear] = useState<string[]>(initialGear);
    const [input, setInput] = useState("");
    const [logs, setLogs] = useState<LoggedWorkout[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [saving, setSaving] = useState(false);

    useEffect(() => {
        getLogs().then(setLogs).catch(() => null);
    }, []);

    useEffect(() => {
        if (window.location.hash !== "#gear") return;
        document.getElementById("gear")?.scrollIntoView({ behavior: "smooth", block: "start" });
    }, []);

    const usage = useMemo(() => computeGearUsage(logs), [logs]);

    async function persist(next: string[]) {
        const previous = gear;
        setGear(next);
        setSaving(true);
        setError(null);
        try {
            await updateGear(next);
        } catch {
            setGear(previous);
            setError("Couldn't save your gear — try again.");
        } finally {
            setSaving(false);
        }
    }

    function add(raw: string) {
        const name = raw.trim().replace(/\s+/g, " ");
        if (!name) return;
        if (gear.some(g => g.toLowerCase() === name.toLowerCase())) {
            setError(`“${name}” is already in your gear.`);
            return;
        }
        setInput("");
        persist([...gear, name]);
    }

    const unusedSuggestions = SUGGESTIONS.filter(s => !gear.some(g => g.toLowerCase() === s.toLowerCase()));

    return (
        <section className="profile-section" id="gear">
            <div className="profile-section-head">
                <h2 className="profile-section-title">Gear</h2>
                <p className="profile-section-desc">
                    Shoes, bikes, belts — attach them to exercises in your logs and track how far each has gone.
                </p>
            </div>

            {error && <div className="error-banner">{error}</div>}

            {gear.length > 0 ? (
                <ul className="gear-list">
                    {gear.map(g => {
                        const u = usage.get(g);
                        return (
                            <li key={g} className="gear-item">
                                <span className="gear-item-icon"><Backpack size={15} /></span>
                                <div className="gear-item-text">
                                    <span className="gear-item-name">{g}</span>
                                    <span className="gear-item-meta">
                                        {u ? (
                                            <>
                                                {u.distanceKm > 0 && <><b>{u.distanceKm.toFixed(u.distanceKm >= 100 ? 0 : 1)} km</b> · </>}
                                                {u.sessions} session{u.sessions !== 1 ? "s" : ""}
                                                {u.lastUsed && <> · last used {lastUsedLabel(u.lastUsed)}</>}
                                            </>
                                        ) : "Not used yet"}
                                    </span>
                                </div>
                                <button
                                    type="button"
                                    className="gear-item-remove"
                                    aria-label={`Remove ${g}`}
                                    disabled={saving}
                                    onClick={() => persist(gear.filter(x => x !== g))}
                                >
                                    <X size={14} />
                                </button>
                            </li>
                        );
                    })}
                </ul>
            ) : (
                <p className="gear-empty">No gear yet. Add your first item below.</p>
            )}

            <form
                className="gear-add"
                onSubmit={e => { e.preventDefault(); add(input); }}
            >
                <input
                    type="text"
                    value={input}
                    maxLength={100}
                    placeholder="e.g. Nike Pegasus 41"
                    aria-label="New gear name"
                    onChange={e => { setInput(e.target.value); setError(null); }}
                />
                <button type="submit" className="button-component white" disabled={!input.trim() || saving}>
                    <Plus size={14} /> Add
                </button>
            </form>

            {unusedSuggestions.length > 0 && gear.length < 3 && (
                <div className="gear-suggestions">
                    <span>Quick add:</span>
                    {unusedSuggestions.slice(0, 4).map(s => (
                        <button key={s} type="button" className="exercise-library-chip" onClick={() => add(s)}>
                            <Plus size={10} /> {s}
                        </button>
                    ))}
                </div>
            )}
        </section>
    );
}

export default GearSection;
