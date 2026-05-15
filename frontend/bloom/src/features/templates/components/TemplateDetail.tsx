import { useState } from "react";
import type { WorkoutTemplate } from "../api.ts";
import type { TemplateExercise, PlannedSet } from "../../../assets/js/data/apiTypes.ts";
import type { Exercise } from "../../exercises/api.ts";
import TemplateExerciseCard from "./TemplateExerciseCard.tsx";
import Button from "../../../components/general/ButtonComponent.tsx";

interface TemplateDetailProps {
    template: WorkoutTemplate;
    exercises: Record<string, Exercise>;
    onDelete: (id: string) => void;
}

function TemplateDetail({ template, exercises, onDelete }: TemplateDetailProps) {
    const [templateExercises, setTemplateExercises] = useState<TemplateExercise[]>(() =>
        [...template.exercises].sort((a, b) => a.order - b.order)
    );

    function handleSetsChange(exerciseId: string, sets: PlannedSet[]) {
        setTemplateExercises(prev =>
            prev.map(ex => ex.exerciseId === exerciseId ? { ...ex, sets } : ex)
        );
    }

    const normalize = (exercises: TemplateExercise[]) =>
        [...exercises].sort((a, b) => a.order - b.order).map(ex => ({
            ...ex,
            sets: [...ex.sets].sort((a, b) => a.order - b.order),
        }));

    const hasChanges = JSON.stringify(normalize(templateExercises)) !== JSON.stringify(normalize(template.exercises));

    return (
        <div className="template-detail-view">
            <div className="template-detail-header">
                <h3>{template.name}</h3>
                <div className="actions-row">
                    <Button text="Save Changes" style="green" disabled={!hasChanges} />
                    <Button text="Delete Template" style="red" onClick={() => onDelete(template.id)} />
                </div>
            </div>

            {templateExercises.map((exercise) => (
                <TemplateExerciseCard
                    key={`${template.id}-${exercise.exerciseId}`}
                    exercise={exercise}
                    exerciseInfo={exercises[exercise.exerciseId]}
                    onSetsChange={handleSetsChange}
                />
            ))}
        </div>
    );
}

export default TemplateDetail;