import { useEffect, useMemo, useRef, useState } from "react";
import { renameGear, setGearIcon, updateGear } from "../auth/api.ts";
import { forgetLogs, getLogs } from "../logbook/api.ts";
import { forgetTemplates } from "../templates/api.ts";
import type { LoggedWorkout } from "../../assets/js/data/apiTypes.ts";
import { relativeDate } from "../../utils/workoutUtils.ts";
import { computeGearUsage } from "./gearStats.ts";
import { SHOE_LIFE_KM, gearKind, resolveKind, type GearKind } from "./gearKinds.ts";
import GearIcon from "./GearIcon.tsx";
import GearIconPicker from "./GearIconPicker.tsx";

const SUGGESTIONS = ["Running shoes", "Road bike", "Trail shoes", "Lifting belt", "Wrist wraps", "Heart-rate strap"];

/** Small inline action icons, drawn like the gear icons. */
const ACTIONS = {
    edit: "M4 20h4L19 9l-4-4L4 16v4Z M13.5 6.5l4 4",
    remove: "M6 6l12 12 M18 6 6 18",
    check: "M5 12.5l4.5 4.5L19 7",
    plus: "M12 5v14 M5 12h14",
};
function ActionIcon({ d, size = 14 }: { d: string; size?: number }) {
    return (
        <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"
            strokeLinecap="round" strokeLinejoin="round" aria-hidden>
            <path d={d} />
        </svg>
    );
}

function lastUsedLabel(iso: string): string {
    const r = relativeDate(iso);
    return r === "Today" || r === "Yesterday" ? r.toLowerCase() : r;
}

const cleanName = (raw: string) => raw.trim().replace(/\s+/g, " ");
const errorText = (e: unknown, fallback: string) => (e as { error?: string })?.error ?? fallback;

interface GearSectionProps {
    initialGear: string[];
    /** Icons picked per item; the rest are guessed from the name. */
    initialIcons?: Record<string, string>;
}

/**
 * Gear closet: add, rename and remove items (saved straight away) and see how
 * much each has been used, with a wear bar for running shoes.
 *
 * Logs refer to gear by name, so a rename goes through the server, which
 * renames it on every log and template too; removing only takes it off the
 * list, and logs that used it keep the name.
 */
