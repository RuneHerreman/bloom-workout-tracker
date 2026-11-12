import { useEffect, useRef, useState } from "react";
import { GEAR_KINDS, KIND_LABELS, type GearKind } from "./gearKinds.ts";
import GearIcon from "./GearIcon.tsx";

interface Props {
    name: string;
    /** The icon shown now: picked, or guessed from the name. */
    kind: GearKind;
    /** Whether the user picked it (false: guessed). */
    picked: boolean;
    /** The kind the name alone would give, for the "Auto" choice. */
    guessed: GearKind;
    disabled?: boolean;
    onPick: (kind: GearKind | null) => void;
}

/**
 * The gear item's icon, as a button: opens a small grid of icons to pick from,
 * plus "Auto" to go back to the one read from the name.
 */
function GearIconPicker({ name, kind, picked, guessed, disabled, onPick }: Props) {
    const [open, setOpen] = useState(false);
    const rootRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (!open) return;
        const close = (e: PointerEvent) => { if (!rootRef.current?.contains(e.target as Node)) setOpen(false); };
        const esc = (e: KeyboardEvent) => { if (e.key === "Escape") setOpen(false); };
        document.addEventListener("pointerdown", close);
        document.addEventListener("keydown", esc);
        return () => { document.removeEventListener("pointerdown", close); document.removeEventListener("keydown", esc); };
    }, [open]);

    function pick(k: GearKind | null) {
        setOpen(false);
        onPick(k);
    }

    return (
        <div className="gear-icon-picker" ref={rootRef}>
            <button type="button" className="gear-item-icon gear-item-icon-btn" aria-haspopup="true" aria-expanded={open}
                aria-label={`Icon for ${name}: ${KIND_LABELS[kind]}. Change`} title="Change icon"
                disabled={disabled} onClick={() => setOpen(o => !o)}>
                <GearIcon kind={kind} />
            </button>
            {open && (
                <div className="gear-icon-menu" role="menu" aria-label={`Icon for ${name}`}>
                    <div className="gear-icon-grid">
                        {GEAR_KINDS.map(k => (
                            <button key={k} type="button" role="menuitemradio" aria-checked={picked && kind === k}
                                className={`gear-icon-choice${picked && kind === k ? " active" : ""}`}
                                title={KIND_LABELS[k]} onClick={() => pick(k)}>
                                <GearIcon kind={k} size={18} />
                                <span>{KIND_LABELS[k]}</span>
                            </button>
                        ))}
                    </div>
                    <button type="button" role="menuitemradio" aria-checked={!picked}
                        className={`gear-icon-auto${!picked ? " active" : ""}`} onClick={() => pick(null)}>
                        <GearIcon kind={guessed} size={14} /> Auto, from the name
                    </button>
                </div>
            )}
        </div>
    );
}

export default GearIconPicker;
