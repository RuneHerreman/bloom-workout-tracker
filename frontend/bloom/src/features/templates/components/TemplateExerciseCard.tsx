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

function SortableSetRow({ item, index, type }: { item: RowItem; index: number; type: string | undefined }) {
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
                <input readOnly value={item.set.reps?.toString() ?? ""} />
            ) : (
                <>
                    <input readOnly value={`${item.set.distance ?? ""} ${item.set.distanceUnit ?? ""}`} />
                    <input readOnly value={item.set.duration?.toString() ?? ""} />
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

    function handleDragEnd(event: DragEndEvent) {
        const { active, over } = event;
        if (!over || active.id === over.id) return;

        const oldIndex = items.findIndex(item => item.id === active.id);
        const newIndex = items.findIndex(item => item.id === over.id);
        const reordered = arrayMove(items, oldIndex, newIndex);
        setItems(reordered);

        const updatedSets = reordered.map((item, i) => ({ ...item.set, order: i + 1 }));
        onSetsChange?.(exercise.exerciseId, updatedSets);
    }

    return (
        <div className="template-exercise-card">
            <header>
                <div>
                    <h3 className="detail-exercise-name">{exerciseInfo?.name}</h3>
                    <p className="detail-exercise-info">{exerciseInfo?.type} · {exerciseInfo?.targetMuscles.join(" - ")}</p>
                </div>
            </header>
            <section className="detail-body">
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
                            <SortableSetRow key={item.id} item={item} index={index} type={exerciseInfo?.type} />
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
