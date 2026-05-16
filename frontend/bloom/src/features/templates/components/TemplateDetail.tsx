import { useState, useEffect, useRef } from "react";
import { DndContext, closestCenter, DragOverlay } from "@dnd-kit/core";
import { SortableContext, verticalListSortingStrategy } from "@dnd-kit/sortable";
import type { WorkoutTemplate } from "../api.ts";
import { updateTemplate } from "../api.ts";
import type { TemplateExercise, PlannedSet } from "../../../assets/js/data/apiTypes.ts";
import type { Exercise } from "../../exercises/api.ts";
import { useExerciseDnd } from "../hooks/useExerciseDnd.ts";
import TemplateExerciseCard from "./TemplateExerciseCard.tsx";
import Button from "../../../components/general/ButtonComponent.tsx";
import { Save, Trash2 } from "lucide-react";

interface TemplateDetailProps {
    template: WorkoutTemplate;
    exercises: Record<string, Exercise>;
    onDelete: (id: string) => void;
    onSave: (id: string, name: string, exercises: TemplateExercise[]) => void;
    pendingExerciseId?: string | null;
    onExerciseAdded?: () => void;
    onDirtyChange?: (isDirty: boolean, save: () => Promise<void>) => void;
}

function TemplateDetail({ template, exercises, onDelete, onSave, pendingExerciseId, onExerciseAdded, onDirtyChange }: TemplateDetailProps) {
    const [templateExercises, setTemplateExercises] = useState<TemplateExercise[]>(() =>
        [...template.exercises].sort((a, b) => a.order - b.order)
    );
    const [name, setName] = useState(template.name);
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [lastAddedId, setLastAddedId] = useState<string | null>(null);

    const { activeExercise, onDragStart, onDragEnd, onDragCancel } = useExerciseDnd(
        templateExercises,
        reordered => setTemplateExercises(reordered)
    );

    useEffect(() => {
        if (!pendingExerciseId) return;
        const type = exercises[pendingExerciseId]?.type ?? "Strength";
        const defaultSet: PlannedSet = type === "Cardio"
            ? { type: "Cardio", order: 1, reps: null, duration: "00:30:00", distance: 5, distanceUnit: "km" }
            : { type,           order: 1, reps: 10,   duration: null,       distance: null, distanceUnit: null };
        setTemplateExercises(prev => [
            ...prev,
            { exerciseId: pendingExerciseId, order: prev.length + 1, sets: [defaultSet] },
        ]);
        setLastAddedId(pendingExerciseId);
        onExerciseAdded?.();
    }, [pendingExerciseId]);

    function handleDeleteExercise(exerciseId: string) {
        setTemplateExercises(prev =>
            prev.filter(ex => ex.exerciseId !== exerciseId).map((ex, i) => ({ ...ex, order: i + 1 }))
        );
    }

    function handleSetsChange(exerciseId: string, sets: PlannedSet[]) {
        setTemplateExercises(prev =>
            prev.map(ex => ex.exerciseId === exerciseId ? { ...ex, sets } : ex)
        );
    }

    const normalize = (exs: TemplateExercise[]) =>
        [...exs].sort((a, b) => a.order - b.order).map(ex => ({
            ...ex,
            sets: [...ex.sets].sort((a, b) => a.order - b.order),
        }));

    const hasChanges = name !== template.name ||
        JSON.stringify(normalize(templateExercises)) !== JSON.stringify(normalize(template.exercises));

    const handleSaveRef = useRef(handleSave);
    handleSaveRef.current = handleSave;
    useEffect(() => {
        onDirtyChange?.(hasChanges, () => handleSaveRef.current());
    }, [hasChanges]); // eslint-disable-line react-hooks/exhaustive-deps

    async function handleSave() {
        setSaving(true);
        setError(null);
        try {
            await updateTemplate(template.id, name, templateExercises);
            onSave(template.id, name, templateExercises);
        } catch (e) {
            setError(e instanceof Error ? e.message : "Failed to save template");
        } finally {
            setSaving(false);
        }
    }

    return (
        <div className="template-detail-view">
            {error && <div className="error-banner">{error}</div>}
            <div className="template-detail-header">
                <input
                    className="template-title-input"
                    value={name}
                    onChange={e => setName(e.target.value)}
                />
                <div className="actions-row">
                    <Button text="Save Changes" style="green" icon={<Save size={14} />} disabled={!hasChanges || saving} onClick={handleSave} />
                    <Button text="Delete Template" style="red" icon={<Trash2 size={14} />} onClick={() => onDelete(template.id)} />
                </div>
            </div>

            <DndContext collisionDetection={closestCenter} onDragStart={onDragStart} onDragEnd={onDragEnd} onDragCancel={onDragCancel}>
                <SortableContext items={templateExercises.map(ex => ex.exerciseId)} strategy={verticalListSortingStrategy}>
                    {templateExercises.map((exercise) => (
                        <TemplateExerciseCard
                            key={`${template.id}-${exercise.exerciseId}`}
                            id={exercise.exerciseId}
                            exercise={exercise}
                            exerciseInfo={exercises[exercise.exerciseId] ?? null}
                            onSetsChange={handleSetsChange}
                            onDelete={handleDeleteExercise}
                            autoFocus={exercise.exerciseId === lastAddedId}
                        />
                    ))}
                </SortableContext>
                <DragOverlay>
                    {activeExercise && (
                        <TemplateExerciseCard
                            id={activeExercise.exerciseId}
                            exercise={activeExercise}
                            exerciseInfo={exercises[activeExercise.exerciseId] ?? null}
                        />
                    )}
                </DragOverlay>
            </DndContext>
        </div>
    );
}

export default TemplateDetail;
