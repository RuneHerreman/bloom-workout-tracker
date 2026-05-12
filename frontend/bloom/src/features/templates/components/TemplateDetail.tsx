import type { WorkoutTemplate } from "../api.ts";
import type { Exercise } from "../../exercises/api.ts";
import TemplateExerciseCard from "./TemplateExerciseCard.tsx";

interface TemplateDetailProps {
    template: WorkoutTemplate;
    exercises: Record<string, Exercise>;
}

function TemplateDetail({ template, exercises }: TemplateDetailProps) {
    return (
        <div className="template-detail-view">
            {template.exercises.sort((a, b) => a.order - b.order).map((exercise, index) => (
                <TemplateExerciseCard key={index} exercise={exercise} exerciseInfo={exercises[exercise.exerciseId]} />
            ))}
        </div>
    );
}

export default TemplateDetail;