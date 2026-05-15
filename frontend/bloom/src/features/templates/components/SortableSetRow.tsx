import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import type { PlannedSet } from "../../../assets/js/data/apiTypes.ts";
import DurationInput from "./DurationInput.tsx";

export interface RowItem {
    id: string;
    set: PlannedSet;
}

function SortableSetRow({ item, index, type, onSetChange }: {
    item: RowItem;
    index: number;
    type: string | undefined;
    onSetChange: (id: string, set: PlannedSet) => void;
}) {
    const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({ id: item.id });

    return (
        <div
            ref={setNodeRef}
            className="set-row"
            style={{
                transform: CSS.Transform.toString(transform),
                transition,
                opacity: isDragging ? 0.4 : 1,
                position: isDragging ? "relative" : undefined,
                zIndex: isDragging ? 1 : undefined,
            }}
        >
            <p>{index + 1}</p>
            {type === "Cardio" ? (
                <>
                    <div className="distance-input">
                        <input
                            type="number"
                            min={0}
                            value={item.set.distance != null ? Math.round(item.set.distance * 1000) : ""}
                            placeholder="0"
                            onChange={e => {
                                const meters = e.target.value === "" ? null : Math.max(0, Number(e.target.value));
                                onSetChange(item.id, { ...item.set, distance: meters == null ? null : meters / 1000 });
                            }}
                        />
                        <span className="unit-label">m</span>
                    </div>
                    <DurationInput
                        value={item.set.duration ?? null}
                        onChange={v => onSetChange(item.id, { ...item.set, duration: v })}
                    />
                </>
            ) : (
                <input
                    type="number"
                    min={0}
                    value={item.set.reps ?? ""}
                    onChange={e => onSetChange(item.id, {
                        ...item.set,
                        reps: e.target.value === "" ? null : Number(e.target.value),
                    })}
                />
            )}
            <span className="set-drag-handle" {...attributes} {...listeners} tabIndex={-1}>⠿</span>
        </div>
    );
}

export default SortableSetRow;