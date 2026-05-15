import type { WorkoutTemplate } from "../api.ts";
import type { Exercise } from "../../exercises/api.ts";
import TemplateExerciseCard from "./TemplateExerciseCard.tsx";
import Button from "../../../components/general/ButtonComponent.tsx";

interface TemplateDetailProps {
    template: WorkoutTemplate;
    exercises: Record<string, Exercise>;
    onDelete: (id: string) => void;
}

function TemplateDetail({ template, exercises, onDelete }: TemplateDetailProps) {
    const sorted = [...template.exercises].sort((a, b) => a.order - b.order);

    return (
        <div className="template-detail-view">
            <div className="template-detail-header">
                <h3>{template.name}</h3>
                <div className="actions-row">
                    <Button text="Edit Template" style="green" />
                    <Button text="Delete Template" style="red" onClick={() => onDelete(template.id)} />
                </div>
            </div>

            {sorted.map((exercise) => (
                <TemplateExerciseCard
                    key={`${template.id}-${exercise.exerciseId}`}
                    exercise={exercise}
                    exerciseInfo={exercises[exercise.exerciseId]}
                />
            ))}
        </div>
    );
}

export default TemplateDetail;