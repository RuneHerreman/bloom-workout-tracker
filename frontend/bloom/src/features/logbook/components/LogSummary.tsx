import { useMemo, useState } from "react";
import type { LoggedExercise, LoggedWorkout } from "../api.ts";
import type { Exercise } from "../../exercises/api.ts";
import { resolveHints } from "../logbookUtils.ts";
import { summarizeSession } from "../sessionSummary.ts";
import { formatVolume } from "../../dashboard/transforms.ts";
import { formatDuration } from "../../insights/analysis.ts";
import BodyMap from "./BodyMap.tsx";

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

/** Muscles listed before "Show all". */
const MUSCLE_ROWS = 6;

const sets = (n: number) => `${n} working set${n !== 1 ? "s" : ""}`;

const shortDate = (iso: string) => new Date(iso).toLocaleDateString("en-GB", { day: "numeric", month: "short" });
const kg = (n: number) => `${n.toLocaleString("en-GB", { maximumFractionDigits: 1 })} kg`;

/**
 * The session at a glance, above the tables: what it added up to, how it compares
 * with last time, and which muscles took the work. Follows the edits as you type
 * (empty fields count as last time's numbers, as they save).
 */
function LogSummary({ name, loggedAt, exercises, exerciseInfo, history }: LogSummaryProps) {
    const [active, setActive] = useState<string | null>(null);
    const [showAll, setShowAll] = useState(false);
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
    const maxSets = Math.max(1, ...s.muscles.map(m => m.sets));
    const activeSets = s.muscles.find(m => m.muscle === active)?.sets ?? 0;
    // A mixed session can touch a dozen muscles: the biggest few first, the rest a tap away.
    // The figure always shows every muscle; only the list is cut.
    const hidden = Math.max(0, s.muscles.length - MUSCLE_ROWS);
    const rows = showAll || hidden <= 1 ? s.muscles : s.muscles.slice(0, MUSCLE_ROWS);

    /** Mouse hover picks and clears; a click picks, and a tap on what's picked clears it. */
    function pick(muscle: string | null, how: "hover" | "click", e?: Event) {
        if (how === "hover") { setActive(muscle); return; }
        const isMouse = (e as PointerEvent | undefined)?.pointerType === "mouse";
        setActive(!isMouse && muscle && active === muscle ? null : muscle);
    }

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

            {s.muscles.length > 0 && (
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
                            <BodyMap muscles={s.muscles} highlighted={active ? [active] : []} onPick={pick} />
                            <p className="log-summary-map-caption" aria-live="polite">
                                {active ? <><strong>{active}</strong> · {sets(activeSets)}</> : "Tap a muscle"}
                            </p>
                        </div>
                        {/* One row per muscle, most sets first: a single workout hits few enough to show them all. */}
                        <ul className={`log-summary-areas${active ? " has-active" : ""}`}>
                            {rows.map(m => (
                                <li key={m.muscle}>
                                    <button type="button"
                                        className={`log-summary-row${active === m.muscle ? " active" : ""}`}
                                        title={`${m.muscle}: ${sets(m.sets)}`}
                                        onPointerEnter={e => e.pointerType === "mouse" && pick(m.muscle, "hover")}
                                        onPointerLeave={e => e.pointerType === "mouse" && pick(null, "hover")}
                                        onClick={e => pick(m.muscle, "click", e.nativeEvent)}>
                                        <span className="log-summary-muscle">{m.muscle}</span>
                                        <span className="log-summary-track"><span style={{ width: `${(m.sets / maxSets) * 100}%` }} /></span>
                                        <span className="log-summary-count">{m.sets}</span>
                                    </button>
                                </li>
                            ))}
                            {hidden > 1 && (
                                <li>
                                    <button type="button" className="log-summary-more" onClick={() => setShowAll(v => !v)}>
                                        {showAll ? "Show fewer" : `Show all ${s.muscles.length}`}
                                    </button>
                                </li>
                            )}
                        </ul>
                    </div>
                </div>
            )}
        </section>
    );
}

export default LogSummary;
