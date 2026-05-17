import { useState, useEffect, useRef } from "react";
import { DndContext, closestCenter, DragOverlay } from "@dnd-kit/core";
import { SortableContext, verticalListSortingStrategy } from "@dnd-kit/sortable";
import type { LoggedWorkout, LoggedExercise, LoggedSet } from "../api.ts";
import { updateLog } from "../api.ts";
import type { Exercise } from "../../exercises/api.ts";
import { toDateInputValue } from "../logbookUtils.ts";
import { useExerciseDnd } from "../../templates/hooks/useExerciseDnd.ts";
import { useShortcut } from "../../../hooks/useShortcut.ts";
import LogExerciseCard from "./LogExerciseCard.tsx";
import Button from "../../../components/general/ButtonComponent.tsx";
import Overlay from "../../../components/general/OverlayComponent.tsx";
import ExerciseLibrary from "../../templates/components/ExerciseLibrary.tsx";
import { Save, Trash2, Plus, MoreHorizontal, BookmarkPlus } from "lucide-react";

interface LogDetailProps {
    log: LoggedWorkout;
    exercises: Record<string, Exercise>;
    onSave: (id: string, name: string, loggedAt: string, note: string | null, exercises: LoggedExercise[]) => void;
    onDelete: (id: string) => void;
    onCreateTemplate: (name: string, exercises: LoggedExercise[]) => void;
    onDirtyChange?: (isDirty: boolean, save: () => Promise<void>) => void;
    autoFocusTitle?: boolean;
}

