import { useState, useEffect } from "react";
import type { WorkoutTemplate } from "../api.ts";
import type { TemplateExercise, PlannedSet } from "../../../assets/js/data/apiTypes.ts";
import type { Exercise } from "../../exercises/api.ts";
import TemplateExerciseCard from "./TemplateExerciseCard.tsx";
import Button from "../../../components/general/ButtonComponent.tsx";

interface TemplateDetailProps {
    template: WorkoutTemplate;
    exercises: Record<string, Exercise>;
}

function TemplateDetail({ template, exercises }: TemplateDetailProps) {
    const [templateExercises, setTemplateExercises] = useState<TemplateExercise[]>(() =>
        [...template.exercises].sort((a, b) => a.order - b.order)
    );

    useEffect(() => {
        setTemplateExercises([...template.exercises].sort((a, b) => a.order - b.order));
    }, [template.id]);

    function handleSetsChange(exerciseId: string, sets: PlannedSet[]) {
        setTemplateExercises(prev =>
            prev.map(ex => ex.exerciseId === exerciseId ? { ...ex, sets } : ex)
        );
    }

    return (
        <div className="template-detail-view">
            <div className="template-detail-header">
                <h3>{template.name}</h3>
                <div className="actions-row">
                    <Button text="Edit Template" style="green" />
                    <Button text="Delete Template" style="red" />
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
