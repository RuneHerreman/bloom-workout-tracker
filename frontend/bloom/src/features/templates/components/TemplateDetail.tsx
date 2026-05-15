import { useState, forwardRef, useImperativeHandle } from "react";
import { DndContext, closestCenter, type DragEndEvent } from "@dnd-kit/core";
import { SortableContext, verticalListSortingStrategy, arrayMove } from "@dnd-kit/sortable";
import type { WorkoutTemplate } from "../api.ts";
import { updateTemplate } from "../api.ts";
import type { TemplateExercise, PlannedSet } from "../../../assets/js/data/apiTypes.ts";
import type { Exercise } from "../../exercises/api.ts";
import TemplateExerciseCard from "./TemplateExerciseCard.tsx";
import Button from "../../../components/general/ButtonComponent.tsx";
import { Save, Trash2 } from "lucide-react";

interface TemplateDetailProps {
    template: WorkoutTemplate;
    exercises: Record<string, Exercise>;
    onDelete: (id: string) => void;
    onSave: (id: string, name: string, exercises: TemplateExercise[]) => void;
}

export interface TemplateDetailHandle {
    addExercise: (exerciseId: string) => void;
}

const TemplateDetail = forwardRef<TemplateDetailHandle, TemplateDetailProps>(function TemplateDetail(
    { template, exercises, onDelete, onSave },
    ref
) {
    const [templateExercises, setTemplateExercises] = useState<TemplateExercise[]>(() =>
        [...template.exercises].sort((a, b) => a.order - b.order)
    );
    const [name, setName] = useState(template.name);

    useImperativeHandle(ref, () => ({
        addExercise(exerciseId: string) {
            const type = exercises[exerciseId]?.type ?? "Strength";
            const defaultSet: PlannedSet = type === "Cardio"
                ? { type: "Cardio", order: 1, reps: null, duration: "00:30:00", distance: 5, distanceUnit: "km" }
                : { type,           order: 1, reps: 10,   duration: null,       distance: null, distanceUnit: null };
            setTemplateExercises(prev => [
                ...prev,
                { exerciseId, order: prev.length + 1, sets: [defaultSet] },
            ]);
        },
    }));

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

    function handleExerciseDragEnd(event: DragEndEvent) {
        const { active, over } = event;
        if (!over || active.id === over.id) return;
        setTemplateExercises(prev => {
            const oldIndex = prev.findIndex(ex => ex.exerciseId === active.id);
            const newIndex = prev.findIndex(ex => ex.exerciseId === over.id);
            return arrayMove(prev, oldIndex, newIndex).map((ex, i) => ({ ...ex, order: i + 1 }));
        });
    }

    const normalize = (exercises: TemplateExercise[]) =>
        [...exercises].sort((a, b) => a.order - b.order).map(ex => ({
            ...ex,
            sets: [...ex.sets].sort((a, b) => a.order - b.order),
        }));

    const hasChanges = name !== template.name ||
        JSON.stringify(normalize(templateExercises)) !== JSON.stringify(normalize(template.exercises));

    const [saving, setSaving] = useState(false);

    async function handleSave() {
        setSaving(true);
        try {
            await updateTemplate(template.id, name, templateExercises);
            onSave(template.id, name, templateExercises);
        } finally {
            setSaving(false);
        }
    }

    return (
        <div className="template-detail-view">
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

            <DndContext collisionDetection={closestCenter} onDragEnd={handleExerciseDragEnd}>
                <SortableContext items={templateExercises.map(ex => ex.exerciseId)} strategy={verticalListSortingStrategy}>
                    {templateExercises.map((exercise) => (
                        <TemplateExerciseCard
                            key={`${template.id}-${exercise.exerciseId}`}
                            id={exercise.exerciseId}
                            exercise={exercise}
                            exerciseInfo={exercises[exercise.exerciseId]}
                            onSetsChange={handleSetsChange}
                            onDelete={handleDeleteExercise}
                        />
                    ))}
                </SortableContext>
            </DndContext>
        </div>
    );
});

export default TemplateDetail;