function LogDetail({ log, exercises, onSave, onDelete, onCreateTemplate, onDirtyChange, autoFocusTitle }: LogDetailProps) {
    const initialDate = toDateInputValue(log.loggedAt);

    const [logExercises, setLogExercises] = useState<LoggedExercise[]>(() =>
        [...log.exercises].sort((a, b) => a.order - b.order)
    );
    const [name, setName] = useState(log.name);
    const [date, setDate] = useState(initialDate);
    const [note, setNote] = useState(log.note ?? "");
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [addExerciseOpen, setAddExerciseOpen] = useState(false);
    const [lastAddedId, setLastAddedId] = useState<string | null>(null);
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

    const { activeExercise, onDragStart, onDragEnd, onDragCancel } = useExerciseDnd(
        logExercises,
        reordered => setLogExercises(reordered)
    );

    const normalize = (exs: LoggedExercise[]) =>
        [...exs].sort((a, b) => a.order - b.order).map(ex => ({
            ...ex,
            sets: [...ex.sets].sort((a, b) => a.order - b.order),
        }));

    const hasChanges = name !== log.name
        || date !== initialDate
        || note !== (log.note ?? "")
        || JSON.stringify(normalize(logExercises)) !== JSON.stringify(normalize(log.exercises));

    const handleSaveRef = useRef(handleSave);
    handleSaveRef.current = handleSave;
    useEffect(() => {
        onDirtyChange?.(hasChanges, () => handleSaveRef.current());
    }, [hasChanges]); // eslint-disable-line react-hooks/exhaustive-deps

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

    function handleSetsChange(exerciseId: string, sets: LoggedSet[]) {
        setLogExercises(prev =>
            prev.map(ex => ex.exerciseId === exerciseId ? { ...ex, sets } : ex)
        );
    }

    function handleGpxChange(exerciseId: string, gpxData: string | null) {
        setLogExercises(prev =>
            prev.map(ex => ex.exerciseId === exerciseId ? { ...ex, gpxData } : ex)
        );
    }

    function handleDeleteExercise(exerciseId: string) {
        setLogExercises(prev =>
            prev.filter(ex => ex.exerciseId !== exerciseId).map((ex, i) => ({ ...ex, order: i + 1 }))
        );
    }

    function handleAddExercise(exerciseId: string) {
        const type = exercises[exerciseId]?.type ?? "Strength";
        const defaultSet: LoggedSet = type === "Cardio"
            ? { type: "Cardio", order: 1, reps: null, weight: null, weightUnit: null, rir: null, duration: "00:30:00", distance: 5, distanceUnit: "km" }
            : { type, order: 1, reps: 10, weight: 60, weightUnit: "kg", rir: 2, duration: null, distance: null, distanceUnit: null };
        setLogExercises(prev => [
            ...prev,
            { exerciseId, order: prev.length + 1, sets: [defaultSet], gpxData: null },
        ]);
        setLastAddedId(exerciseId);
        setAddExerciseOpen(false);
    }

    useShortcut("s", handleSave, true);
    useShortcut("e", () => setAddExerciseOpen(true), true);
    useShortcut("Enter", () => {
        const focused = document.activeElement;
        if (focused?.tagName === "TEXTAREA") return;
        const card = focused?.closest("[data-exercise-id]");
        const id = card?.getAttribute("data-exercise-id") ?? logExercises.at(-1)?.exerciseId;
        if (!id) return;
        const btn = document.querySelector<HTMLButtonElement>(`[data-exercise-id="${id}"] .exercise-footer button`);
        btn?.click();
    }, true);

    async function handleSave() {
        setSaving(true);
        setError(null);
        try {
            const loggedAt = new Date(date).toISOString();
            await updateLog(log.id, name, loggedAt, logExercises, note || null);
            onSave(log.id, name, loggedAt, note || null, logExercises);
        } catch (e) {
            setError(e instanceof Error ? e.message : "Failed to save log");
        } finally {
            setSaving(false);
        }
    }

    return (
        <div className="detail-view">
            {addExerciseOpen && (
                <Overlay title="Exercise library" subtitle="Add exercise" onClose={() => setAddExerciseOpen(false)}>
                    <ExerciseLibrary
                        exercises={Object.values(exercises)}
                        onSelect={e => handleAddExercise(e.id)}
                    />
                </Overlay>
            )}

            {error && <div className="error-banner">{error}</div>}

            {showSticky && hasChanges && (
                <div className="sticky-save-bar" style={{ top: panelTop }}>
                    <Button text="Save Changes" style="green" icon={<Save size={14} />} disabled={saving} onClick={handleSave} />
                </div>
            )}
            <div className="detail-header">
                <div className="log-detail-title-group">
                    <input
                        ref={titleInputRef}
                        className="template-title-input"
                        value={name}
                        onChange={e => setName(e.target.value)}
                    />
                    <input
                        type="date"
                        className="log-date-input"
                        value={date}
                        onChange={e => setDate(e.target.value)}
                    />
                </div>
                <div className="actions-row" ref={actionsRef}>
                    <Button text="Save Changes" style="green" icon={<Save size={14} />} disabled={!hasChanges || saving} onClick={handleSave} />
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
                                    onClick={() => { setMenuOpen(false); onCreateTemplate(name, logExercises); }}
                                >
                                    <BookmarkPlus size={14} />
                                    Create Template from Log
                                </button>
                                <button
                                    className="overflow-menu-item danger"
                                    onClick={() => { setMenuOpen(false); onDelete(log.id); }}
                                >
                                    <Trash2 size={14} />
                                    Delete Log
                                </button>
                            </div>
                        )}
                    </div>
                </div>
            </div>

            <textarea
                className="log-note-area"
                placeholder="Add a note about this workout…"
                value={note}
                onChange={e => setNote(e.target.value)}
            />

            <DndContext collisionDetection={closestCenter} onDragStart={onDragStart} onDragEnd={onDragEnd} onDragCancel={onDragCancel}>
                <SortableContext items={logExercises.map(ex => ex.exerciseId)} strategy={verticalListSortingStrategy}>
                    {logExercises.map(ex => (
                        <LogExerciseCard
                            key={ex.exerciseId}
                            id={ex.exerciseId}
                            exercise={ex}
                            exerciseInfo={exercises[ex.exerciseId]}
                            onSetsChange={handleSetsChange}
                            onDelete={handleDeleteExercise}
                            onGpxChange={handleGpxChange}
                            autoFocus={ex.exerciseId === lastAddedId}
                        />
                    ))}
                </SortableContext>
                <DragOverlay>
                    {activeExercise && (
                        <LogExerciseCard
                            id={activeExercise.exerciseId}
                            exercise={activeExercise}
                            exerciseInfo={exercises[activeExercise.exerciseId]}
                            onSetsChange={() => {}}
                            onDelete={() => {}}
                        />
                    )}
                </DragOverlay>
            </DndContext>

            <Button text="Add Exercise" style="grey" icon={<Plus size={15} />} onClick={() => setAddExerciseOpen(true)} />
        </div>
    );
}

export default LogDetail;