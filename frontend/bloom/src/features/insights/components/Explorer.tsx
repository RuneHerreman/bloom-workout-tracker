import { useMemo, useRef, useState, type ReactNode } from "react";
import { Search, ArrowUpRight, ArrowDownRight, Minus } from "lucide-react";
import { Sparkline } from "./ChartParts.tsx";

export interface ExplorerItem {
    id: string;
    name: string;
    sub: string;
    spark: number[];
    /** Signed change, already formatted, e.g. "+12%" — with its direction for icon + colour. */
    change: { text: string; direction: "up" | "down" | "flat" } | null;
    /** Right-aligned secondary figure, e.g. "8 sessions". */
    meta: string;
}

interface ExplorerProps {
    title: string;
    items: ExplorerItem[];
    selectedId: string | null;
    onSelect: (id: string) => void;
    searchPlaceholder: string;
    children: ReactNode;
}

const DIRECTION_ICON = { up: ArrowUpRight, down: ArrowDownRight, flat: Minus };

/**
 * Master–detail: a searchable list of exercises (each with its trend) beside
 * the selected one's detail. One click shows everything about an exercise —
 * no checkbox juggling. On phones the list collapses into a picker above the detail.
 */
function Explorer({ title, items, selectedId, onSelect, searchPlaceholder, children }: ExplorerProps) {
    const [query, setQuery] = useState("");
    const listRef = useRef<HTMLUListElement>(null);
    const visible = useMemo(() => {
        const q = query.trim().toLowerCase();
        return q ? items.filter(i => i.name.toLowerCase().includes(q) || i.sub.toLowerCase().includes(q)) : items;
    }, [items, query]);

    function onKeyDown(e: React.KeyboardEvent) {
        if (e.key !== "ArrowDown" && e.key !== "ArrowUp") return;
        e.preventDefault();
        const i = visible.findIndex(v => v.id === selectedId);
        const next = visible[Math.min(visible.length - 1, Math.max(0, i + (e.key === "ArrowDown" ? 1 : -1)))];
        if (next) {
            onSelect(next.id);
            listRef.current?.querySelector<HTMLElement>(`[data-id="${next.id}"]`)?.scrollIntoView({ block: "nearest" });
        }
    }

    return (
        <section className="explorer">
            <aside className="explorer-list-pane">
                <div className="explorer-list-head">
                    <h2 className="explorer-title">{title} <span className="explorer-count">{items.length}</span></h2>
                    <label className="explorer-search">
                        <Search size={14} aria-hidden />
                        <input
                            type="search"
                            value={query}
                            placeholder={searchPlaceholder}
                            aria-label={searchPlaceholder}
                            onChange={e => setQuery(e.target.value)}
                            onKeyDown={onKeyDown}
                        />
                    </label>
                </div>
                <ul className="explorer-list" ref={listRef} role="listbox" aria-label={title} onKeyDown={onKeyDown} tabIndex={-1}>
                    {visible.map(item => {
                        const Icon = item.change ? DIRECTION_ICON[item.change.direction] : null;
                        return (
                            <li
                                key={item.id}
                                data-id={item.id}
                                role="option"
                                aria-selected={item.id === selectedId}
                                className={`explorer-item${item.id === selectedId ? " selected" : ""}`}
                                onClick={() => onSelect(item.id)}
                            >
                                <span className="explorer-item-text">
                                    <span className="explorer-item-name">{item.name}</span>
                                    <span className="explorer-item-sub">{item.sub}</span>
                                </span>
                                <Sparkline values={item.spark} />
                                <span className="explorer-item-figures">
                                    {item.change && Icon ? (
                                        <span className={`trend trend-${item.change.direction}`}>
                                            <Icon size={12} aria-hidden />{item.change.text}
                                        </span>
                                    ) : <span className="trend trend-none">—</span>}
                                    <span className="explorer-item-meta">{item.meta}</span>
                                </span>
                            </li>
                        );
                    })}
                    {visible.length === 0 && <li className="explorer-empty">Nothing matches “{query}”.</li>}
                </ul>
            </aside>

            {/* Phone: a compact picker replaces the list */}
            <label className="explorer-picker">
                <span className="sr-only">{title}</span>
                <select value={selectedId ?? ""} onChange={e => onSelect(e.target.value)}>
                    {items.map(i => <option key={i.id} value={i.id}>{i.name}{i.change ? ` (${i.change.text})` : ""}</option>)}
                </select>
            </label>

            <div className="explorer-detail">{children}</div>
        </section>
    );
}

export default Explorer;
