import type { WorkoutTemplate } from "../api.ts";

interface TemplateSidebarCardProps {
    template: WorkoutTemplate;
    isActive: boolean;
    onSelect: () => void;
    onDelete: () => void;
}

function TemplateSidebarCard({ template, isActive, onSelect, onDelete }: TemplateSidebarCardProps) {
    const exerciseCount = template.exercises.length;
    const setCount = template.exercises.reduce((sum, ex) => sum + ex.sets.length, 0);
    const cardioSets  = template.exercises.reduce((sum, ex) => sum + ex.sets.filter(s => s.type === "Cardio").length, 0);
    const plyoSets    = template.exercises.reduce((sum, ex) => sum + ex.sets.filter(s => s.type === "Plyometric").length, 0);
    const strengthSets = setCount - cardioSets - plyoSets;
    const colorClass = cardioSets >= strengthSets && cardioSets >= plyoSets
        ? "cardio"
        : plyoSets > strengthSets
            ? "plyometric"
            : "strength";

    return (
        <div className={`template-card ${colorClass}${isActive ? " active" : ""}`} onClick={onSelect}>
            <p className="template-card-name">{template.name}</p>
            <p className="template-card-meta">
                {exerciseCount} {exerciseCount === 1 ? "exercise" : "exercises"} · {setCount} sets
            </p>
            <button
                className="template-card-delete"
                title="Delete template"
                onClick={e => { e.stopPropagation(); onDelete(); }}
            >
                ×
            </button>
        </div>
    );
}

export default TemplateSidebarCard;
