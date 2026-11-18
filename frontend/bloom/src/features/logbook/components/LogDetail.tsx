import { useState, useEffect, useRef, useMemo } from "react";
import { DndContext, closestCenter, DragOverlay } from "@dnd-kit/core";
import { SortableContext, verticalListSortingStrategy } from "@dnd-kit/sortable";
import type { LoggedWorkout, LoggedExercise, LoggedSet } from "../api.ts";
import { updateLog, createLog } from "../api.ts";
import type { Exercise } from "../../exercises/api.ts";
import { toDateInputValue, withDate, formatDate, prefilledFirstSet, resolveHints, findMissingReps, workingSetNumbers, type PreviousSetsLookup } from "../logbookUtils.ts";
import ExerciseDragChip from "../../templates/components/ExerciseDragChip.tsx";
import { useExerciseDnd } from "../../templates/hooks/useExerciseDnd.ts";
import { useShortcut } from "../../../hooks/useShortcut.ts";
import LogExerciseCard from "./LogExerciseCard.tsx";
import LogSummary from "./LogSummary.tsx";
import LogReadView from "./LogReadView.tsx";
import { readOpenReadOnly } from "../logViewPref.ts";
import Button from "../../../components/general/ButtonComponent.tsx";
import Overlay from "../../../components/general/OverlayComponent.tsx";
import ExerciseLibrary from "../../templates/components/ExerciseLibrary.tsx";
import { Save, Trash2, Plus, MoreHorizontal, BookmarkPlus, Pencil, Rows3 } from "lucide-react";

const NO_LOGS: LoggedWorkout[] = [];

interface LogDetailProps {
    log: LoggedWorkout;
    exercises: Record<string, Exercise>;
    onSave: (id: string, name: string, loggedAt: string, note: string | null, exercises: LoggedExercise[]) => void;
    onDelete: (id: string) => void;
    onCreateTemplate: (name: string, exercises: LoggedExercise[]) => void;
    onDirtyChange?: (isDirty: boolean, save: () => Promise<void>) => void;
    onExerciseCreated?: (exercise: Exercise) => void;
    userGear?: string[];
    autoFocusTitle?: boolean;
    openMap?: boolean;
    /** A local, not-yet-saved log: Save creates it on the server. */
    isDraft?: boolean;
    /** Called on every edit of a draft so it can be kept in local storage. */
    onDraftChange?: (draft: LoggedWorkout) => void;
    /** Called once a draft has been saved and now has a real id. */
    onDraftSaved?: (log: LoggedWorkout) => void;
    /** Last time's sets per exercise, used to prefill newly added exercises. */
    previousSets?: PreviousSetsLookup;
    /** Every log, for the summary's "vs last time" and records. */
    history?: LoggedWorkout[];
}

