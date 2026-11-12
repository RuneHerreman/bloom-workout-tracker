import { useEffect, useMemo, useState } from "react";
import { searchExercises, peekExercises, type Exercise } from "../exercises/api.ts";
import { getLogs, peekLogs } from "../logbook/api.ts";
import type { LoggedWorkout } from "../../assets/js/data/apiTypes.ts";
import ExerciseForm from "../exercises/components/ExerciseForm.tsx";

const ICON = {
    edit: "M4 20h4L19 9l-4-4L4 16v4Z M13.5 6.5l4 4",
    plus: "M12 5v14 M5 12h14",
};
function Icon({ d }: { d: string }) {
    return (
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"
            strokeLinecap="round" strokeLinejoin="round" aria-hidden>
            <path d={d} />
        </svg>
    );
}

const byName = (a: Exercise, b: Exercise) => a.name.localeCompare(b.name);

/**
 * The exercises you made yourself, in one place: how often each is used, and
 * the same form the library uses to edit, delete or add one.
 */
function CustomExercisesSection() {
    const [all, setAll] = useState<Exercise[]>(() => peekExercises() ?? []);
    const [logs, setLogs] = useState<LoggedWorkout[]>(() => peekLogs() ?? []);
    const [loaded, setLoaded] = useState(() => !!peekExercises());
    const [editing, setEditing] = useState<Exercise | "new" | null>(null);
    const [notice, setNotice] = useState<string | null>(null);

    useEffect(() => {
        searchExercises().then(e => { setAll(e); setLoaded(true); }).catch(() => setLoaded(true));
        getLogs().then(setLogs).catch(() => null);
    }, []);

    const mine = useMemo(() => all.filter(e => e.isCustom).sort(byName), [all]);
    const knownMuscles = useMemo(() => [...new Set(all.flatMap(e => e.targetMuscles))].sort(), [all]);
    const uses = useMemo(() => {
        const count = new Map<string, number>();
        for (const log of logs) {
            for (const id of new Set(log.exercises.map(x => x.exerciseId))) count.set(id, (count.get(id) ?? 0) + 1);
        }
        return count;
    }, [logs]);

    function saved(ex: Exercise) {
        setAll(prev => (prev.some(e => e.id === ex.id) ? prev.map(e => (e.id === ex.id ? ex : e)) : [...prev, ex]));
        setNotice(editing === "new" ? `Added “${ex.name}”.` : `Saved “${ex.name}”.`);
        setEditing(null);
    }
    function deleted(id: string) {
        const name = all.find(e => e.id === id)?.name;
        setAll(prev => prev.filter(e => e.id !== id));
        setNotice(name ? `Deleted “${name}”.` : null);
        setEditing(null);
    }

    return (
        <section className="profile-section" id="exercises">
            <div className="profile-section-head profile-section-head-row">
                <div>
                    <h2 className="profile-section-title">My exercises</h2>
                    <p className="profile-section-desc">Exercises you added yourself. Only you can see them.</p>
                </div>
                {!editing && (
                    <button type="button" className="button-component white" onClick={() => { setEditing("new"); setNotice(null); }}>
                        <Icon d={ICON.plus} /> New exercise
                    </button>
                )}
            </div>

            {notice && !editing && <p className="gear-notice" role="status">{notice}</p>}

            {editing ? (
                <div className="custom-ex-form">
                    <ExerciseForm
                        key={editing === "new" ? "new" : editing.id}
                        exercise={editing === "new" ? undefined : editing}
                        knownMuscles={knownMuscles}
                        backLabel="Back to my exercises"
                        createLabel="Create exercise"
                        onSaved={saved}
                        onDeleted={deleted}
                        onCancel={() => setEditing(null)}
                    />
                </div>
            ) : !loaded ? null : mine.length === 0 ? (
                <p className="gear-empty">None yet. Make one here, or from the exercise library while you build a workout.</p>
            ) : (
                <ul className="custom-ex-list">
                    {mine.map(e => {
                        const n = uses.get(e.id) ?? 0;
                        return (
                            <li key={e.id} className="custom-ex-item">
                                <span className={`type-dot ${e.type.toLowerCase()}`} title={e.type} aria-hidden />
                                <div className="custom-ex-text">
                                    <span className="custom-ex-name">{e.name}</span>
                                    <span className="custom-ex-meta">
                                        {e.type} · {e.targetMuscles.join(", ") || "No muscles"} · {n ? `in ${n} workout${n === 1 ? "" : "s"}` : "not logged yet"}
                                    </span>
                                </div>
                                <button type="button" className="gear-icon-btn" aria-label={`Edit ${e.name}`}
                                    onClick={() => { setEditing(e); setNotice(null); }}>
                                    <Icon d={ICON.edit} />
                                </button>
                            </li>
                        );
                    })}
                </ul>
            )}
        </section>
    );
}

export default CustomExercisesSection;
