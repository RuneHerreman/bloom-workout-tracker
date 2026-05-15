import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import type { LoggedSet } from "../api.ts";
import DurationInput from "../../templates/components/DurationInput.tsx";
import { GripVertical, X } from "lucide-react";

export interface LogRowItem {
    id: string;
    set: LoggedSet;
}

interface LogSortableSetRowProps {
    item: LogRowItem;
    index: number;
    type: string | undefined;
    onSetChange: (id: string, set: LoggedSet) => void;
    onDelete: (id: string) => void;
}

function LogSortableSetRow({ item, index, type, onSetChange, onDelete }: LogSortableSetRowProps) {
    const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({ id: item.id });

    const isCardio = type === "Cardio";

    return (
        <div
            ref={setNodeRef}
            className="log-set-row"
            style={{ transform: CSS.Transform.toString(transform), transition, opacity: isDragging ? 0.4 : 1 }}
        >
            <p>{index + 1}</p>
            {isCardio ? (
                <>
                    <div className="distance-input">
                        <input
                            type="number"
                            min={1}
                            value={item.set.distance != null ? Math.round(item.set.distance * 1000) : ""}
                            placeholder="0"
                            onChange={e => {
                                const meters = e.target.value === "" ? null : Number(e.target.value);
                                onSetChange(item.id, { ...item.set, distance: meters == null || meters <= 0 ? null : meters / 1000, distanceUnit: "km" });
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
                <>
                    <input
                        type="number"
                        min={0}
                        value={item.set.reps ?? ""}
                        placeholder="0"
                        onChange={e => onSetChange(item.id, { ...item.set, reps: e.target.value === "" ? null : Number(e.target.value) })}
                    />
                    <div className="log-weight-cell">
                        <input
                            type="number"
                            min={0}
                            value={item.set.weight ?? ""}
                            placeholder="0"
                            onChange={e => onSetChange(item.id, { ...item.set, weight: e.target.value === "" ? null : Number(e.target.value) })}
                        />
                        <select
                            className="log-unit-select"
                            value={item.set.weightUnit ?? "kg"}
                            onChange={e => onSetChange(item.id, { ...item.set, weightUnit: e.target.value as "kg" | "lbs" })}
                        >
                            <option value="kg">kg</option>
                            <option value="lbs">lbs</option>
                        </select>
                    </div>
                    <input
                        type="number"
                        min={0}
                        max={10}
                        value={item.set.rir ?? ""}
                        placeholder="—"
                        onChange={e => onSetChange(item.id, { ...item.set, rir: e.target.value === "" ? null : Number(e.target.value) })}
                    />
                </>
            )}
            <span className="log-set-actions">
                <span className="set-drag-handle" {...attributes} {...listeners} tabIndex={-1}><GripVertical size={14} /></span>
                <button className="log-set-delete" tabIndex={-1} onClick={() => onDelete(item.id)}><X size={12} /></button>
            </span>
        </div>
    );
}

export default LogSortableSetRow;