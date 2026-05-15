import { useState } from "react";
import { DndContext, closestCenter, type DragEndEvent } from "@dnd-kit/core";
import { SortableContext, verticalListSortingStrategy, arrayMove } from "@dnd-kit/sortable";
import type { TemplateExercise, PlannedSet } from "../../../assets/js/data/apiTypes.ts";
import type { Exercise } from "../../exercises/api.ts";
import Button from "../../../components/general/ButtonComponent.tsx";
import SortableSetRow, { type RowItem } from "./SortableSetRow.tsx";

interface TemplateExerciseCardProps {
    exercise: TemplateExercise;
    exerciseInfo?: Exercise;
    onSetsChange?: (exerciseId: string, sets: PlannedSet[]) => void;
}

function TemplateExerciseCard({ exercise, exerciseInfo, onSetsChange }: TemplateExerciseCardProps) {
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

    function handleSetChange(id: string, updatedSet: PlannedSet) {
        const updated = items.map(item => item.id === id ? { ...item, set: updatedSet } : item);
        setItems(updated);
        pushChange(updated);
    }

    const isCardio = exerciseInfo?.type === "Cardio";
    const bodyClass = `detail-body ${isCardio ? "is-cardio" : "is-strength"}`;

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
                    {isCardio ? (
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