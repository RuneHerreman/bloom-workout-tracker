import { useEffect, useRef } from "react";

export function useShortcut(key: string, handler: () => void, ctrl = false, alt = false) {
    const handlerRef = useRef(handler);
    handlerRef.current = handler;

    useEffect(() => {
        function onKeyDown(e: KeyboardEvent) {
            if (ctrl !== e.ctrlKey) return;
            if (alt !== e.altKey) return;
            if (e.key.toLowerCase() !== key.toLowerCase()) return;
            e.preventDefault();
            handlerRef.current();
        }
        window.addEventListener("keydown", onKeyDown);
        return () => window.removeEventListener("keydown", onKeyDown);
    }, [key, ctrl, alt]);
}
