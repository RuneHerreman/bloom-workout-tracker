import { useEffect, useLayoutEffect, useRef, type RefObject } from "react";

const FOCUSABLE = [
    "a[href]", "button:not([disabled])", "input:not([disabled]):not([type=hidden])",
    "select:not([disabled])", "textarea:not([disabled])", "[tabindex]:not([tabindex='-1'])",
].join(",");

// Open traps, innermost last: only the top one reacts to Tab / Escape.
const stack: HTMLElement[] = [];

export function focusables(root: HTMLElement): HTMLElement[] {
    return [...root.querySelectorAll<HTMLElement>(FOCUSABLE)]
        .filter(el => el.getClientRects().length > 0 && !el.closest("[inert]"));
}

/**
 * Modal focus handling: moves focus into `ref` on mount (unless a child already took it,
 * e.g. via autoFocus), keeps Tab / Shift+Tab cycling inside, closes on Escape, and puts
 * focus back where it was on unmount. `initialFocus` picks the first element to focus.
 */
export function useFocusTrap(
    ref: RefObject<HTMLElement | null>,
    onEscape?: () => void,
    initialFocus?: (root: HTMLElement) => HTMLElement | null | undefined,
) {
    const onEscapeRef = useRef(onEscape);
    useLayoutEffect(() => { onEscapeRef.current = onEscape; });
    const initialFocusRef = useRef(initialFocus);

    useEffect(() => {
        const root = ref.current;
        if (!root) return;
        const previous = document.activeElement as HTMLElement | null;
        stack.push(root);

        if (!root.contains(document.activeElement)) {
            const target = initialFocusRef.current?.(root) ?? focusables(root)[0] ?? root;
            target.focus({ preventScroll: true });
        }

        function onKeyDown(e: KeyboardEvent) {
            if (!root || stack[stack.length - 1] !== root) return;
            if (e.key === "Escape" && onEscapeRef.current) {
                e.stopPropagation();
                onEscapeRef.current();
                return;
            }
            if (e.key !== "Tab") return;
            const items = focusables(root);
            if (items.length === 0) { e.preventDefault(); root.focus(); return; }
            const first = items[0]!;
            const last = items[items.length - 1]!;
            const active = document.activeElement;
            if (!root.contains(active)) { e.preventDefault(); first.focus(); }
            else if (e.shiftKey && active === first) { e.preventDefault(); last.focus(); }
            else if (!e.shiftKey && active === last) { e.preventDefault(); first.focus(); }
        }

        document.addEventListener("keydown", onKeyDown, true);
        return () => {
            document.removeEventListener("keydown", onKeyDown, true);
            stack.splice(stack.indexOf(root), 1);
            if (previous && document.contains(previous)) previous.focus({ preventScroll: true });
        };
    }, [ref]);
}
