import { useState, useMemo, useRef, useEffect } from "react";
import type { Exercise } from "../../exercises/api.ts";
import { X } from "lucide-react";
import Button from "../../../components/general/ButtonComponent.tsx";
import { useRecentExercises } from "../hooks/useRecentExercises.ts";

const TYPE_LABELS: Record<string, string> = { Strength: "Strength", Cardio: "Cardio", Plyometric: "Plyo" };

function ExerciseLibrary({ exercises, onSelect }: { exercises: Exercise[]; onSelect: (exercise: Exercise) => void }) {
    const [search, setSearch] = useState("");
    const [activeMuscles, setActiveMuscles] = useState<Set<string>>(new Set());
    const [highlighted, setHighlighted] = useState(0);
    const searchRef = useRef<HTMLInputElement>(null);
    const listRef = useRef<HTMLUListElement>(null);
    const { recentIds, addRecent } = useRecentExercises();

    useEffect(() => { searchRef.current?.focus(); }, []);

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
            const matchesMuscle = activeMuscles.size === 0 || e.targetMuscles.some(m => activeMuscles.has(m));
            return matchesSearch && matchesMuscle;
        });
    }, [exercises, search, activeMuscles]);

    useEffect(() => { setHighlighted(0); }, [filtered]);

    useEffect(() => {
        const item = listRef.current?.children[highlighted] as HTMLElement | undefined;
        item?.scrollIntoView({ block: "nearest" });
    }, [highlighted]);

    const isFiltering = search.trim() !== "" || activeMuscles.size > 0;

    const recents = useMemo(() =>
        recentIds.map(id => exercises.find(e => e.id === id)).filter(Boolean) as Exercise[],
        [recentIds, exercises]
    );

    function handleSelect(exercise: Exercise) {
        addRecent(exercise.id);
        onSelect(exercise);
    }

    const clearAll = () => {
        setSearch("");
        setActiveMuscles(new Set());
    };

    const hasFilters = isFiltering;

    function handleKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
        if (filtered.length === 0) return;
        if (e.key === "ArrowDown") {
            e.preventDefault();
            setHighlighted(h => Math.min(h + 1, filtered.length - 1));
        } else if (e.key === "ArrowUp") {
            e.preventDefault();
            setHighlighted(h => Math.max(h - 1, 0));
        } else if (e.key === "Enter") {
            e.preventDefault();
            handleSelect(filtered[highlighted]);
        }
    }

    return (
        <div className="exercise-library">
            <div className="exercise-library-search-wrap">
                <input
                    ref={searchRef}
                    className="exercise-library-search"
                    type="text"
                    placeholder="Search exercises…"
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                    onKeyDown={handleKeyDown}
                />
                {hasFilters && (
                    <Button style="exercise-library-clear" onClick={clearAll} aria-label="Clear filters" text={"Clear all"} icon={<X size={13} />} />
                )}
            </div>
            <div className="exercise-library-chips">
                {allMuscles.map(m => (
                    <button
                        key={m}
                        className={`exercise-library-chip${activeMuscles.has(m) ? " active" : ""}`}
                        onClick={() => toggleMuscle(m)}
                    >
                        {m}
                    </button>
                ))}
            </div>
            {!isFiltering && recents.length > 0 && (
                <>
                    <p className="exercise-library-section-label">Recently used</p>
                    <ul className="exercise-library-list exercise-library-recents">
                        {recents.map(e => (
                            <li
                                key={e.id}
                                className="exercise-library-row"
                                onClick={() => handleSelect(e)}
                            >
                                <span className={`type-badge ${e.type.toLowerCase()}`}>
                                    {TYPE_LABELS[e.type] ?? e.type}
                                </span>
                                <div>
                                    <p className="exercise-library-name">{e.name}</p>
                                    <p className="exercise-library-muscles">{e.targetMuscles.join(", ")}</p>
                                </div>
                            </li>
                        ))}
                    </ul>
                </>
            )}
            <hr className="exercise-library-divider" />
            <ul ref={listRef} className="exercise-library-list">
                {filtered.length === 0 ? (
                    <p className="exercise-library-empty">No exercises found</p>
                ) : filtered.map((e, i) => (
                    <li
                        key={e.id}
                        className={`exercise-library-row${i === highlighted ? " highlighted" : ""}`}
                        onClick={() => handleSelect(e)}
                        onMouseEnter={() => setHighlighted(i)}
                    >
                        <span className={`type-badge ${e.type.toLowerCase()}`}>
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