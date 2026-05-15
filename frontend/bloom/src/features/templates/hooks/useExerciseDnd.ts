import { useState } from "react";
import { arrayMove } from "@dnd-kit/sortable";
import type { DragStartEvent, DragEndEvent } from "@dnd-kit/core";
import type { TemplateExercise } from "../../../assets/js/data/apiTypes.ts";

export function useExerciseDnd(
    exercises: TemplateExercise[],
    onReorder: (reordered: TemplateExercise[]) => void
) {
    const [activeId, setActiveId] = useState<string | null>(null);
    const activeExercise = exercises.find(ex => ex.exerciseId === activeId) ?? null;

    function onDragStart(e: DragStartEvent) {
        setActiveId(e.active.id as string);
    }

    function onDragEnd(event: DragEndEvent) {
        setActiveId(null);
        const { active, over } = event;
        if (!over || active.id === over.id) return;
        const oldIndex = exercises.findIndex(ex => ex.exerciseId === active.id);
        const newIndex = exercises.findIndex(ex => ex.exerciseId === over.id);
        onReorder(arrayMove(exercises, oldIndex, newIndex).map((ex, i) => ({ ...ex, order: i + 1 })));
    }

    function onDragCancel() {
        setActiveId(null);
    }

    return { activeExercise, onDragStart, onDragEnd, onDragCancel };
}