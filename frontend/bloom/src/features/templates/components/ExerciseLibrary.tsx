import { useState, useMemo, useRef, useEffect } from "react";
import { createPortal } from "react-dom";
import type { Exercise } from "../../exercises/api.ts";
import { X, Info, Search, Plus, Pencil, Sparkles } from "lucide-react";
import Button from "../../../components/general/ButtonComponent.tsx";
import ExerciseForm from "../../exercises/components/ExerciseForm.tsx";
import { useRecentExercises } from "../hooks/useRecentExercises.ts";

const TYPE_LABELS: Record<string, string> = { Strength: "Strength", Cardio: "Cardio", Plyometric: "Plyo" };

type TooltipState = { text: string; x: number; y: number } | null;

function InfoTooltip({ description }: { description: string }) {
    const [tooltip, setTooltip] = useState<TooltipState>(null);
    const iconRef = useRef<HTMLDivElement>(null);
    const dismissTimer = useRef<ReturnType<typeof setTimeout> | null>(null);

    function showTooltip() {
        const rect = iconRef.current?.getBoundingClientRect();
        if (!rect) return;
        setTooltip({ text: description, x: rect.right, y: rect.top + rect.height / 2 });
    }

    function handleTouchStart(ev: React.TouchEvent) {
        ev.stopPropagation();
        if (tooltip) {
            setTooltip(null);
            if (dismissTimer.current) clearTimeout(dismissTimer.current);
        } else {
            showTooltip();
            dismissTimer.current = setTimeout(() => setTooltip(null), 2500);
        }
    }

    useEffect(() => () => { if (dismissTimer.current) clearTimeout(dismissTimer.current); }, []);

    return (
        <div
            ref={iconRef}
            className="exercise-library-row-info"
            onClick={ev => ev.stopPropagation()}
            onMouseEnter={showTooltip}
            onMouseLeave={() => setTooltip(null)}
            onTouchStart={handleTouchStart}
        >
            <Info size={14} />
            {tooltip && createPortal(
                <div
                    className="exercise-library-row-tooltip"
                    style={{ top: tooltip.y, left: tooltip.x }}
                >
                    {tooltip.text}
                </div>,
                document.body
            )}
        </div>
    );
}

type View = { mode: "browse" } | { mode: "create"; name: string } | { mode: "edit"; exercise: Exercise };

interface ExerciseLibraryProps {
    exercises: Exercise[];
    onSelect: (exercise: Exercise) => void;
    /** Called after a custom exercise is created or edited so the parent can upsert it. */
    onSaved?: (exercise: Exercise) => void;
}

function ExerciseRow({ exercise: e, highlighted, onSelect, onHover, onEdit }: {
    exercise: Exercise;
    highlighted?: boolean;
    onSelect: () => void;
    onHover?: () => void;
    onEdit?: () => void;
}) {
    return (
        <li
            className={`exercise-library-row${highlighted ? " highlighted" : ""}`}
            onClick={onSelect}
            onMouseEnter={onHover}
        >
            <span className={`type-dot ${e.type.toLowerCase()}`} data-label={TYPE_LABELS[e.type] ?? e.type} />
            <div>
                <p className="exercise-library-name">
                    {e.name}
                    {e.isCustom && <span className="exercise-custom-badge">Custom</span>}
                </p>
                <p className="exercise-library-muscles">{e.targetMuscles.join(", ")}</p>
            </div>
            <span className="exercise-library-row-end">
                {e.isCustom && onEdit && (
                    <button
                        type="button"
                        className="exercise-library-edit"
                        aria-label={`Edit ${e.name}`}
                        onClick={ev => { ev.stopPropagation(); onEdit(); }}
                    >
                        <Pencil size={12} />
                    </button>
                )}
                {e.description ? <InfoTooltip description={e.description} /> : null}
            </span>
        </li>
    );
}

