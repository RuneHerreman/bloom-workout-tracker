import { useState } from "react";
import type { WorkoutTemplate } from "../api.ts";
import { type FilterType, dominantType, matchesFilter } from "../templateUtils.ts";
import TemplateSidebarCard from "./TemplateSidebarCard.tsx";

const FILTERS: FilterType[] = ["Strength", "Cardio", "Plyometric"];

interface TemplateSideBarProps {
    templates: WorkoutTemplate[];
    selectedId: string | null;
    loading: boolean;
    onSelect: (id: string) => void;
    onDelete: (id: string) => void;
}

function TemplateSideBar({ templates, selectedId, loading, onSelect }: TemplateSideBarProps) {
    const [activeFilters, setActiveFilters] = useState<Set<FilterType>>(new Set());

    const toggleFilter = (type: FilterType) => {
        setActiveFilters(prev => {
            const next = new Set(prev);
            if (next.has(type)) next.delete(type); else next.add(type);
            return next;
        });
    };

    const visible = activeFilters.size === 0
        ? templates
        : templates.filter(t => [...activeFilters].some(f => matchesFilter(t, f)));

    return (
        <aside className="template-sidebar">
            <div className="template-sidebar-header">
                <div className="template-sidebar-title-row">
                    <span className="template-sidebar-title">Your templates</span>
                    {!loading && (<span className="template-sidebar-count">{templates.length}</span>)}
                </div>
                <div className="template-filter-chips">
                    {FILTERS.map(f => (
                        <button
                            key={f}
                            className={`template-filter-chip ${f.toLowerCase()}${activeFilters.has(f) ? " active" : ""}`}
                            onClick={() => toggleFilter(f)}
                        >
                            {f}
                        </button>
                    ))}
                </div>
            </div>
            <div className="template-sidebar-list">
                {loading ? (
                    <p className="template-sidebar-loading">Loading…</p>
                ) : visible.length === 0 ? (
                    <p className="template-sidebar-empty">No templates found</p>
                ) : (
                    visible.map(t => (
                        <TemplateSidebarCard
                            key={t.id}
                            template={t}
                            colorClass={dominantType(t)}
                            isActive={t.id === selectedId}
                            onSelect={() => onSelect(t.id)}
                        />
                    ))
                )}
            </div>
        </aside>
    );
}

export default TemplateSideBar;