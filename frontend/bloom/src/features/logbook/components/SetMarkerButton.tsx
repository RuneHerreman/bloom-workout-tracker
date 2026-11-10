import { useEffect, useRef, useState } from "react";
import { createPortal } from "react-dom";
import { Check } from "lucide-react";
import type { SetMarker } from "../../../assets/js/data/apiTypes.ts";
import { MARKERS, markerShort } from "../logbookUtils.ts";

interface SetMarkerButtonProps {
    marker: SetMarker | null | undefined;
    /** Working-set number shown when no marker is set. */
    number: number;
    onChange: (marker: SetMarker | null) => void;
}

function SetMarkerButton({ marker, number, onChange }: SetMarkerButtonProps) {
    const [menu, setMenu] = useState<{ x: number; y: number } | null>(null);
    const btnRef = useRef<HTMLButtonElement>(null);
    const menuRef = useRef<HTMLDivElement>(null);
    const short = markerShort(marker);
    const current = MARKERS.find(m => m.value === (marker ?? null))!;

    function toggle() {
        if (menu) { setMenu(null); return; }
        const r = btnRef.current?.getBoundingClientRect();
        if (!r) return;
        const MENU_HEIGHT = 240;
        const below = r.bottom + MENU_HEIGHT < window.innerHeight;
        setMenu({ x: r.left, y: below ? r.bottom + 4 : Math.max(8, r.top - 4 - MENU_HEIGHT) });
    }

    useEffect(() => {
        if (!menu) return;
        function onDown(e: PointerEvent) {
            const t = e.target as Node;
            if (menuRef.current?.contains(t) || btnRef.current?.contains(t)) return;
            setMenu(null);
        }
        function onKey(e: KeyboardEvent) { if (e.key === "Escape") setMenu(null); }
        function onScroll() { setMenu(null); }
        document.addEventListener("pointerdown", onDown);
        document.addEventListener("keydown", onKey);
        window.addEventListener("scroll", onScroll, true);
        return () => {
            document.removeEventListener("pointerdown", onDown);
            document.removeEventListener("keydown", onKey);
            window.removeEventListener("scroll", onScroll, true);
        };
    }, [menu]);

    return (
        <div className="set-marker-cell">
            <button
                ref={btnRef}
                type="button"
                tabIndex={-1}
                className={`set-marker-btn${short ? ` marker-${short.toLowerCase()}` : ""}`}
                title={`${current.label} — click to change set type`}
                aria-label={`Set type: ${current.label}. Change set type`}
                aria-haspopup="menu"
                aria-expanded={!!menu}
                onClick={toggle}
            >
                {short ?? number}
            </button>
            {menu && createPortal(
                <div ref={menuRef} className="set-marker-menu" role="menu" style={{ left: menu.x, top: menu.y }}>
                    <p className="set-marker-menu-title">Set type</p>
                    {MARKERS.map(m => {
                        const active = (marker ?? null) === m.value;
                        return (
                            <button
                                key={m.short}
                                type="button"
                                role="menuitemradio"
                                aria-checked={active}
                                className={`set-marker-option${active ? " active" : ""}`}
                                onClick={() => { onChange(m.value); setMenu(null); }}
                            >
                                <span className={`set-marker-chip${m.value ? ` marker-${m.short.toLowerCase()}` : ""}`}>
                                    {m.value ? m.short : number}
                                </span>
                                <span className="set-marker-option-text">
                                    <span className="set-marker-option-label">{m.label}</span>
                                    <span className="set-marker-option-hint">{m.hint}</span>
                                </span>
                                {active && <Check size={13} className="set-marker-option-check" />}
                            </button>
                        );
                    })}
                </div>,
                document.body
            )}
        </div>
    );
}

export default SetMarkerButton;
