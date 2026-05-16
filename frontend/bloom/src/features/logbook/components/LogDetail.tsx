import { useState } from "react";
import { DndContext, closestCenter, DragOverlay } from "@dnd-kit/core";
import { SortableContext, verticalListSortingStrategy } from "@dnd-kit/sortable";
import type { LoggedWorkout, LoggedExercise, LoggedSet } from "../api.ts";
import { updateLog } from "../api.ts";
import type { Exercise } from "../../exercises/api.ts";
import { toDateInputValue } from "../logbookUtils.ts";
import { useExerciseDnd } from "../../templates/hooks/useExerciseDnd.ts";
import LogExerciseCard from "./LogExerciseCard.tsx";
import Button from "../../../components/general/ButtonComponent.tsx";
import Overlay from "../../../components/general/OverlayComponent.tsx";
import ExerciseLibrary from "../../templates/components/ExerciseLibrary.tsx";
import { Save, Trash2, Plus } from "lucide-react";

interface LogDetailProps {
    log: LoggedWorkout;
    exercises: Record<string, Exercise>;
    onSave: (id: string, name: string, loggedAt: string, note: string | null, exercises: LoggedExercise[]) => void;
    onDelete: (id: string) => void;
}

function LogDetail({ log, exercises, onSave, onDelete }: LogDetailProps) {
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
        <div className="log-detail-view">
            {addExerciseOpen && (
                <Overlay title="Exercise library" subtitle="Add exercise" onClose={() => setAddExerciseOpen(false)}>
                    <ExerciseLibrary
                        exercises={Object.values(exercises)}
                        onSelect={e => handleAddExercise(e.id)}
                    />
                </Overlay>
            )}

            {error && <div className="error-banner">{error}</div>}

            <div className="log-detail-header">
                <div className="log-detail-title-group">
                    <input
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
                <div className="actions-row">
                    <Button text="Save Changes" style="green" icon={<Save size={14} />} disabled={!hasChanges || saving} onClick={handleSave} />
                    <Button text="Delete Log" style="red" icon={<Trash2 size={14} />} onClick={() => onDelete(log.id)} />
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