function ExerciseLibrary({ exercises: allExercises, onSelect, onSaved }: ExerciseLibraryProps) {
    const [view, setView] = useState<View>({ mode: "browse" });
    const [deletedIds, setDeletedIds] = useState<Set<string>>(new Set());
    const [onlyCustom, setOnlyCustom] = useState(false);
    const exercises = useMemo(
        () => allExercises.filter(e => !deletedIds.has(e.id)),
        [allExercises, deletedIds]
    );
    const [search, setSearch] = useState("");
    const [activeMuscles, setActiveMuscles] = useState<Set<string>>(new Set());
    const [highlighted, setHighlighted] = useState(0);
    const searchRef = useRef<HTMLInputElement>(null);
    const listRef = useRef<HTMLUListElement>(null);
    const { recentIds, addRecent } = useRecentExercises();

    useEffect(() => { if (view.mode === "browse") searchRef.current?.focus(); }, [view.mode]);

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
            return matchesSearch && matchesMuscle && (!onlyCustom || e.isCustom);
        });
    }, [exercises, search, activeMuscles, onlyCustom]);
    const hasCustom = exercises.some(e => e.isCustom);

    useEffect(() => { setHighlighted(0); }, [filtered]);

    useEffect(() => {
        const item = listRef.current?.children[highlighted] as HTMLElement | undefined;
        item?.scrollIntoView({ block: "nearest" });
    }, [highlighted]);

    const isFiltering = search.trim() !== "" || activeMuscles.size > 0 || onlyCustom;

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
        setOnlyCustom(false);
    };

    function handleFormSaved(saved: Exercise) {
        onSaved?.(saved);
        if (view.mode === "create") {
            handleSelect(saved);
        } else {
            setView({ mode: "browse" });
        }
    }

    function handleFormDeleted(id: string) {
        setDeletedIds(prev => new Set(prev).add(id));
        setView({ mode: "browse" });
    }

    if (view.mode !== "browse") {
        return (
            <div className="exercise-library">
                <ExerciseForm
                    key={view.mode === "edit" ? view.exercise.id : "new"}
                    exercise={view.mode === "edit" ? view.exercise : undefined}
                    initialName={view.mode === "create" ? view.name : undefined}
                    knownMuscles={allMuscles}
                    onSaved={handleFormSaved}
                    onDeleted={handleFormDeleted}
                    onCancel={() => setView({ mode: "browse" })}
                />
            </div>
        );
    }

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
                <Search className="exercise-library-search-icon" size={14} />
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
                <button
                    type="button"
                    className="exercise-library-new"
                    onClick={() => setView({ mode: "create", name: search.trim() })}
                >
                    <Plus size={13} /> New
                </button>
            </div>
            <div className="exercise-library-chips">
                {hasCustom && (
                    <button
                        className={`exercise-library-chip custom${onlyCustom ? " active" : ""}`}
                        onClick={() => setOnlyCustom(v => !v)}
                    >
                        <Sparkles size={10} /> My exercises
                    </button>
                )}
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
                            <ExerciseRow
                                key={e.id}
                                exercise={e}
                                onSelect={() => handleSelect(e)}
                                onEdit={() => setView({ mode: "edit", exercise: e })}
                            />
                        ))}
                    </ul>
                </>
            )}
            <hr className="exercise-library-divider" />
            <ul ref={listRef} className="exercise-library-list">
                {filtered.length === 0 ? (
                    <div className="exercise-library-empty">
                        <p>No exercises found</p>
                        <button
                            type="button"
                            className="exercise-library-create-cta"
                            onClick={() => setView({ mode: "create", name: search.trim() })}
                        >
                            <Plus size={13} />
                            {search.trim() ? <>Create “{search.trim()}”</> : "Create a custom exercise"}
                        </button>
                    </div>
                ) : filtered.map((e, i) => (
                    <ExerciseRow
                        key={e.id}
                        exercise={e}
                        highlighted={i === highlighted}
                        onSelect={() => handleSelect(e)}
                        onHover={() => setHighlighted(i)}
                        onEdit={() => setView({ mode: "edit", exercise: e })}
                    />
                ))}
            </ul>
        </div>
    );
}

export default ExerciseLibrary;
