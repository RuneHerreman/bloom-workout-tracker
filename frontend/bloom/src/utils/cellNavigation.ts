import type { KeyboardEvent } from "react";

/**
 * Spreadsheet-style movement between set cells: ↑/↓ go to the same column in the
 * row above/below, ←/→ to the previous/next cell (number inputs have no caret
 * position, so they always move). Up/down replace the browser's number stepping.
 */
export function moveBetweenCells(e: KeyboardEvent<HTMLDivElement>) {
    const input = e.target as HTMLElement;
    if (input.tagName !== "INPUT" || e.altKey || e.ctrlKey || e.metaKey || e.shiftKey) return;
    const cells = (row: Element | null) => (row ? [...row.querySelectorAll<HTMLInputElement>("input")] : []);
    const row = e.currentTarget;
    const own = cells(row);
    const col = own.indexOf(input as HTMLInputElement);
    let next: HTMLInputElement | undefined;

    if (e.key === "ArrowUp" || e.key === "ArrowDown") {
        let sibling = e.key === "ArrowUp" ? row.previousElementSibling : row.nextElementSibling;
        while (sibling && !sibling.classList.contains("set-row")) {
            sibling = e.key === "ArrowUp" ? sibling.previousElementSibling : sibling.nextElementSibling;
        }
        const target = cells(sibling);
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
