import { useState, useMemo, useRef, useEffect, useLayoutEffect, useCallback, useDeferredValue, memo } from "react";
import { createPortal } from "react-dom";
import type { Exercise } from "../../exercises/api.ts";
import { X, Info, Search, Plus, Pencil, Sparkles } from "lucide-react";
import Button from "../../../components/general/ButtonComponent.tsx";
import ExerciseForm from "../../exercises/components/ExerciseForm.tsx";
import { useRecentExercises } from "../hooks/useRecentExercises.ts";
import { useLoggedExerciseUsage } from "../hooks/useLoggedExerciseUsage.ts";
import { matchScore } from "../exerciseSearch.ts";

/** How many exercises the "Recently used" list shows. */
const RECENT_COUNT = 8;

const TYPE_LABELS: Record<string, string> = { Strength: "Strength", Cardio: "Cardio", Plyometric: "Plyo" };

/** Rows rendered per page; more are added as you scroll or arrow towards the end. */
const PAGE = 60;

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

/**
 * Memoised: with 300+ exercises, re-rendering every row on each keystroke or
 * arrow-key press is what made the menu lag. Handlers receive the exercise so
 * the parent can pass the same function to every row.
 */
const ExerciseRow = memo(function ExerciseRow({ exercise: e, highlighted, onSelect, onEdit }: {
    exercise: Exercise;
    highlighted?: boolean;
    onSelect: (exercise: Exercise) => void;
    onEdit?: (exercise: Exercise) => void;
}) {
    return (
        <li
            className={`exercise-library-row${highlighted ? " highlighted" : ""}`}
            onClick={() => onSelect(e)}
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
                        onClick={ev => { ev.stopPropagation(); onEdit(e); }}
                    >
                        <Pencil size={12} />
                    </button>
                )}
                {e.description ? <InfoTooltip description={e.description} /> : null}
            </span>
        </li>
    );
});

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
    const searchRef = useRef<HTMLInputElement>(null);
    const listRef = useRef<HTMLUListElement>(null);
    const { recentIds, addRecent } = useRecentExercises();
    const usage = useLoggedExerciseUsage();

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

    // Typing updates the input straight away; the list catches up a moment later.
    const deferredSearch = useDeferredValue(search);
    const filtered = useMemo(() => {
        const q = deferredSearch.trim();
        const matches: { e: Exercise; score: number; i: number }[] = [];
        exercises.forEach((e, i) => {
            if (activeMuscles.size > 0 && !e.targetMuscles.some(m => activeMuscles.has(m))) return;
            if (onlyCustom && !e.isCustom) return;
            const score = q ? matchScore(e.name, q) : 0;
            if (score !== null) matches.push({ e, score, i });
        });
        // Searching: best match first, then the ones you log most, then A–Z.
        if (q) {
            matches.sort((a, b) =>
                a.score - b.score
                || (usage.counts.get(b.e.id) ?? 0) - (usage.counts.get(a.e.id) ?? 0)
                || a.i - b.i);
        }
        return matches.map(m => m.e);
    }, [exercises, deferredSearch, activeMuscles, onlyCustom, usage]);
    const hasCustom = exercises.some(e => e.isCustom);

    // Keyboard highlight; the mouse uses CSS :hover so moving over rows never re-renders the list.
    // Kept per filter result, so a new search starts at the top again.
    const [highlight, setHighlight] = useState<{ list: Exercise[]; index: number }>({ list: filtered, index: 0 });
    const highlighted = highlight.list === filtered ? highlight.index : 0;

    // Mounting all 300+ rows at once (e.g. when the search is cleared) froze the
    // input; render a page and add the next one when the end comes into view.
    const [shown, setShown] = useState<{ list: Exercise[]; count: number }>({ list: filtered, count: PAGE });
    const visibleCount = shown.list === filtered ? shown.count : PAGE;
    const visible = filtered.length > visibleCount ? filtered.slice(0, visibleCount) : filtered;
    const sentinelRef = useRef<HTMLLIElement>(null);
    const hasMore = visible.length < filtered.length;

    useEffect(() => {
        const el = sentinelRef.current;
        if (!el || !hasMore) return;
        const io = new IntersectionObserver(entries => {
            if (entries.some(e => e.isIntersecting)) setShown({ list: filtered, count: visibleCount + PAGE });
        }, { rootMargin: "400px 0px" });
        io.observe(el);
        return () => io.disconnect();
    }, [filtered, visibleCount, hasMore]);

    function moveHighlight(index: number) {
        if (index >= visibleCount - 5) setShown({ list: filtered, count: visibleCount + PAGE });
        setHighlight({ list: filtered, index });
        const item = listRef.current?.children[index] as HTMLElement | undefined;
        item?.scrollIntoView({ block: "nearest" });
    }

    const isFiltering = search.trim() !== "" || activeMuscles.size > 0 || onlyCustom;

    // What you've logged lately (from your logbook, so it follows you across devices),
    // topped up with picks made in this browser that aren't logged yet.
    const recents = useMemo(() => {
        const byId = new Map(exercises.map(e => [e.id, e]));
        const ids = [...new Set([...usage.recentIds, ...recentIds])];
        return ids.map(id => byId.get(id)).filter((e): e is Exercise => !!e).slice(0, RECENT_COUNT);
    }, [usage.recentIds, recentIds, exercises]);

    // The parents pass inline callbacks; read the latest ones through a ref so
    // handleSelect keeps one identity and the memoised rows don't re-render.
    const latest = useRef({ onSelect, addRecent });
    useLayoutEffect(() => { latest.current = { onSelect, addRecent }; });
    const handleSelect = useCallback((exercise: Exercise) => {
        latest.current.addRecent(exercise.id);
        latest.current.onSelect(exercise);
    }, []);
    const handleEdit = useCallback((exercise: Exercise) => setView({ mode: "edit", exercise }), []);

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
            moveHighlight(Math.min(highlighted + 1, filtered.length - 1));
        } else if (e.key === "ArrowUp") {
            e.preventDefault();
            moveHighlight(Math.max(highlighted - 1, 0));
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
                            <ExerciseRow key={e.id} exercise={e} onSelect={handleSelect} onEdit={handleEdit} />
                        ))}
                    </ul>
                </>
            )}
            <hr className="exercise-library-divider" />
            <ul ref={listRef} className={`exercise-library-list${deferredSearch !== search ? " is-stale" : ""}`}>
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
                ) : visible.map((e, i) => (
                    <ExerciseRow key={e.id} exercise={e} highlighted={i === highlighted} onSelect={handleSelect} onEdit={handleEdit} />
                ))}
                {hasMore && <li ref={sentinelRef} className="exercise-library-more" aria-hidden />}
            </ul>
        </div>
    );
}

export default ExerciseLibrary;
