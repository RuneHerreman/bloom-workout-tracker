import type { Exercise, LoggedExercise } from "../../../assets/js/data/apiTypes.ts";
import { setParts, workingSetNumbers } from "../logbookUtils.ts";
import { MapPin } from "lucide-react";

interface LogReadViewProps {
    exercises: LoggedExercise[];
    exerciseInfo: Record<string, Exercise>;
    /** Opens the editor scrolled to this exercise. */
    onEditExercise: (order: number) => void;
}

const MARKER: Record<string, { short: string; cls: string; title: string }> = {
    WarmUp: { short: "W", cls: "marker-w", title: "Warm-up" },
    DropSet: { short: "D", cls: "marker-d", title: "Drop set" },
    Failure: { short: "F", cls: "marker-f", title: "To failure" },
};

/** Dense, read-only list of a log: a row per exercise, its sets in aligned columns. */
function LogReadView({ exercises, exerciseInfo, onEditExercise }: LogReadViewProps) {
    if (exercises.length === 0) return <p className="log-read-empty">No exercises in this log.</p>;
    return (
        <ol className="log-read-list">
            {[...exercises].sort((a, b) => a.order - b.order).map((ex, i) => {
                const info = exerciseInfo[ex.exerciseId];
                const name = info?.name ?? "Unknown exercise";
                const sets = [...ex.sets].sort((a, b) => a.order - b.order);
                const working = workingSetNumbers(sets).filter(n => n > 0).length;
                const muscles = info?.targetMuscles.slice(0, 3).join(", ");
                return (
                    <li key={ex.order} className="log-read-item">
                        <span className="log-read-index" aria-hidden><span>{i + 1}</span></span>
                        <div className="log-read-head">
                            <button
                                type="button"
                                className="log-read-name"
                                title={`Edit ${name}`}
                                onClick={() => onEditExercise(ex.order)}
                            >
                                {name}
                            </button>
                            <span className="log-read-meta">
                                {working} set{working !== 1 ? "s" : ""}{muscles ? ` · ${muscles}` : ""}
                                {ex.gpxData && <MapPin size={11} className="log-read-gpx" aria-label="Has a GPS track" />}
                            </span>
                            {ex.note?.trim() && <span className="log-read-note">{ex.note}</span>}
                        </div>
                        <ul className="log-read-sets" aria-label={`${name} sets`}>
                            {sets.map((s, j) => {
                                const m = s.marker ? MARKER[s.marker] : undefined;
                                const p = setParts(s);
                                return (
                                    <li key={j} className={`log-read-set${m ? ` ${m.cls}` : ""}`} title={m?.title}>
                                        {m && <b className="log-read-marker">{m.short}</b>}
                                        <span className="log-read-value">{p.value}</span>
                                        {p.unit && <span className="log-read-unit">{p.unit}</span>}
                                        {p.detail && <span className="log-read-detail">{p.detail}</span>}
                                    </li>
                                );
                            })}
                        </ul>
                    </li>
                );
            })}
        </ol>
    );
}

export default LogReadView;
