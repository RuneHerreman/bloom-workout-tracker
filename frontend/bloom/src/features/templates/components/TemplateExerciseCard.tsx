import { useState } from "react";
import { DndContext, closestCenter, type DragEndEvent } from "@dnd-kit/core";
import { SortableContext, verticalListSortingStrategy, arrayMove, useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import type { TemplateExercise, PlannedSet } from "../../../assets/js/data/apiTypes.ts";
import type { Exercise } from "../../exercises/api.ts";
import Button from "../../../components/general/ButtonComponent.tsx";
import SortableSetRow, { type RowItem } from "./SortableSetRow.tsx";
import { PlusIcon, GripVertical, X } from "lucide-react";

interface TemplateExerciseCardProps {
    id: string;
    exercise: TemplateExercise;
    exerciseInfo?: Exercise;
    onSetsChange?: (exerciseId: string, sets: PlannedSet[]) => void;
    onDelete?: (exerciseId: string) => void;
}

function TemplateExerciseCard({ id, exercise, exerciseInfo, onSetsChange, onDelete }: TemplateExerciseCardProps) {
    const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({ id });
    const [items, setItems] = useState<RowItem[]>(() =>
        [...exercise.sets]
            .sort((a, b) => a.order - b.order)
            .map(set => ({ id: crypto.randomUUID(), set }))
    );

    function pushChange(updated: RowItem[]) {
        onSetsChange?.(exercise.exerciseId, updated.map((item, i) => ({ ...item.set, order: i + 1 })));
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

    function handleSetChange(itemId: string, updatedSet: PlannedSet) {
        const updated = items.map(item => item.id === itemId ? { ...item, set: updatedSet } : item);
        setItems(updated);
        pushChange(updated);
    }

    const exerciseType = exerciseInfo?.type ?? "Strength";
    const bodyClass = `detail-body ${exerciseType === "Cardio" ? "is-cardio" : "is-strength"}`;

    function handleAddSet() {
        const newSet: PlannedSet = exerciseType === "Cardio"
            ? { type: "Cardio", order: items.length + 1, reps: null, duration: "00:30:00", distance: 5, distanceUnit: "km" }
            : { type: exerciseType, order: items.length + 1, reps: 10, duration: null, distance: null, distanceUnit: null };
        const updated = [...items, { id: crypto.randomUUID(), set: newSet }];
        setItems(updated);
        pushChange(updated);
    }

    return (
        <div
            ref={setNodeRef}
            className="template-exercise-card"
            style={{ transform: CSS.Transform.toString(transform), transition, opacity: isDragging ? 0.4 : 1 }}
        >
            <header>
                <div>
                    <h3 className="detail-exercise-name">{exerciseInfo?.name}</h3>
                    <p className="detail-exercise-info">{exerciseInfo?.type} · {exerciseInfo?.targetMuscles.join(" - ")}</p>
                </div>
                <div className="exercise-card-actions">
                    <span className="exercise-drag-handle" {...attributes} {...listeners} tabIndex={-1}><GripVertical size={16} /></span>
                    <button className="exercise-delete-btn" tabIndex={-1} onClick={() => onDelete?.(exercise.exerciseId)}><X size={14} /></button>
                </div>
            </header>
            <section className={bodyClass}>
                <div className="set-grid-header">
                    <p>Set</p>
                    {exerciseType === "Cardio" ? (
                        <><p>Distance</p><p>Duration</p></>
                    ) : (
                        <p>Reps</p>
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
                                type={exerciseType}
                                onSetChange={handleSetChange}
                            />
                        ))}
                    </SortableContext>
                </DndContext>
            </section>
            <section className="detail-footer">
                <Button text="Add set" style="modern" icon={<PlusIcon size={15} />} onClick={handleAddSet} />
            </section>
        </div>
    );
}

export default TemplateExerciseCard;