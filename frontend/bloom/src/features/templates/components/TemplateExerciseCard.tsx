import { useState } from "react";
import { DndContext, closestCenter, type DragEndEvent } from "@dnd-kit/core";
import { SortableContext, verticalListSortingStrategy, useSortable, arrayMove } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import type { TemplateExercise, PlannedSet } from "../../../assets/js/data/apiTypes.ts";
import type { Exercise } from "../../exercises/api.ts";
import Button from "../../../components/general/ButtonComponent.tsx";

interface TemplateExerciseCardProps {
    exercise: TemplateExercise;
    exerciseInfo?: Exercise;
    onSetsChange?: (exerciseId: string, sets: PlannedSet[]) => void;
}

interface RowItem {
    id: string;
    set: PlannedSet;
}

function parseDuration(raw: string | null): [number, number, number] {
    if (!raw) return [0, 0, 0];
    const parts = raw.split(":").map(Number);
    if (parts.length === 3) return [parts[0] ?? 0, parts[1] ?? 0, parts[2] ?? 0];
    if (parts.length === 2) return [0, parts[0] ?? 0, parts[1] ?? 0];
    return [0, 0, parts[0] ?? 0];
}

function formatDuration(h: number, m: number, s: number): string {
    const pad = (n: number) => String(n).padStart(2, "0");
    return `${pad(h)}:${pad(m)}:${pad(s)}`;
}

function DurationInput({ value, onChange }: { value: string | null; onChange: (v: string | null) => void }) {
    const [h, m, s] = parseDuration(value);

    function handleChange(part: "h" | "m" | "s", raw: string) {
        const n = raw === "" ? 0 : Math.max(0, Number(raw));
        const clamped = part === "h" ? Math.min(n, 99) : Math.min(n, 59);
        const next = part === "h" ? formatDuration(clamped, m, s)
                   : part === "m" ? formatDuration(h, clamped, s)
                   :                formatDuration(h, m, clamped);
        onChange(next === "00:00:00" ? null : next);
    }

    return (
        <div className="duration-input">
            <input type="number" min={0} max={99} value={h === 0 ? "" : h} placeholder="0" onChange={e => handleChange("h", e.target.value)} />
            <span className="duration-sep">:</span>
            <input type="number" min={0} max={59} value={m === 0 ? "" : m} placeholder="00" onChange={e => handleChange("m", e.target.value)} />
            <span className="duration-sep">:</span>
            <input type="number" min={0} max={59} value={s === 0 ? "" : s} placeholder="00" onChange={e => handleChange("s", e.target.value)} />
        </div>
    );
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
            {type === "Strength" ? (
                <input
                    type="number"
                    min={0}
                    value={item.set.reps ?? ""}
                    onChange={e => onSetChange(item.id, {
                        ...item.set,
                        reps: e.target.value === "" ? null : Number(e.target.value),
                    })}
                />
            ) : (
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
            )}
            <span className="set-drag-handle" {...attributes} {...listeners}>⠿</span>
        </div>
    );
}

function TemplateExerciseCard({ exercise, exerciseInfo, onSetsChange }: TemplateExerciseCardProps) {
    const [items, setItems] = useState<RowItem[]>(() =>
        [...exercise.sets]
            .sort((a, b) => a.order - b.order)
            .map((set, i) => ({ id: `${exercise.exerciseId}-${i}`, set }))
    );

    function pushChange(updated: RowItem[]) {
        const updatedSets = updated.map((item, i) => ({ ...item.set, order: i + 1 }));
        onSetsChange?.(exercise.exerciseId, updatedSets);
    }

    function handleDragEnd(event: DragEndEvent) {
        const { active, over } = event;
        if (!over || active.id === over.id) return;
        const oldIndex = items.findIndex(item => item.id === active.id);
        const newIndex = items.findIndex(item => item.id === over.id);
        const reordered = arrayMove(items, oldIndex, newIndex);
        setItems(reordered);
        pushChange(reordered);
    }

    function handleSetChange(id: string, updatedSet: PlannedSet) {
        const updated = items.map(item => item.id === id ? { ...item, set: updatedSet } : item);
        setItems(updated);
        pushChange(updated);
    }

    const bodyClass = `detail-body ${exerciseInfo?.type === "Strength" ? "is-strength" : "is-cardio"}`;

    return (
        <div className="template-exercise-card">
            <header>
                <div>
                    <h3 className="detail-exercise-name">{exerciseInfo?.name}</h3>
                    <p className="detail-exercise-info">{exerciseInfo?.type} · {exerciseInfo?.targetMuscles.join(" - ")}</p>
                </div>
            </header>
            <section className={bodyClass}>
                <div className="set-grid-header">
                    <p>Set</p>
                    {exerciseInfo?.type === "Strength" ? (
                        <p>Reps</p>
                    ) : (
                        <><p>Distance</p><p>Duration</p></>
                    )}
                    <span />
                </div>
                <DndContext collisionDetection={closestCenter} onDragEnd={handleDragEnd}>
                    <SortableContext items={items.map(i => i.id)} strategy={verticalListSortingStrategy}>
                        {items.map((item, index) => (
                            <SortableSetRow
                                key={item.id}
                                item={item}
                                index={index}
                                type={exerciseInfo?.type}
                                onSetChange={handleSetChange}
                            />
                        ))}
                    </SortableContext>
                </DndContext>
            </section>
            <section className="detail-footer">
                <Button text="Add set" style="modern" icon="+" />
            </section>
        </div>
    );
}

export default TemplateExerciseCard;
