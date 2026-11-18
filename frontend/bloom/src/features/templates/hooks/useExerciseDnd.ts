import { useLayoutEffect, useRef, useState } from "react";
import { arrayMove } from "@dnd-kit/sortable";
import { MeasuringStrategy, type DragStartEvent, type DragEndEvent, type Modifier } from "@dnd-kit/core";
import { getEventCoordinates } from "@dnd-kit/utilities";

/** Puts the drag chip's grip under the pointer instead of at the card's top-left corner. */
// The overlay starts at the card's top-left; its own size isn't reliably measured on the
// first frames (dnd-kit falls back to the whole card), so offset by the chip's known size.
const CHIP_GRIP_X = 16;
const CHIP_HALF_HEIGHT = 19;
const gripToCursor: Modifier = ({ activatorEvent, activeNodeRect, transform }) => {
    const at = activatorEvent && getEventCoordinates(activatorEvent);
    if (!at || !activeNodeRect) return transform;
    return {
        ...transform,
        x: transform.x + at.x - activeNodeRect.left - CHIP_GRIP_X,
        y: transform.y + at.y - activeNodeRect.top - CHIP_HALF_HEIGHT,
    };
};

function scrollParent(el: HTMLElement): HTMLElement | Window {
    for (let p = el.parentElement; p; p = p.parentElement) {
        const { overflowY } = getComputedStyle(p);
        if ((overflowY === "auto" || overflowY === "scroll") && p.scrollHeight > p.clientHeight) return p;
    }
    return window;
}

const scrollTopOf = (s: HTMLElement | Window) => (s instanceof Window ? s.scrollY : s.scrollTop);

/** Measured at drag start, before the cards collapse. */
interface Anchor {
    card: HTMLElement;
    list: HTMLElement;
    scroller: HTMLElement | Window;
    listHeight: number;
    pointerY: number;
}

/**
 * Exercise reordering. While dragging, the list collapses every card to its
 * header (style the `is-dragging` list class), so moves are short hops rather
 * than whole-card jumps; the dragged card is kept under the pointer as it shrinks.
 */
export function useExerciseDnd<T extends { exerciseId: string }>(
    exercises: T[],
    onReorder: (reordered: T[]) => void,
    // Must match the ids handed to SortableContext / useSortable.
    getId: (ex: T) => string = ex => ex.exerciseId
) {
    const [activeId, setActiveId] = useState<string | null>(null);
    const activeExercise = exercises.find(ex => getId(ex) === activeId) ?? null;
    const anchor = useRef<Anchor | null>(null);

    // Runs after the collapse renders but before it paints. Collapsing shrinks
    // everything above the dragged card, which would slide the list out from under
    // the pointer. Keep the list's height (so the page can't get shorter and clamp
    // the scroll), then scroll so the dragged card is under the pointer again; if
    // the page can't scroll up that far, pad the top of the list instead.
    useLayoutEffect(() => {
        const a = anchor.current;
        if (!activeId || !a) return;
        a.list.style.minHeight = `${a.listHeight}px`;
        // The browser's own scroll anchoring would fight the padding below.
        const scrollerStyle = a.scroller instanceof Window ? document.documentElement.style : a.scroller.style;
        scrollerStyle.overflowAnchor = "none";
        const r = a.card.getBoundingClientRect();
        const delta = r.top + r.height / 2 - a.pointerY;
        const before = scrollTopOf(a.scroller);
        a.scroller.scrollBy({ top: delta, behavior: "instant" });
        const rest = delta - (scrollTopOf(a.scroller) - before);
        if (rest < -1) a.list.style.paddingTop = `${-rest}px`;
        return () => {
            a.list.style.minHeight = "";
            a.list.style.paddingTop = "";
            scrollerStyle.overflowAnchor = "";
            anchor.current = null;
        };
    }, [activeId]);

    function onDragStart(e: DragStartEvent) {
        const id = String(e.active.id);
        const at = e.activatorEvent && getEventCoordinates(e.activatorEvent);
        const card = document.querySelector<HTMLElement>(`[data-exercise-id="${CSS.escape(id)}"]`);
        const list = card?.closest<HTMLElement>(".exercise-list");
        anchor.current = at && card && list
            ? { card, list, scroller: scrollParent(card), listHeight: list.getBoundingClientRect().height, pointerY: at.y }
            : null;
        setActiveId(id);
    }

    function onDragEnd(event: DragEndEvent) {
        setActiveId(null);
        const { active, over } = event;
        if (!over || active.id === over.id) return;
        const oldIndex = exercises.findIndex(ex => getId(ex) === active.id);
        const newIndex = exercises.findIndex(ex => getId(ex) === over.id);
        if (oldIndex < 0 || newIndex < 0) return;
        onReorder(arrayMove(exercises, oldIndex, newIndex).map((ex, i) => ({ ...ex, order: i + 1 })));
    }

    function onDragCancel() {
        setActiveId(null);
    }

    return {
        activeExercise,
        isDragging: activeId != null,
        onDragStart, onDragEnd, onDragCancel,
        /** Spread onto DndContext: re-measure as the cards collapse. */
        dndProps: { measuring: { droppable: { strategy: MeasuringStrategy.Always } } },
        /** Pass to DragOverlay. */
        overlayModifiers: [gripToCursor],
    };
}
