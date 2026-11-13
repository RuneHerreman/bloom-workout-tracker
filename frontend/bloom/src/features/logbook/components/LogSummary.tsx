import { useMemo, useState } from "react";
import type { LoggedExercise, LoggedWorkout } from "../api.ts";
import type { Exercise } from "../../exercises/api.ts";
import { resolveHints } from "../logbookUtils.ts";
import { areaOf, summarizeSession, type MuscleArea } from "../sessionSummary.ts";
import { formatVolume } from "../../dashboard/transforms.ts";
import { formatDuration } from "../../insights/analysis.ts";
import BodyMap from "./BodyMap.tsx";
import { ChevronRight } from "lucide-react";

const VIEW_KEY = "bloom.logSummary.muscleView";
function readView(): "body" | "list" {
    try { return localStorage.getItem(VIEW_KEY) === "list" ? "list" : "body"; } catch { return "body"; }
}

interface LogSummaryProps {
    name: string;
    loggedAt: string;
    exercises: LoggedExercise[];
    exerciseInfo: Record<string, Exercise>;
    /** Every other log, for "vs last time" and records. */
    history: LoggedWorkout[];
}

/** What's picked out: an area (all its muscles) or one muscle in it. */
interface Selection { key: string; label: string; sets: number; muscles: string[] }

const areaSelection = (a: MuscleArea): Selection =>
    ({ key: `area:${a.area}`, label: a.area, sets: a.sets, muscles: a.parts.map(p => p.muscle) });

/** An area with just itself in it (Chest, or Core) has nothing to unfold. */
const hasParts = (a: MuscleArea) => a.parts.length > 1 || (a.parts.length === 1 && a.parts[0].muscle !== a.area);

const sets = (n: number) => `${n} working set${n !== 1 ? "s" : ""}`;

const shortDate = (iso: string) => new Date(iso).toLocaleDateString("en-GB", { day: "numeric", month: "short" });
const kg = (n: number) => `${n.toLocaleString("en-GB", { maximumFractionDigits: 1 })} kg`;

/**
 * The session at a glance, above the tables: what it added up to, how it compares
 * with last time, and which muscles took the work. Follows the edits as you type
 * (empty fields count as last time's numbers, as they save).
 */
