import { useEffect, useId, useMemo, useRef, useState } from "react";
import { Search, X } from "lucide-react";

export interface LiftOption {
    id: string;
    name: string;
    /** Latest estimate, shown on the right so you can tell lifts apart at a glance. */
    e1rm: number;
    lastDate: string;
}

interface Props {
    options: LiftOption[];
    value: string;
    onChange: (id: string) => void;
}

const fmtAgo = (iso: string) => {
    const days = Math.floor((Date.now() - new Date(iso).getTime()) / 86_400_000);
    if (days <= 0) return "today";
    if (days === 1) return "yesterday";
    if (days < 14) return `${days} days ago`;
    if (days < 60) return `${Math.round(days / 7)} weeks ago`;
    return new Date(iso).toLocaleDateString("en-GB", { month: "short", year: "numeric" });
};

/**
 * Search your logged lifts, most recently trained first. Typing filters; arrows,
 * Enter and Escape work like a select. Clearing it goes back to "any lift".
 */
function LiftPicker({ options, value, onChange }: Props) {
    const [open, setOpen] = useState(false);
    const [query, setQuery] = useState("");
    const [active, setActive] = useState(0);
    const rootRef = useRef<HTMLDivElement>(null);
    const listRef = useRef<HTMLUListElement>(null);
    const listId = useId();
    const selected = options.find(o => o.id === value) ?? null;

    const filtered = useMemo(() => {
        const q = query.trim().toLowerCase();
        return q ? options.filter(o => o.name.toLowerCase().includes(q)) : options;
    }, [options, query]);

    useEffect(() => {
        if (!open) return;
        const close = (e: PointerEvent) => { if (!rootRef.current?.contains(e.target as Node)) setOpen(false); };
        document.addEventListener("pointerdown", close);
        return () => document.removeEventListener("pointerdown", close);
    }, [open]);

    useEffect(() => {
        listRef.current?.querySelector<HTMLElement>(`[data-index="${active}"]`)?.scrollIntoView({ block: "nearest" });
    }, [active, open]);

    function openList() {
        setQuery("");
        setActive(Math.max(0, options.findIndex(o => o.id === value)));
        setOpen(true);
    }
    function pick(id: string) {
        onChange(id);
        setOpen(false);
        setQuery("");
    }

    function onKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
        if (!open && (e.key === "ArrowDown" || e.key === "Enter")) { e.preventDefault(); openList(); return; }
        if (!open) return;
        if (e.key === "ArrowDown") { e.preventDefault(); setActive(a => Math.min(filtered.length - 1, a + 1)); }
        else if (e.key === "ArrowUp") { e.preventDefault(); setActive(a => Math.max(0, a - 1)); }
        else if (e.key === "Enter") { e.preventDefault(); if (filtered[active]) pick(filtered[active].id); }
        else if (e.key === "Escape") { e.preventDefault(); setOpen(false); }
    }

    return (
        <div className={`lift-picker${open ? " open" : ""}`} ref={rootRef}>
            <Search className="lift-picker-icon" size={15} aria-hidden />
            <input
                role="combobox"
                aria-label="Exercise"
                aria-expanded={open}
                aria-controls={listId}
                aria-activedescendant={open && filtered[active] ? `${listId}-${active}` : undefined}
                aria-autocomplete="list"
                placeholder={options.length ? "Search your lifts" : "No lifts logged yet"}
                value={open ? query : selected?.name ?? ""}
                onFocus={openList}
                onClick={() => !open && openList()}
                onChange={e => { setQuery(e.target.value); setActive(0); if (!open) setOpen(true); }}
                onKeyDown={onKeyDown}
                disabled={!options.length}
            />
            {selected && !open && (
                <button type="button" className="lift-picker-clear" aria-label="Clear exercise" onClick={() => onChange("")}>
                    <X size={14} />
                </button>
            )}
            {open && (
                <ul className="lift-picker-list" role="listbox" id={listId} ref={listRef}>
                    {filtered.length === 0 && <li className="lift-picker-empty">No logged lift matches “{query}”</li>}
                    {filtered.map((o, i) => (
                        <li
                            key={o.id}
                            id={`${listId}-${i}`}
                            data-index={i}
                            role="option"
                            aria-selected={o.id === value}
                            className={`lift-picker-option${i === active ? " active" : ""}${o.id === value ? " selected" : ""}`}
                            onPointerDown={e => e.preventDefault()}
                            onClick={() => pick(o.id)}
                            onPointerEnter={() => setActive(i)}
                        >
                            <span className="lift-picker-name">{o.name}</span>
                            <span className="lift-picker-meta">
                                <b>{Math.round(o.e1rm * 10) / 10} kg</b> · {fmtAgo(o.lastDate)}
                            </span>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}

export default LiftPicker;
