import type { WorkoutTemplate } from "../api.ts";

interface TemplateSidebarCardProps {
    template: WorkoutTemplate;
    colorClass: "strength" | "cardio" | "plyometric";
    isActive: boolean;
    onSelect: () => void;
}

const TYPE_LABELS = { strength: "Strength", cardio: "Cardio", plyometric: "Plyo" } as const;

function TemplateSidebarCard({ template, colorClass, isActive, onSelect }: TemplateSidebarCardProps) {
    const exerciseCount = template.exercises.length;
    const setCount = template.exercises.reduce((n, ex) => n + ex.sets.length, 0);

    return (
        <div className={`template-card ${colorClass}${isActive ? " active" : ""}`} onClick={onSelect}>
            <p className="template-card-name">{template.name}</p>
            <div className="template-card-bottom">
                <span className={`template-card-type-badge ${colorClass}`}>{TYPE_LABELS[colorClass]}</span>
                <span className="template-card-meta">
                    {exerciseCount} ex · {setCount} sets
                </span>
            </div>
        </div>
    );
}

export default TemplateSidebarCard;
