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
    /** Exercise name, for screen-reader labels. */
    exerciseName: string;
    onSetChange: (id: string, set: LoggedSet) => void;
    onDelete: (id: string) => void;
}

const toNumber = (raw: string) => (raw === "" ? null : Number(raw));

function LogSortableSetRow({ item, number, type, exerciseName, onSetChange, onDelete }: LogSortableSetRowProps) {
    const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({ id: item.id });

    const isCardio = type === "Cardio";
    const short = markerShort(item.set.marker);
    const hint = item.set.hint;
    const setLabel = number ? `set ${number}` : "warm-up set";
    const label = (field: string) => `${exerciseName}, ${setLabel}, ${field}`;

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
                    <label className="distance-input">
                        <input
                            type="number"
                            inputMode="numeric"
                            enterKeyHint="next"
                            min={1}
                            aria-label={label("distance in metres")}
                            value={item.set.distance != null ? Math.round(item.set.distance * 1000) : ""}
                            placeholder={hint?.distance != null ? String(Math.round(hint.distance * 1000)) : "0"}
                            onFocus={e => e.target.select()}
                            onChange={e => {
                                const meters = toNumber(e.target.value);
                                onSetChange(item.id, { ...item.set, distance: meters == null || meters <= 0 ? null : meters / 1000, distanceUnit: "km" });
                            }}
                        />
                        <span className="unit-label" aria-hidden>m</span>
                    </label>
                    <DurationInput
                        value={item.set.duration ?? null}
                        placeholder={hint?.duration ?? null}
                        label={label("duration")}
                        onChange={v => onSetChange(item.id, { ...item.set, duration: v })}
                    />
                </>
            ) : (
                <>
                    <input
                        type="number"
                        inputMode="numeric"
                        enterKeyHint="next"
                        min={0}
                        aria-label={label("reps")}
                        value={item.set.reps ?? ""}
                        placeholder={hint?.reps != null ? String(hint.reps) : "—"}
                        onFocus={e => e.target.select()}
                        onChange={e => onSetChange(item.id, { ...item.set, reps: toNumber(e.target.value) })}
                    />
                    <label className="weight-cell">
                        <input
                            type="number"
                            inputMode="decimal"
                            enterKeyHint="next"
                            step="any"
                            min={0}
                            aria-label={label("weight in kg")}
                            value={item.set.weight ?? ""}
                            placeholder={hint?.weight != null ? String(hint.weight) : "0"}                            onFocus={e => e.target.select()}
                            onChange={e => onSetChange(item.id, { ...item.set, weight: toNumber(e.target.value) })}
                        />
                        <span className="unit-label" aria-hidden>kg</span>
                    </label>
                    <input
                        type="number"
                        inputMode="numeric"
                        enterKeyHint="next"
                        min={0}
                        max={10}
                        aria-label={label("reps in reserve")}
                        value={item.set.rir ?? ""}
                        placeholder="—"
                        onFocus={e => e.target.select()}
                        onChange={e => onSetChange(item.id, { ...item.set, rir: toNumber(e.target.value) })}
                    />
                </>
            )}
            <span className="set-actions">
                <button
                    type="button"
                    className="set-delete"
                    tabIndex={-1}
                    aria-label={`Remove ${setLabel}`}
                    onClick={() => onDelete(item.id)}
                >
                    <X size={12} />
                </button>
                <span
                    className="set-drag-handle"
                    {...attributes}
                    {...listeners}
                    tabIndex={-1}
                    aria-label={`Reorder ${setLabel}`}
                >
                    <GripVertical size={14} />
                </span>
            </span>
        </div>
    );
}

export default LogSortableSetRow;