function LogSummary({ name, loggedAt, exercises, exerciseInfo, history }: LogSummaryProps) {
    const [active, setActive] = useState<Selection | null>(null);
    const [open, setOpen] = useState<Set<string>>(() => new Set());
    // Narrow screens show one of the two; wide ones show both and ignore this.
    const [view, setView] = useState<"body" | "list">(readView);
    function pickView(v: "body" | "list") {
        setView(v);
        try { localStorage.setItem(VIEW_KEY, v); } catch { /* private mode: just don't remember */ }
    }

    const s = useMemo(
        () => summarizeSession(name, loggedAt, resolveHints(exercises), exerciseInfo, history),
        [name, loggedAt, exercises, exerciseInfo, history],
    );

    const hasStrength = s.workingSets > 0;
    const hasCardio = s.distanceKm > 0 || s.minutes > 0;
    if (!hasStrength && !hasCardio) return null;

    const volume = formatVolume(s.volumeKg);
    const change = s.previous && s.previous.volumeKg > 0 && s.volumeKg > 0
        ? Math.round(((s.volumeKg - s.previous.volumeKg) / s.previous.volumeKg) * 100)
        : null;
    const maxSets = Math.max(1, ...s.areas.map(a => a.sets));

    /** Mouse hover picks and clears; a click picks, and a tap on what's picked clears it. */
    function pick(sel: Selection | null, how: "hover" | "click", e?: Event) {
        if (how === "hover") { setActive(sel); return; }
        const isMouse = (e as PointerEvent | undefined)?.pointerType === "mouse";
        setActive(!isMouse && sel && active?.key === sel.key ? null : sel);
    }

    /** From the figure: a muscle tagged only by its area ("Back") picks the whole area. */
    function pickFromBody(muscle: string | null, how: "hover" | "click", e?: Event) {
        if (!muscle) { pick(null, how, e); return; }
        const area = s.areas.find(a => a.area === areaOf(muscle));
        if (!area) return;
        const part = area.parts.find(p => p.muscle === muscle);
        pick(muscle === area.area || !hasParts(area) || !part
            ? areaSelection(area)
            : { key: muscle, label: muscle, sets: part.sets, muscles: [muscle] }, how, e);
    }

    function toggleOpen(area: string) {
        setOpen(prev => { const next = new Set(prev); if (next.has(area)) next.delete(area); else next.add(area); return next; });
    }

    const hoverProps = (sel: Selection) => ({
        onPointerEnter: (e: React.PointerEvent) => e.pointerType === "mouse" && pick(sel, "hover"),
        onPointerLeave: (e: React.PointerEvent) => e.pointerType === "mouse" && pick(null, "hover"),
    });

    return (
        <section className="log-summary" aria-label="Session summary">
            <dl className="log-summary-stats">
                {hasStrength && (
                    <div>
                        <dt>Volume</dt>
                        <dd>{volume.value}<span>{volume.unit === "tonnes" ? "t" : "kg"}</span></dd>
                        {s.previous && (
                            <p className="log-summary-caption">
                                {change !== null && (
                                    <span className={`log-summary-change ${change > 0 ? "up" : change < 0 ? "down" : ""}`}>
                                        {change > 0 ? "↑" : change < 0 ? "↓" : "→"} {Math.abs(change)}%
                                    </span>
                                )}
                                {" "}<span className="log-summary-nowrap">vs {shortDate(s.previous.loggedAt)}</span>
                            </p>
                        )}
                    </div>
                )}
                {hasStrength && (
                    <div>
                        <dt>Working sets</dt>
                        <dd>{s.workingSets}</dd>
                        <p className="log-summary-caption">{s.strengthExercises} exercise{s.strengthExercises !== 1 ? "s" : ""}</p>
                    </div>
                )}
                {hasCardio && s.distanceKm > 0 && (
                    <div>
                        <dt>Distance</dt>
                        <dd>{s.distanceKm.toLocaleString("en-GB", { maximumFractionDigits: 1 })}<span>km</span></dd>
                    </div>
                )}
                {hasCardio && s.minutes > 0 && (
                    <div>
                        <dt>Time</dt>
                        <dd className="log-summary-text">{formatDuration(Math.round(s.minutes))}</dd>
                    </div>
                )}
                {hasStrength && (
                    <div title={s.records.map(r => `${r.name}: ${kg(r.weightKg)} (was ${kg(r.previousKg)})`).join("\n") || undefined}>
                        <dt>Records</dt>
                        <dd className={s.records.length ? "log-summary-record" : "is-zero"}>{s.records.length}</dd>
                        <p className="log-summary-caption">
                            {s.records.length
                                ? `${s.records[0].name} ${kg(s.records[0].weightKg)}${s.records.length > 1 ? ` +${s.records.length - 1}` : ""}`
                                : "None this time"}
                        </p>
                    </div>
                )}
            </dl>

            {s.areas.length > 0 && (
                <div className={`log-summary-muscles show-${view}`}>
                    <div className="log-summary-muscles-head">
                        <p className="log-summary-label">Muscles worked <span>· working sets</span></p>
                        <div className="log-summary-toggle" role="tablist" aria-label="Show muscles as">
                            {(["body", "list"] as const).map(v => (
                                <button key={v} type="button" role="tab" aria-selected={view === v}
                                    className={view === v ? "active" : undefined} onClick={() => pickView(v)}>
                                    {v === "body" ? "Body" : "List"}
                                </button>
                            ))}
                        </div>
                    </div>
                    <div className="log-summary-muscles-body">
                        <div className="log-summary-map">
                            <BodyMap muscles={s.muscles} highlighted={active?.muscles ?? []} onPick={pickFromBody} />
                            <p className="log-summary-map-caption" aria-live="polite">
                                {active ? <><strong>{active.label}</strong> · {sets(active.sets)}</> : "Tap a muscle"}
                            </p>
                        </div>
                        <ul className={`log-summary-areas${active ? " has-active" : ""}`}>
                            {s.areas.map(a => {
                                const sel = areaSelection(a);
                                const expandable = hasParts(a);
                                const isOpen = expandable && open.has(a.area);
                                return (
                                    <li key={a.area} className={isOpen ? "open" : undefined}>
                                        <button type="button"
                                            className={`log-summary-row is-area${active?.key === sel.key ? " active" : ""}`}
                                            aria-expanded={expandable ? isOpen : undefined}
                                            title={sets(a.sets)}
                                            {...hoverProps(sel)}
                                            onClick={e => { if (expandable) toggleOpen(a.area); pick(sel, "click", e.nativeEvent); }}>
                                            <span className="log-summary-muscle">
                                                {expandable ? <ChevronRight size={14} className="log-summary-chevron" aria-hidden /> : <span className="log-summary-chevron" aria-hidden />}
                                                {a.area}
                                            </span>
                                            <span className="log-summary-track"><span style={{ width: `${(a.sets / maxSets) * 100}%` }} /></span>
                                            <span className="log-summary-count">{a.sets}</span>
                                        </button>
                                        {isOpen && (
                                            <ul className="log-summary-parts">
                                                {a.parts.map(p => {
                                                    const partSel: Selection = { key: p.muscle, label: p.muscle === a.area ? `${a.area} (general)` : p.muscle, sets: p.sets, muscles: [p.muscle] };
                                                    return (
                                                        <li key={p.muscle}>
                                                            <button type="button"
                                                                className={`log-summary-row is-part${active?.key === partSel.key ? " active" : ""}`}
                                                                title={`${partSel.label}: ${sets(p.sets)}`}
                                                                {...hoverProps(partSel)}
                                                                onClick={e => pick(partSel, "click", e.nativeEvent)}>
                                                                <span className="log-summary-muscle">{p.muscle === a.area ? "General" : p.muscle}</span>
                                                                <span className="log-summary-track"><span style={{ width: `${(p.sets / maxSets) * 100}%` }} /></span>
                                                                <span className="log-summary-count">{p.sets}</span>
                                                            </button>
                                                        </li>
                                                    );
                                                })}
                                            </ul>
                                        )}
                                    </li>
                                );
                            })}
                        </ul>
                    </div>
                </div>
            )}
        </section>
    );
}

export default LogSummary;
