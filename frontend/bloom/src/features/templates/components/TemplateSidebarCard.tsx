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
    const cardioSets    = sets.filter(s => s.type === "Cardio").length;
    const plyoSets      = sets.filter(s => s.type === "Plyometric").length;
    const strengthSets  = setCount - cardioSets - plyoSets;

    const colorClass = cardioSets >= strengthSets && cardioSets >= plyoSets
        ? "cardio"
        : plyoSets > strengthSets
            ? "plyometric"
            : "strength";

    const strengthPct  = setCount > 0 ? (strengthSets / setCount) * 100 : 0;
    const cardioPct    = setCount > 0 ? (cardioSets   / setCount) * 100 : 0;
    const plyoPct      = setCount > 0 ? (plyoSets     / setCount) * 100 : 0;

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
            {setCount > 0 && (
                <div className="template-card-bar">
                    {strengthSets > 0 && <div className="bar-segment strength" style={{ width: `${strengthPct}%` }} />}
                    {cardioSets   > 0 && <div className="bar-segment cardio"   style={{ width: `${cardioPct}%` }} />}
                    {plyoSets     > 0 && <div className="bar-segment plyometric" style={{ width: `${plyoPct}%` }} />}
                </div>
            )}
        </div>
    );
}

export default TemplateSidebarCard;