function GearSection({ initialGear, initialIcons = {} }: GearSectionProps) {
    const [gear, setGear] = useState<string[]>(initialGear);
    const [icons, setIcons] = useState<Record<string, string>>(initialIcons);
    const [input, setInput] = useState("");
    const [logs, setLogs] = useState<LoggedWorkout[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [saving, setSaving] = useState(false);
    const [editing, setEditing] = useState<{ from: string; value: string } | null>(null);
    const [confirmRemove, setConfirmRemove] = useState<string | null>(null);
    const [notice, setNotice] = useState<string | null>(null);
    const editRef = useRef<HTMLInputElement>(null);

    useEffect(() => {
        getLogs().then(setLogs).catch(() => null);
    }, []);

    useEffect(() => {
        if (window.location.hash !== "#gear") return;
        document.getElementById("gear")?.scrollIntoView({ behavior: "smooth", block: "start" });
    }, []);

    useEffect(() => { if (editing) editRef.current?.select(); }, [editing?.from]); // eslint-disable-line react-hooks/exhaustive-deps

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
            setError("Couldn't save your gear. Try again.");
        } finally {
            setSaving(false);
        }
    }

    function add(raw: string) {
        const name = cleanName(raw);
        if (!name) return;
        if (gear.some(g => g.toLowerCase() === name.toLowerCase())) {
            setError(`“${name}” is already in your gear.`);
            return;
        }
        setInput("");
        setNotice(null);
        persist([...gear, name]);
    }

    async function saveRename() {
        if (!editing) return;
        const to = cleanName(editing.value);
        if (!to || to === editing.from) { setEditing(null); return; }
        if (gear.some(g => g !== editing.from && g.toLowerCase() === to.toLowerCase())) {
            setError(`“${to}” is already in your gear.`);
            return;
        }
        setSaving(true);
        setError(null);
        try {
            const result = await renameGear(editing.from, to);
            setGear(prev => prev.map(g => (g === editing.from ? to : g)));
            setIcons(prev => {
                if (!(editing.from in prev)) return prev;
                const { [editing.from]: kept, ...rest } = prev;
                return { ...rest, [to]: kept };
            });
            // Logs and templates now carry the new name: reload them.
            forgetLogs();
            forgetTemplates();
            getLogs().then(setLogs).catch(() => null);
            const touched = [
                result.workouts && `${result.workouts} workout${result.workouts === 1 ? "" : "s"}`,
                result.templates && `${result.templates} template${result.templates === 1 ? "" : "s"}`,
            ].filter(Boolean).join(" and ");
            setNotice(`Renamed to “${to}”${touched ? `, in ${touched} too` : ""}.`);
            setEditing(null);
        } catch (e) {
            setError(errorText(e, "Couldn't rename it. Try again."));
        } finally {
            setSaving(false);
        }
    }

    async function pickIcon(name: string, kind: GearKind | null) {
        const previous = icons;
        const next = { ...icons };
        if (kind) next[name] = kind; else delete next[name];
        setIcons(next);
        setError(null);
        try {
            await setGearIcon(name, kind);
        } catch (e) {
            setIcons(previous);
            setError(errorText(e, "Couldn't change the icon. Try again."));
        }
    }

    function remove(name: string) {
        setConfirmRemove(null);
        setNotice(null);
        persist(gear.filter(x => x !== name));
        // The server drops the icon along with the item.
        setIcons(prev => { const next = { ...prev }; delete next[name]; return next; });
    }

    const unusedSuggestions = SUGGESTIONS.filter(s => !gear.some(g => g.toLowerCase() === s.toLowerCase()));

    return (
        <section className="profile-section" id="gear">
            <div className="profile-section-head">
                <h2 className="profile-section-title">Gear</h2>
                <p className="profile-section-desc">
                    Shoes, bikes, belts: attach them to exercises in your logs and see how far each has gone.
                </p>
            </div>

            {error && <div className="error-banner">{error}</div>}
            {notice && <p className="gear-notice" role="status">{notice}</p>}

            {gear.length > 0 ? (
                <ul className="gear-list">
                    {gear.map(g => {
                        const u = usage.get(g);
                        const kind = resolveKind(g, icons);
                        const isEditing = editing?.from === g;
                        const wear = kind === "shoe" && u && u.distanceKm > 0 ? Math.min(1, u.distanceKm / SHOE_LIFE_KM) : null;
                        return (
                            <li key={g} className={`gear-item gear-${kind}`}>
                                <GearIconPicker name={g} kind={kind} picked={g in icons} guessed={gearKind(g)}
                                    disabled={saving} onPick={k => pickIcon(g, k)} />

                                {isEditing ? (
                                    <form className="gear-item-edit" onSubmit={e => { e.preventDefault(); saveRename(); }}>
                                        <input
                                            ref={editRef}
                                            value={editing.value}
                                            maxLength={100}
                                            aria-label={`New name for ${g}`}
                                            onChange={e => { setEditing({ from: g, value: e.target.value }); setError(null); }}
                                            onKeyDown={e => { if (e.key === "Escape") { e.preventDefault(); setEditing(null); } }}
                                        />
                                        <button type="submit" className="gear-icon-btn confirm" aria-label="Save name" disabled={saving}>
                                            <ActionIcon d={ACTIONS.check} />
                                        </button>
                                        <button type="button" className="gear-icon-btn" aria-label="Cancel" onClick={() => setEditing(null)}>
                                            <ActionIcon d={ACTIONS.remove} />
                                        </button>
                                    </form>
                                ) : (
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
                                        {wear !== null && (
                                            <span className={`gear-wear${wear >= 1 ? " worn" : wear >= 0.8 ? " due" : ""}`}
                                                title={`About ${SHOE_LIFE_KM} km is a usual life for running shoes`}>
                                                <span className="gear-wear-track"><span style={{ width: `${wear * 100}%` }} /></span>
                                                {wear >= 1 ? "Past a usual shoe life" : wear >= 0.8 ? "Nearly due for new ones" : `${Math.round(wear * 100)}% of a usual ${SHOE_LIFE_KM} km`}
                                            </span>
                                        )}
                                    </div>
                                )}

                                {!isEditing && (confirmRemove === g ? (
                                    <div className="gear-confirm">
                                        <span>{u ? "Logs that used it keep the name." : "Remove it?"}</span>
                                        <button type="button" className="button-component red" disabled={saving} onClick={() => remove(g)}>Remove</button>
                                        <button type="button" className="unsaved-dialog-discard" onClick={() => setConfirmRemove(null)}>Keep</button>
                                    </div>
                                ) : (
                                    <div className="gear-item-actions">
                                        <button type="button" className="gear-icon-btn" aria-label={`Rename ${g}`} disabled={saving}
                                            onClick={() => { setEditing({ from: g, value: g }); setConfirmRemove(null); setNotice(null); }}>
                                            <ActionIcon d={ACTIONS.edit} />
                                        </button>
                                        <button type="button" className="gear-icon-btn danger" aria-label={`Remove ${g}`} disabled={saving}
                                            onClick={() => { setConfirmRemove(g); setEditing(null); }}>
                                            <ActionIcon d={ACTIONS.remove} />
                                        </button>
                                    </div>
                                ))}
                            </li>
                        );
                    })}
                </ul>
            ) : (
                <p className="gear-empty">No gear yet. Add your first item below.</p>
            )}

            <form className="gear-add" onSubmit={e => { e.preventDefault(); add(input); }}>
                <span className="gear-add-field">
                    <span className="gear-add-icon"><GearIcon kind={gearKind(input)} size={15} /></span>
                    <input
                        type="text"
                        value={input}
                        maxLength={100}
                        placeholder="e.g. Nike Pegasus 41"
                        aria-label="New gear name"
                        onChange={e => { setInput(e.target.value); setError(null); }}
                    />
                </span>
                <button type="submit" className="button-component white" disabled={!input.trim() || saving}>
                    <ActionIcon d={ACTIONS.plus} /> Add
                </button>
            </form>

            {unusedSuggestions.length > 0 && gear.length < 3 && (
                <div className="gear-suggestions">
                    <span>Quick add:</span>
                    {unusedSuggestions.slice(0, 4).map(s => (
                        <button key={s} type="button" className="exercise-library-chip" onClick={() => add(s)}>
                            <GearIcon kind={gearKind(s)} size={12} /> {s}
                        </button>
                    ))}
                </div>
            )}
        </section>
    );
}

export default GearSection;
