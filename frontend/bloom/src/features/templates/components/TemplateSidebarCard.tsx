import type { WorkoutTemplate } from "../api.ts";

interface TemplateSidebarCardProps {
    template: WorkoutTemplate;
    colorClass: "strength" | "cardio" | "plyometric" | "mix";
    isActive: boolean;
    onSelect: () => void;
}

const TYPE_LABELS = { strength: "Strength", cardio: "Cardio", plyometric: "Plyo", mix: "Mix" } as const;

function TemplateSidebarCard({ template, colorClass, isActive, onSelect }: TemplateSidebarCardProps) {
    const exerciseCount = template.exercises.length;
    const setCount = template.exercises.reduce((n, ex) => n + ex.sets.length, 0);

    return (
        <div className={`sidebar-card ${colorClass}${isActive ? " active" : ""}`} onClick={onSelect}>
            <span className={`type-dot ${colorClass}`} data-label={TYPE_LABELS[colorClass]} />
            <p className="sidebar-card-name">{template.name}</p>
            <span className="sidebar-card-meta">
                {exerciseCount} ex · {setCount} set{setCount !== 1 ? "s" : ""}
            </span>
        </div>
    );
}

export default TemplateSidebarCard;
