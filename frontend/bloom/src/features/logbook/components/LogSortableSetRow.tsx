import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import type { LoggedSet } from "../api.ts";
import DurationInput from "../../templates/components/DurationInput.tsx";
import SetMarkerButton from "./SetMarkerButton.tsx";
import { markerShort } from "../logbookUtils.ts";
import { GripVertical, X } from "lucide-react";
import { moveBetweenCells } from "../../../utils/cellNavigation.ts";

export interface LogRowItem {
    id: string;
    set: LoggedSet;
}

interface LogSortableSetRowProps {
    item: LogRowItem;
    /** Working-set number (warm-ups are not counted). */
    number: number;
    type: string | undefined;
    onSetChange: (id: string, set: LoggedSet) => void;
    onDelete: (id: string) => void;
}

function LogSortableSetRow({ item, number, type, onSetChange, onDelete }: LogSortableSetRowProps) {
    const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({ id: item.id });

    const isCardio = type === "Cardio";
    const short = markerShort(item.set.marker);

    return (
        <div
            ref={setNodeRef}
            className={`set-row${short ? ` has-marker marker-${short.toLowerCase()}` : ""}`}
            style={{ transform: CSS.Transform.toString(transform), transition, opacity: isDragging ? 0.4 : 1 }}
            onKeyDown={moveBetweenCells}
        >
            <SetMarkerButton
                marker={item.set.marker}
                number={number}
                onChange={marker => onSetChange(item.id, { ...item.set, marker })}
            />
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
                    <div className="weight-cell">
                        <input
                            type="number"
                            min={0}
                            value={item.set.weight ?? ""}
                            placeholder="0"
                            onChange={e => onSetChange(item.id, { ...item.set, weight: e.target.value === "" ? null : Number(e.target.value) })}
                        />
                        <span className="unit-label">kg</span>
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
            <span className="set-actions">
                <button className="set-delete" tabIndex={-1} onClick={() => onDelete(item.id)}><X size={12} /></button>
                <span className="set-drag-handle" {...attributes} {...listeners} tabIndex={-1}><GripVertical size={14} /></span>
            </span>
        </div>
    );
}

export default LogSortableSetRow;