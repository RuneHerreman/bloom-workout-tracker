import { useState, useRef } from "react";
import { DndContext, closestCenter, type DragEndEvent } from "@dnd-kit/core";
import { SortableContext, verticalListSortingStrategy, arrayMove, useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import type { LoggedExercise, LoggedSet } from "../api.ts";
import type { Exercise } from "../../exercises/api.ts";
import Button from "../../../components/general/ButtonComponent.tsx";
import LogSortableSetRow, { type LogRowItem } from "./LogSortableSetRow.tsx";
import { parseGpx, formatDuration, type GpxStats } from "../gpxUtils.ts";
import { PlusIcon, GripVertical, X, MapPin } from "lucide-react";

interface LogExerciseCardProps {
    id: string;
    exercise: LoggedExercise;
    exerciseInfo?: Exercise;
    onSetsChange: (exerciseId: string, sets: LoggedSet[]) => void;
    onDelete: (exerciseId: string) => void;
    onGpxChange?: (exerciseId: string, gpxData: string | null) => void;
}

function LogExerciseCard({ id, exercise, exerciseInfo, onSetsChange, onDelete, onGpxChange }: LogExerciseCardProps) {
    const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({ id });
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [gpxStats, setGpxStats] = useState<GpxStats | null>(() =>
        exercise.gpxData ? parseGpx(exercise.gpxData) : null
    );

    const [items, setItems] = useState<LogRowItem[]>(() =>
        [...exercise.sets]
            .sort((a, b) => a.order - b.order)
            .map(set => ({ id: crypto.randomUUID(), set }))
    );

    function pushChange(updated: LogRowItem[]) {
        onSetsChange(exercise.exerciseId, updated.map((item, i) => ({ ...item.set, order: i + 1 })));
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

    function handleSetChange(itemId: string, updatedSet: LoggedSet) {
        const updated = items.map(item => item.id === itemId ? { ...item, set: updatedSet } : item);
        setItems(updated);
        pushChange(updated);
    }

    function handleDeleteSet(itemId: string) {
        const updated = items.filter(item => item.id !== itemId);
        setItems(updated);
        pushChange(updated);
    }

    const exerciseType = exerciseInfo?.type ?? "Strength";
    const isCardio = exerciseType === "Cardio";
    const bodyClass = `log-body ${isCardio ? "is-cardio" : "is-strength"}`;

    function handleGpxFile(e: React.ChangeEvent<HTMLInputElement>) {
        const file = e.target.files?.[0];
        if (!file) return;
        const reader = new FileReader();
        reader.onload = ev => {
            const xml = ev.target?.result as string;
            const stats = parseGpx(xml);
            setGpxStats(stats);
            onGpxChange?.(exercise.exerciseId, xml);
        };
        reader.readAsText(file);
        e.target.value = "";
    }

    function handleRemoveGpx() {
        setGpxStats(null);
        onGpxChange?.(exercise.exerciseId, null);
    }

    function handleAddSet() {
        const last = items[items.length - 1]?.set;
        const newSet: LoggedSet = last
            ? { ...last, order: items.length + 1 }
            : isCardio
                ? { type: "Cardio", order: items.length + 1, reps: null, weight: null, weightUnit: null, rir: null, duration: "00:30:00", distance: 5, distanceUnit: "km" }
                : { type: exerciseType, order: items.length + 1, reps: 10, weight: 60, weightUnit: "kg", rir: 2, duration: null, distance: null, distanceUnit: null };
        const updated = [...items, { id: crypto.randomUUID(), set: newSet }];
        setItems(updated);
        pushChange(updated);
    }

    return (
        <div
            ref={setNodeRef}
            className="exercise-card"
            style={{ transform: CSS.Transform.toString(transform), transition, opacity: isDragging ? 0.4 : 1 }}
        >
            <header>
                <div>
                    <h3 className="exercise-name">{exerciseInfo?.name ?? "Unknown exercise"}</h3>
                    <p className="exercise-info">{exerciseType} · {exerciseInfo?.targetMuscles.join(" - ")}</p>
                </div>
                <div className="exercise-card-actions">
                    <span className="exercise-drag-handle" {...attributes} {...listeners} tabIndex={-1}><GripVertical size={16} /></span>
                    <button className="exercise-delete-btn" tabIndex={-1} onClick={() => onDelete(exercise.exerciseId)}><X size={14} /></button>
                </div>
            </header>
            <section className={bodyClass}>
                <div className="set-grid-header">
                    <p>Set</p>
                    {isCardio ? (
                        <><p>Distance</p><p>Duration</p></>
                    ) : (
                        <><p>Reps</p><p>Weight</p><p>RIR</p></>
                    )}
                    <span />
                </div>
                <DndContext collisionDetection={closestCenter} onDragEnd={handleDragEnd}>
                    <SortableContext items={items.map(i => i.id)} strategy={verticalListSortingStrategy}>
                        {items.map((item, index) => (
                            <LogSortableSetRow
                                key={item.id}
                                item={item}
                                index={index}
                                type={exerciseType}
                                onSetChange={handleSetChange}
                                onDelete={handleDeleteSet}
                            />
                        ))}
                    </SortableContext>
                </DndContext>
            </section>
            <section className="exercise-footer">
                <Button text="Add set" style="modern" icon={<PlusIcon size={15} />} onClick={handleAddSet} />
                {isCardio && (
                    gpxStats ? (
                        <div className="gpx-stats">
                            <MapPin size={12} className="gpx-stats-icon" />
                            <span>{gpxStats.distanceKm.toFixed(2)} km</span>
                            {gpxStats.elevationGainM > 0 && <span>+{Math.round(gpxStats.elevationGainM)} m</span>}
                            {gpxStats.durationMs > 0 && <span>{formatDuration(gpxStats.durationMs)}</span>}
                            <button className="gpx-remove" onClick={handleRemoveGpx} aria-label="Remove GPX"><X size={11} /></button>
                        </div>
                    ) : (
                        <>
                            <input ref={fileInputRef} type="file" accept=".gpx" style={{ display: "none" }} onChange={handleGpxFile} />
                            <Button text="Attach GPX" style="modern" icon={<MapPin size={14} />} onClick={() => fileInputRef.current?.click()} />
                        </>
                    )
                )}
            </section>
        </div>
    );
}

export default LogExerciseCard;