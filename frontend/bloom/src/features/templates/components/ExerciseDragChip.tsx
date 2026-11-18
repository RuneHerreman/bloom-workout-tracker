import { GripVertical } from "lucide-react";

/** What follows the pointer while reordering exercises: just the name, not the whole card. */
function ExerciseDragChip({ name, sets }: { name: string; sets: number }) {
    return (
        <div className="exercise-drag-chip">
            <GripVertical size={14} aria-hidden />
            <span className="exercise-drag-chip-name">{name}</span>
            <span className="exercise-drag-chip-meta">{sets} set{sets !== 1 ? "s" : ""}</span>
        </div>
    );
}

export default ExerciseDragChip;
