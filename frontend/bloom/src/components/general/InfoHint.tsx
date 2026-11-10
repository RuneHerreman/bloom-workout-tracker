import { useEffect, useRef, useState, type ReactNode } from "react";
import { createPortal } from "react-dom";
import { Info } from "lucide-react";

interface InfoHintProps {
    /** Short heading shown in bold at the top of the popover. */
    title?: string;
    children: ReactNode;
    /** Accessible label for the trigger icon. */
    label?: string;
    size?: number;
}

type Position = { x: number; y: number; placeAbove: boolean };

/**
 * Small "i" trigger that explains a term. Opens on hover / keyboard focus,
 * toggles on tap for touch devices, and closes on Escape or outside tap.
 * The popover is portalled to <body> so it is never clipped by card overflow.
 */
function InfoHint({ title, children, label = "More info", size = 12 }: InfoHintProps) {
    const [pos, setPos] = useState<Position | null>(null);
    const [pinned, setPinned] = useState(false);
    const triggerRef = useRef<HTMLButtonElement>(null);
    const popRef = useRef<HTMLDivElement>(null);

    function open() {
        const rect = triggerRef.current?.getBoundingClientRect();
        if (!rect) return;
        const placeAbove = rect.bottom + 160 > window.innerHeight;
        setPos({
            x: Math.min(Math.max(rect.left + rect.width / 2, 140), window.innerWidth - 140),
            y: placeAbove ? rect.top : rect.bottom,
            placeAbove,
        });
    }

    function close() {
        setPos(null);
        setPinned(false);
    }

    useEffect(() => {
        if (!pos) return;
        function onKey(e: KeyboardEvent) { if (e.key === "Escape") close(); }
        function onDown(e: PointerEvent) {
            const t = e.target as Node;
            if (popRef.current?.contains(t) || triggerRef.current?.contains(t)) return;
            close();
        }
        function onScroll() { if (!pinned) close(); }
        document.addEventListener("keydown", onKey);
        document.addEventListener("pointerdown", onDown);
        window.addEventListener("scroll", onScroll, true);
        return () => {
            document.removeEventListener("keydown", onKey);
            document.removeEventListener("pointerdown", onDown);
            window.removeEventListener("scroll", onScroll, true);
        };
    }, [pos, pinned]);

    return (
        <>
            <button
                ref={triggerRef}
                type="button"
                className={`info-hint-trigger${pos ? " open" : ""}`}
                aria-label={label}
                aria-expanded={!!pos}
                tabIndex={0}
                onMouseEnter={() => { if (!pinned) open(); }}
                onMouseLeave={() => { if (!pinned) setPos(null); }}
                onFocus={open}
                onBlur={() => { if (!pinned) setPos(null); }}
                onClick={e => {
                    e.stopPropagation();
                    if (pinned) close(); else { open(); setPinned(true); }
                }}
            >
                <Info size={size} />
            </button>
            {pos && createPortal(
                <div
                    ref={popRef}
                    role="tooltip"
                    className={`info-hint-pop${pos.placeAbove ? " above" : ""}`}
                    style={{ left: pos.x, top: pos.y }}
                >
                    {title && <p className="info-hint-title">{title}</p>}
                    <div className="info-hint-body">{children}</div>
                </div>,
                document.body
            )}
        </>
    );
}

export default InfoHint;
