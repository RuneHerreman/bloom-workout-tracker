import type { KeyboardEvent } from "react";

/**
 * Spreadsheet-style movement between set cells: ↑/↓ go to the same column in the
 * row above/below, ←/→ to the previous/next cell (number inputs have no caret
 * position, so they always move). Up/down replace the browser's number stepping.
 * Enter (the phone keyboard's Next/Go) walks forward: reps → weight → RIR, then the
 * first cell of the next set; on the last set it closes the keyboard.
 */
export function moveBetweenCells(e: KeyboardEvent<HTMLDivElement>) {
    const input = e.target as HTMLElement;
    if (input.tagName !== "INPUT" || e.altKey || e.ctrlKey || e.metaKey || e.shiftKey) return;
    const cells = (row: Element | null) => (row ? [...row.querySelectorAll<HTMLInputElement>("input")] : []);
    const row = e.currentTarget;
    const own = cells(row);
    const col = own.indexOf(input as HTMLInputElement);
    let next: HTMLInputElement | undefined;
    const sibling = (from: Element, dir: "up" | "down") => {
        let el = dir === "up" ? from.previousElementSibling : from.nextElementSibling;
        while (el && !el.classList.contains("set-row")) {
            el = dir === "up" ? el.previousElementSibling : el.nextElementSibling;
        }
        return el;
    };

    if (e.key === "Enter") {
        e.preventDefault();
        next = own[col + 1] ?? cells(sibling(row, "down"))[0];
        if (!next) (input as HTMLInputElement).blur();
    } else if (e.key === "ArrowUp" || e.key === "ArrowDown") {
        const target = cells(sibling(row, e.key === "ArrowUp" ? "up" : "down"));
        next = target[Math.min(col, target.length - 1)];
        e.preventDefault(); // no number stepping, even at the first/last row
    } else if (e.key === "ArrowLeft" || e.key === "ArrowRight") {
        const el = input as HTMLInputElement;
        const caretMovable = el.type !== "number" && el.selectionStart !== null &&
            (e.key === "ArrowLeft" ? el.selectionStart > 0 : (el.selectionEnd ?? 0) < el.value.length);
        if (caretMovable) return;
        next = own[col + (e.key === "ArrowLeft" ? -1 : 1)];
        if (next) e.preventDefault();
    }
    if (next) { next.focus(); next.select(); }
}
