import { useState, useEffect, useLayoutEffect, useRef, useMemo, useCallback, useDeferredValue, memo } from "react";
import { Search } from "lucide-react";
import type { LoggedWorkout } from "../api.ts";
import { type LogFilterType, dominantTypeFromLog, matchesLogFilter, groupByMonth } from "../logbookUtils.ts";
import LogSidebarCard from "./LogSidebarCard.tsx";
import SidebarSkeleton from "../../../components/general/SidebarSkeleton.tsx";

const FILTERS: LogFilterType[] = ["Strength", "Cardio", "Plyometric"];

/** Cards rendered per page; the next page is added when the end of the list comes into view. */
const PAGE = 60;

interface LogSideBarProps {
    logs: LoggedWorkout[];
    draft?: LoggedWorkout | null;
    selectedId: string | null;
    loading: boolean;
    onSelect: (id: string) => void;
}

/**
 * Memoised, with pages of cards: with a few hundred logs, rendering every card
 * on each keystroke, filter click or logbook update made the page stutter.
 */
function LogSideBar({ logs, draft, selectedId, loading, onSelect }: LogSideBarProps) {
    const [activeFilters, setActiveFilters] = useState<Set<LogFilterType>>(new Set());
    const [query, setQuery] = useState("");
    const listRef = useRef<HTMLDivElement>(null);
    const sentinelRef = useRef<HTMLDivElement>(null);

    // The page passes a new handler every render; keep one identity for the memoised cards.
    const latestSelect = useRef(onSelect);
    useLayoutEffect(() => { latestSelect.current = onSelect; });
    const select = useCallback((id: string) => latestSelect.current(id), []);

    function toggleFilter(type: LogFilterType) {
        setActiveFilters(prev => {
            const next = new Set(prev);
            if (next.has(type)) next.delete(type); else next.add(type);
            return next;
        });
    }

    // The input updates straight away; the list follows when the browser has time.
    const deferredQuery = useDeferredValue(query);
    const deferredFilters = useDeferredValue(activeFilters);
    const visible = useMemo(() => {
        const q = deferredQuery.trim().toLowerCase();
        return logs.filter(l =>
            (q === "" || l.name.toLowerCase().includes(q)) &&
            (deferredFilters.size === 0 || [...deferredFilters].some(f => matchesLogFilter(l, f)))
        );
    }, [logs, deferredQuery, deferredFilters]);
    const stale = deferredQuery !== query || deferredFilters !== activeFilters;

    // Paging, reset whenever the result list changes. The selected log is always within the page.
    const [shown, setShown] = useState<{ list: LoggedWorkout[]; count: number }>({ list: visible, count: PAGE });
    const selectedIndex = selectedId ? visible.findIndex(l => l.id === selectedId) : -1;
    const count = Math.max(shown.list === visible ? shown.count : PAGE, selectedIndex + 10);
    const hasMore = count < visible.length;

    // Month headers count every match, not just the rendered page.
    const groups = useMemo(() => groupByMonth(visible, l => l.loggedAt), [visible]);
    const pagedGroups = useMemo(() => {
        let left = count;
        const out: { key: string; label: string; total: number; items: LoggedWorkout[] }[] = [];
        for (const g of groups) {
            if (left <= 0) break;
            out.push({ key: g.key, label: g.label, total: g.items.length, items: g.items.slice(0, left) });
            left -= g.items.length;
        }
        return out;
    }, [groups, count]);

    useEffect(() => {
        const el = sentinelRef.current;
        if (!el || !hasMore) return;
        const io = new IntersectionObserver(entries => {
            if (entries.some(e => e.isIntersecting)) setShown({ list: visible, count: count + PAGE });
        }, { root: listRef.current, rootMargin: "600px 0px" });
        io.observe(el);
        return () => io.disconnect();
    }, [visible, count, hasMore]);

    useEffect(() => {
        if (!selectedId || !listRef.current) return;
        const el = listRef.current.querySelector<HTMLElement>(`[data-id="${selectedId}"]`);
        el?.scrollIntoView({ block: "center", behavior: "smooth" });
    }, [selectedId]);

    return (
        <aside className="feature-sidebar">
            <div className="feature-sidebar-header">
                <div className="feature-sidebar-title-row">
                    <span className="feature-sidebar-title">Your logs</span>
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
            <div className={`feature-sidebar-list${stale ? " is-stale" : ""}`} ref={listRef}>
                {draft && (
                    <LogSidebarCard
                        log={draft}
                        colorClass={dominantTypeFromLog(draft)}
                        isActive={draft.id === selectedId}
                        isDraft
                        onSelect={select}
                    />
                )}
                {loading ? (
                    <SidebarSkeleton />
                ) : visible.length === 0 ? (
                    <p className="feature-sidebar-empty">
                        {logs.length === 0 ? "No logs yet — start your first workout." : "No logs match your search."}
                    </p>
                ) : (
                    pagedGroups.map(g => (
                        <section key={g.key} className="sidebar-month" aria-label={g.label}>
                            <h3 className="sidebar-month-header">
                                <span className="sidebar-month-label">{g.label}</span>
                                <span className="sidebar-month-count">
                                    {g.total} workout{g.total !== 1 ? "s" : ""}
                                </span>
                            </h3>
                            {g.items.map(l => (
                                <LogSidebarCard
                                    key={l.id}
                                    log={l}
                                    colorClass={dominantTypeFromLog(l)}
                                    isActive={l.id === selectedId}
                                    onSelect={select}
                                />
                            ))}
                        </section>
                    ))
                )}
                {hasMore && <div ref={sentinelRef} className="feature-sidebar-more" aria-hidden />}
            </div>
        </aside>
    );
}

export default memo(LogSideBar);