function LogDetail({
    log, exercises, onSave, onDelete, onCreateTemplate, onDirtyChange, onExerciseCreated,
    userGear, autoFocusTitle, openMap, isDraft = false, onDraftChange, onDraftSaved, previousSets, history = NO_LOGS,
}: LogDetailProps) {
    const initialDate = toDateInputValue(log.loggedAt);

    // Stable list for the exercise library, so it doesn't re-filter on every render of this view.
    const exerciseList = useMemo(() => Object.values(exercises), [exercises]);
    const [logExercises, setLogExercises] = useState<LoggedExercise[]>(() =>
        [...log.exercises].sort((a, b) => a.order - b.order)
    );
    const [name, setName] = useState(log.name);
    const [date, setDate] = useState(initialDate);
    // True while the date input holds an impossible day/month (31 November mid-typing).
    const [dateIncomplete, setDateIncomplete] = useState(false);
    // Compact read-only view: per log, starts on when the "open logs read-only" setting is on.
    const [readOnly, setReadOnly] = useState(() => !isDraft && !autoFocusTitle && readOpenReadOnly());
    // Exercise to scroll to once the editor has rendered (clicked in the read view).
    const [jumpTo, setJumpTo] = useState<number | null>(null);

    useEffect(() => {
        if (jumpTo == null || readOnly) return;
        const card = document.querySelector<HTMLElement>(`[data-exercise-id="${jumpTo}"]`);
        card?.scrollIntoView({ behavior: "smooth", block: "start" });
        card?.querySelector<HTMLInputElement>("section input")?.focus({ preventScroll: true });
        setJumpTo(null);
    }, [jumpTo, readOnly]);

    function editExercise(order: number) {
        setReadOnly(false);
        setJumpTo(order);
    }
    const [note, setNote] = useState(log.note ?? "");
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [addExerciseOpen, setAddExerciseOpen] = useState(false);
    const [lastAddedOrder, setLastAddedOrder] = useState<number | null>(null);
    const [showSticky, setShowSticky] = useState(false);
    const [panelTop, setPanelTop] = useState(0);
    const [menuOpen, setMenuOpen] = useState(false);
    const actionsRef = useRef<HTMLDivElement>(null);
    const menuRef = useRef<HTMLDivElement>(null);
    const titleInputRef = useRef<HTMLInputElement>(null);

    useEffect(() => {
        if (!autoFocusTitle) return;
        titleInputRef.current?.focus();
        titleInputRef.current?.select();
    }, []);

    const { activeExercise, isDragging, onDragStart, onDragEnd, onDragCancel, dndProps, overlayModifiers } = useExerciseDnd(
        logExercises,
        reordered => { setLogExercises(reordered); setLastAddedOrder(null); },
        // Order, not exerciseId: the same exercise can be logged twice.
        ex => String(ex.order)
    );

    // Canonical shape so optional fields (note/gear/marker) missing on freshly
    // added items compare equal to the server's explicit null / [] values.
    const normalize = (exs: LoggedExercise[]) =>
        [...exs].sort((a, b) => a.order - b.order).map(ex => ({
            exerciseId: ex.exerciseId,
            order: ex.order,
            gpxData: ex.gpxData ?? null,
            note: ex.note?.trim() ? ex.note : null,
            gear: ex.gear ?? [],
            sets: [...ex.sets].sort((a, b) => a.order - b.order).map(s => ({
                type: s.type, order: s.order, duration: s.duration ?? null,
                distance: s.distance ?? null, distanceUnit: s.distanceUnit ?? null,
                reps: s.reps ?? null, weight: s.weight ?? null, weightUnit: s.weightUnit ?? null,
                rir: s.rir ?? null, marker: s.marker ?? null,
            })),
        }));

    const hasChanges = name !== log.name
        || date !== initialDate
        || note !== (log.note ?? "")
        || JSON.stringify(normalize(logExercises)) !== JSON.stringify(normalize(log.exercises));
    // A draft is always saveable, but never "unsaved": it is kept locally on every edit.
    const canSave = !dateIncomplete && (isDraft ? logExercises.length > 0 || name.trim() !== "" : hasChanges);
    const currentLoggedAt = () => (date === initialDate ? log.loggedAt : withDate(log.loggedAt, date));

    const handleSaveRef = useRef(handleSave);
    handleSaveRef.current = handleSave;
    useEffect(() => {
        onDirtyChange?.(!isDraft && hasChanges, () => handleSaveRef.current());
    }, [hasChanges, isDraft]); // eslint-disable-line react-hooks/exhaustive-deps

    useEffect(() => {
        if (!isDraft) return;
        onDraftChange?.({ ...log, name, note: note || null, loggedAt: currentLoggedAt(), exercises: logExercises });
    }, [isDraft, name, note, date, logExercises]); // eslint-disable-line react-hooks/exhaustive-deps

    useEffect(() => {
        const el = actionsRef.current;
        if (!el) return;
        const panel = el.closest('.panel-detail') as HTMLElement | null;
        const top = Math.round(panel?.getBoundingClientRect().top ?? 0);
        setPanelTop(top);
        const observer = new IntersectionObserver(
            ([entry]) => setShowSticky(!entry.isIntersecting),
            { rootMargin: `-${top}px 0px 0px 0px`, threshold: 0 }
        );
        observer.observe(el);
        return () => observer.disconnect();
    }, []);

    useEffect(() => {
        if (!menuOpen) return;
        function handleClickOutside(e: MouseEvent) {
            if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
                setMenuOpen(false);
            }
        }
        document.addEventListener("mousedown", handleClickOutside);
        return () => document.removeEventListener("mousedown", handleClickOutside);
    }, [menuOpen]);

    function handleSetsChange(exerciseOrder: number, sets: LoggedSet[]) {
        setLogExercises(prev =>
            prev.map(ex => ex.order === exerciseOrder ? { ...ex, sets } : ex)
        );
    }

    function handleGpxChange(exerciseOrder: number, gpxData: string | null) {
        setLogExercises(prev =>
            prev.map(ex => ex.order === exerciseOrder ? { ...ex, gpxData } : ex)
        );
    }

    function handleNoteChange(exerciseOrder: number, exNote: string) {
        setLogExercises(prev =>
            prev.map(ex => ex.order === exerciseOrder ? { ...ex, note: exNote } : ex)
        );
    }

    function handleGearChange(exerciseOrder: number, gear: string[]) {
        setLogExercises(prev =>
            prev.map(ex => ex.order === exerciseOrder ? { ...ex, gear } : ex)
        );
    }

    function handleDeleteExercise(exerciseOrder: number) {
        setLogExercises(prev =>
            prev.filter(ex => ex.order !== exerciseOrder).map((ex, i) => ({ ...ex, order: i + 1 }))
        );
    }

    function handleAddExercise(exerciseId: string, typeHint?: string) {
        const type = typeHint ?? exercises[exerciseId]?.type ?? "Strength";
        // First set = what you did first last time; "Add set" then copies the row above.
        const firstSet = prefilledFirstSet(type, previousSets?.(exerciseId, currentLoggedAt()));
        setLogExercises(prev => [
            ...prev,
            { exerciseId, order: prev.length + 1, sets: [firstSet], gpxData: null, note: null, gear: [] },
        ]);
        setLastAddedOrder(logExercises.length + 1);
        setAddExerciseOpen(false);
    }

    useShortcut("s", handleSave, true);
    useShortcut("e", () => { setReadOnly(false); setAddExerciseOpen(true); }, true);
    useShortcut("Enter", () => {
        const focused = document.activeElement;
        if (focused?.tagName === "TEXTAREA") return;
        const card = focused?.closest("[data-exercise-id]");
        const id = card?.getAttribute("data-exercise-id") ?? logExercises.at(-1)?.order.toString();
        if (!id) return;
        const btn = document.querySelector<HTMLButtonElement>(`[data-exercise-id="${id}"] .exercise-footer button`);
        btn?.click();
    }, true);

    async function handleSave() {
        if (saving || !canSave) return;
        setSaving(true);
        setError(null);
        try {
            const missing = findMissingReps(logExercises);
            if (missing) {
                const name = exercises[missing.exercise.exerciseId]?.name ?? "An exercise";
                const n = workingSetNumbers(missing.exercise.sets)[missing.setIndex];
                setError(`${name}: fill in the reps for ${n ? `set ${n}` : "the warm-up set"}, or remove it.`);
                // Take the user to the empty field; the banner is out of sight when scrolled down.
                const row = document.querySelectorAll(`[data-exercise-id="${missing.exercise.order}"] .set-row`)[missing.setIndex];
                row?.querySelector<HTMLInputElement>("input")?.focus();
                return;
            }
            const loggedAt = currentLoggedAt();
            const cleaned = resolveHints(logExercises).map(ex => ({ ...ex, note: ex.note?.trim() ? ex.note.trim() : null }));
            const finalName = name.trim() || "Untitled workout";
            if (isDraft) {
                const id = await createLog(finalName, cleaned, note || null, loggedAt);
                onDraftSaved?.({ ...log, id, name: finalName, loggedAt, note: note || null, exercises: cleaned });
                return;
            }
            await updateLog(log.id, finalName, loggedAt, cleaned, note || null);
            setLogExercises(cleaned);
            onSave(log.id, finalName, loggedAt, note || null, cleaned);
        } catch (e) {
            setError(e instanceof Error ? e.message : "Failed to save log");
        } finally {
            setSaving(false);
        }
    }

    return (
        <div className="detail-view">
            {addExerciseOpen && (
                <Overlay title="Add exercise" panelClassName="overlay-panel-fixed" onClose={() => setAddExerciseOpen(false)}>
                    <ExerciseLibrary
                        exercises={exerciseList}
                        onSelect={e => handleAddExercise(e.id, e.type)}
                        onSaved={onExerciseCreated}
                    />
                </Overlay>
            )}

            {error && <div className="error-banner">{error}</div>}

            {showSticky && canSave && (
                <div className="sticky-save-bar" style={{ top: panelTop }}>
                    <Button text={isDraft ? "Save log" : "Save changes"} style="green" icon={<Save size={14} />} disabled={saving} onClick={handleSave} />
                </div>
            )}
            <div className="detail-header">
                <div className="log-detail-title-group">
                    {readOnly ? (
                        <h1 className="template-title-input log-read-title">{name || "Untitled workout"}</h1>
                    ) : (
                        <input
                            ref={titleInputRef}
                            className="template-title-input"
                            value={name}
                            placeholder="Name this workout"
                            aria-label="Workout name"
                            onChange={e => setName(e.target.value)}
                        />
                    )}
                    <div className="log-detail-meta">
                        {readOnly ? (
                            <span className="log-read-date">{formatDate(currentLoggedAt())}</span>
                        ) : (
                        <input
                            type="date"
                            className="log-date-input"
                            aria-label="Workout date"
                            // Uncontrolled: the input reports "" while half-typed, and writing that back
                            // clears every segment (Firefox). Only complete dates reach state.
                            // `date` only ever holds complete dates; read once, when the editor opens.
                            defaultValue={date}
                            onChange={e => {
                                const v = e.target.value;
                                setDateIncomplete(!v);
                                if (v) setDate(v);
                            }}
                        />
                        )}
                        {isDraft && (
                            <span className="draft-badge" title="Kept on this device until you save">
                                Draft · not saved yet
                            </span>
                        )}
                    </div>
                </div>
                <div className="actions-row" ref={actionsRef}>
                    {readOnly ? (
                        <>
                            {canSave && <Button text="Save changes" style="green" icon={<Save size={14} />} disabled={saving} onClick={handleSave} />}
                            <Button text="Edit" style={canSave ? "white" : "green"} icon={<Pencil size={14} />} onClick={() => setReadOnly(false)} />
                        </>
                    ) : (
                        <>
                            <Button text={isDraft ? "Save log" : "Save changes"} style="green" icon={<Save size={14} />} disabled={!canSave || saving} onClick={handleSave} />
                            {!isDraft && (
                                <button
                                    type="button"
                                    className="icon-btn"
                                    aria-label="Compact view"
                                    title="Compact view: read-only and denser"
                                    onClick={() => setReadOnly(true)}
                                >
                                    <Rows3 size={15} />
                                </button>
                            )}
                        </>
                    )}
                    <div className="overflow-menu" ref={menuRef}>
                        <button
                            className="overflow-menu-trigger"
                            aria-label="More actions"
                            onClick={() => setMenuOpen(o => !o)}
                        >
                            <MoreHorizontal size={16} />
                        </button>
                        {menuOpen && (
                            <div className="overflow-menu-dropdown">
                                <button
                                    className="overflow-menu-item"
                                    onClick={() => { setMenuOpen(false); onCreateTemplate(name, resolveHints(logExercises)); }}
                                >
                                    <BookmarkPlus size={14} />
                                    Create template from log
                                </button>
                                <button
                                    className="overflow-menu-item danger"
                                    onClick={() => { setMenuOpen(false); onDelete(log.id); }}
                                >
                                    <Trash2 size={14} />
                                    {isDraft ? "Discard draft" : "Delete log"}
                                </button>
                            </div>
                        )}
                    </div>
                </div>
            </div>

            <LogSummary name={name} loggedAt={currentLoggedAt()} exercises={logExercises} exerciseInfo={exercises} history={history} />

            {readOnly ? (
                <>
                    {note.trim() && <p className="log-read-workout-note">{note}</p>}
                    <LogReadView exercises={logExercises} exerciseInfo={exercises} onEditExercise={editExercise} />
                </>
            ) : (<>
            <textarea
                className="log-note-area"
                placeholder="Add a note about this workout…"
                value={note}
                onChange={e => setNote(e.target.value)}
            />

            <DndContext collisionDetection={closestCenter} onDragStart={onDragStart} onDragEnd={onDragEnd} onDragCancel={onDragCancel} {...dndProps}>
                <div className={`exercise-list${isDragging ? " is-dragging" : ""}`}>
                <SortableContext items={logExercises.map(ex => String(ex.order))} strategy={verticalListSortingStrategy}>
                    {logExercises.map((ex, i) => (
                        <LogExerciseCard
                            // Cards keep sets/UI in local state, so remount when a slot gets a different exercise.
                            key={`${ex.order}-${ex.exerciseId}`}
                            id={String(ex.order)}
                            exercise={ex}
                            exerciseInfo={exercises[ex.exerciseId]}
                            onSetsChange={handleSetsChange}
                            onDelete={handleDeleteExercise}
                            onGpxChange={handleGpxChange}
                            onNoteChange={handleNoteChange}
                            onGearChange={handleGearChange}
                            userGear={userGear}
                            autoFocus={ex.order === lastAddedOrder}
                            autoOpenMap={openMap && i === logExercises.findIndex(e => !!e.gpxData)}
                        />
                    ))}
                </SortableContext>
                </div>
                <DragOverlay modifiers={overlayModifiers}>
                    {activeExercise && (
                        <ExerciseDragChip name={exercises[activeExercise.exerciseId]?.name ?? "Exercise"} sets={activeExercise.sets.length} />
                    )}
                </DragOverlay>
            </DndContext>

            {logExercises.length === 0 && (
                <div className="log-empty-exercises">
                    <p>No exercises yet.</p>
                    <p>Add one and last time's numbers show in grey; leave a field empty to keep them.</p>
                </div>
            )}
            <Button text="Add exercise" style="grey" icon={<Plus size={15} />} onClick={() => setAddExerciseOpen(true)} />
            </>)}
        </div>
    );
}

export default LogDetail;