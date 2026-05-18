import { useState, useEffect, useRef } from "react";
import { Search } from "lucide-react";
import type { LoggedWorkout } from "../api.ts";
import { type LogFilterType, dominantTypeFromLog, matchesLogFilter } from "../logbookUtils.ts";
import LogSidebarCard from "./LogSidebarCard.tsx";

const FILTERS: LogFilterType[] = ["Strength", "Cardio", "Plyometric"];

interface LogSideBarProps {
    logs: LoggedWorkout[];
    selectedId: string | null;
    loading: boolean;
    onSelect: (id: string) => void;
}

function LogSideBar({ logs, selectedId, loading, onSelect }: LogSideBarProps) {
    const [activeFilters, setActiveFilters] = useState<Set<LogFilterType>>(new Set());
    const [query, setQuery] = useState("");
    const listRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (!selectedId || !listRef.current) return;
        const el = listRef.current.querySelector<HTMLElement>(`[data-id="${selectedId}"]`);
        el?.scrollIntoView({ block: "center", behavior: "smooth" });
    }, [selectedId]);

    function toggleFilter(type: LogFilterType) {
        setActiveFilters(prev => {
            const next = new Set(prev);
            if (next.has(type)) next.delete(type); else next.add(type);
            return next;
        });
    }

    const q = query.trim().toLowerCase();
    const visible = logs.filter(l =>
        (q === "" || l.name.toLowerCase().includes(q)) &&
        (activeFilters.size === 0 || [...activeFilters].some(f => matchesLogFilter(l, f)))
    );

    return (
        <aside className="feature-sidebar">
            <div className="feature-sidebar-header">
                <div className="feature-sidebar-title-row">
                    <span className="feature-sidebar-title">Your logs</span>
                    {!loading && <span className="feature-sidebar-count">{logs.length}</span>}
                </div>
                <div className="exercise-library-search-wrap">
                    <Search className="exercise-library-search-icon" size={14} />
                    <input
                        className="exercise-library-search"
                        placeholder="Search logs…"
                        value={query}
                        onChange={e => setQuery(e.target.value)}
                    />
                </div>
                <div className="filter-chips">
                    {FILTERS.map(f => (
                        <button
                            key={f}
                            className={`filter-chip ${f.toLowerCase()}${activeFilters.has(f) ? " active" : ""}`}
                            onClick={() => toggleFilter(f)}
                        >
                            {f}
                        </button>
                    ))}
                </div>
            </div>
            <div className="feature-sidebar-list" ref={listRef}>
                {loading ? (
                    <p className="feature-sidebar-loading">Loading…</p>
                ) : visible.length === 0 ? (
                    <p className="feature-sidebar-empty">No logs found</p>
                ) : (
                    visible.map(l => (
                        <LogSidebarCard
                            key={l.id}
                            log={l}
                            colorClass={dominantTypeFromLog(l)}
                            isActive={l.id === selectedId}
                            onSelect={() => onSelect(l.id)}
                        />
                    ))
                )}
            </div>
        </aside>
    );
}

export default LogSideBar;