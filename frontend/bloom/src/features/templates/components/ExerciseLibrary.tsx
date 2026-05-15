import { useState, useMemo } from "react";
import type { Exercise } from "../../exercises/api.ts";
import {X} from "lucide-react";
import Button from "../../../components/general/ButtonComponent.tsx";

const TYPE_LABELS: Record<string, string> = { Strength: "Strength", Cardio: "Cardio", Plyometric: "Plyo" };

function ExerciseLibrary({ exercises, onSelect }: { exercises: Exercise[]; onSelect: (exercise: Exercise) => void }) {
    const [search, setSearch] = useState("");
    const [activeMusclces, setActiveMuscles] = useState<Set<string>>(new Set());

    const allMuscles = useMemo(() =>
        [...new Set(exercises.flatMap(e => e.targetMuscles))].sort(),
        [exercises]
    );

    const toggleMuscle = (m: string) =>
        setActiveMuscles(prev => {
            const next = new Set(prev);
            if (next.has(m)) next.delete(m); else next.add(m);
            return next;
        });

    const filtered = useMemo(() => {
        const q = search.trim().toLowerCase();
        return exercises.filter(e => {
            const matchesSearch = !q || e.name.toLowerCase().includes(q);
            const matchesMuscle = activeMusclces.size === 0 || e.targetMuscles.some(m => activeMusclces.has(m));
            return matchesSearch && matchesMuscle;
        });
    }, [exercises, search, activeMusclces]);

    const hasFilters = search.trim() !== "" || activeMusclces.size > 0;

    const clearAll = () => {
        setSearch("");
        setActiveMuscles(new Set());
    };

    return (
        <div className="exercise-library">
            <div className="exercise-library-search-wrap">
                <input
                    className="exercise-library-search"
                    type="text"
                    placeholder="Search exercises…"
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                />
                {hasFilters && (
                    <Button style="exercise-library-clear" onClick={clearAll} aria-label="Clear filters" text={"Clear all"} icon={<X size={13} />}/>
                )}
            </div>
            <div className="exercise-library-chips">
                {allMuscles.map(m => (
                    <button
                        key={m}
                        className={`exercise-library-chip${activeMusclces.has(m) ? " active" : ""}`}
                        onClick={() => toggleMuscle(m)}
                    >
                        {m}
                    </button>
                ))}
            </div>
            <hr className="exercise-library-divider" />
            <ul className="exercise-library-list">
                {filtered.length === 0 ? (
                    <p className="exercise-library-empty">No exercises found</p>
                ) : filtered.map(e => (
                    <li key={e.id} className="exercise-library-row" onClick={() => onSelect(e)}>
                        <span className={`template-card-type-badge ${e.type.toLowerCase()}`}>
                            {TYPE_LABELS[e.type] ?? e.type}
                        </span>
                        <div>
                            <p className="exercise-library-name">{e.name}</p>
                            <p className="exercise-library-muscles">{e.targetMuscles.join(", ")}</p>
                        </div>
                    </li>
                ))}
            </ul>
        </div>
    );
}

export default ExerciseLibrary;