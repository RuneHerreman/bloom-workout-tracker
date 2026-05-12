import type { WorkoutTemplate } from "../api.ts";

interface TemplateSidebarCardProps {
    template: WorkoutTemplate;
    isActive: boolean;
    onSelect: () => void;
    onDelete: () => void;
}

const TYPE_LABELS = { strength: "Strength", cardio: "Cardio", plyometric: "Plyo" } as const;

function TemplateSidebarCard({ template, isActive, onSelect, onDelete }: TemplateSidebarCardProps) {
    const exerciseCount = template.exercises.length;
    const sets = template.exercises.flatMap(ex => ex.sets);
    const setCount = sets.length;
    const cardioSets   = sets.filter(s => s.type === "Cardio").length;
    const plyoSets     = sets.filter(s => s.type === "Plyometric").length;
    const strengthSets = setCount - cardioSets - plyoSets;

    const colorClass = cardioSets >= strengthSets && cardioSets >= plyoSets
        ? "cardio"
        : plyoSets > strengthSets
            ? "plyometric"
            : "strength";

    return (
        <div className={`template-card ${colorClass}${isActive ? " active" : ""}`} onClick={onSelect}>
            <div className="template-card-top">
                <p className="template-card-name">{template.name}</p>
                <button
                    className="template-card-delete"
                    title="Delete template"
                    onClick={e => { e.stopPropagation(); onDelete(); }}
                >
                    ×
                </button>
            </div>
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
