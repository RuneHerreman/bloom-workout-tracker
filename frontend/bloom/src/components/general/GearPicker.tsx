import { useEffect, useRef, useState } from "react";
import { createPortal } from "react-dom";
import { NavLink } from "react-router-dom";
import { Check, Backpack, Settings2, X } from "lucide-react";

interface GearPickerProps {
    /** Gear attached to this exercise. */
    selected: string[];
    /** The user's gear closet (from their profile). */
    available: string[];
    onChange: (gear: string[]) => void;
}

/**
 * Footer control for attaching gear (shoes, bike, belt…) to an exercise.
 * Selected gear shows as removable chips; the button opens a picker
 * listing the user's gear closet.
 */
function GearPicker({ selected, available, onChange }: GearPickerProps) {
    const [menu, setMenu] = useState<{ x: number; y: number } | null>(null);
    const btnRef = useRef<HTMLButtonElement>(null);
    const menuRef = useRef<HTMLDivElement>(null);

    // Keep gear that was removed from the profile visible so it can still be detached.
    const options = [...available, ...selected.filter(g => !available.includes(g))];

    function toggleMenu() {
        if (menu) { setMenu(null); return; }
        const r = btnRef.current?.getBoundingClientRect();
        if (!r) return;
        const height = Math.min(options.length, 6) * 34 + 90;
        const below = r.bottom + height < window.innerHeight;
        setMenu({ x: Math.min(r.left, window.innerWidth - 232), y: below ? r.bottom + 4 : r.top - 4 - height });
    }

    function toggle(item: string) {
        onChange(selected.includes(item) ? selected.filter(g => g !== item) : [...selected, item]);
    }

    useEffect(() => {
        if (!menu) return;
        function onDown(e: PointerEvent) {
            const t = e.target as Node;
            if (menuRef.current?.contains(t) || btnRef.current?.contains(t)) return;
            setMenu(null);
        }
        function onKey(e: KeyboardEvent) { if (e.key === "Escape") setMenu(null); }
        function onScroll(e: Event) {
            if (menuRef.current?.contains(e.target as Node)) return;
            setMenu(null);
        }
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
        <div className="gear-picker">
            {selected.map(g => (
                <span key={g} className="gear-chip">
                    <Backpack size={11} aria-hidden />
                    {g}
                    <button type="button" tabIndex={-1} aria-label={`Remove ${g}`} onClick={() => toggle(g)}>
                        <X size={10} />
                    </button>
                </span>
            ))}
            <button
                ref={btnRef}
                type="button"
                className="button-component modern gear-picker-trigger"
                aria-haspopup="menu"
                aria-expanded={!!menu}
                onClick={toggleMenu}
            >
                <Backpack size={14} />
                {selected.length ? "Gear" : "Add gear"}
            </button>
            {menu && createPortal(
                <div ref={menuRef} className="gear-menu" role="menu" style={{ left: menu.x, top: menu.y }}>
                    <p className="gear-menu-title">Your gear</p>
                    {options.length === 0 ? (
                        <p className="gear-menu-empty">No gear yet. Add your shoes, bike or belt in your profile.</p>
                    ) : (
                        <div className="gear-menu-list">
                            {options.map(g => {
                                const active = selected.includes(g);
                                return (
                                    <button
                                        key={g}
                                        type="button"
                                        role="menuitemcheckbox"
                                        aria-checked={active}
                                        className={`gear-menu-option${active ? " active" : ""}`}
                                        onClick={() => toggle(g)}
                                    >
                                        <span className="gear-menu-check">{active && <Check size={11} />}</span>
                                        {g}
                                    </button>
                                );
                            })}
                        </div>
                    )}
                    <NavLink to="/profile#gear" className="gear-menu-manage" onClick={() => setMenu(null)}>
                        <Settings2 size={12} /> Manage gear
                    </NavLink>
                </div>,
                document.body
            )}
        </div>
    );
}

export default GearPicker;
