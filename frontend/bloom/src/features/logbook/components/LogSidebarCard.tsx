import { memo } from "react";
import type { LoggedWorkout } from "../api.ts";
import { relativeDate } from "../logbookUtils.ts";

interface LogSidebarCardProps {
    log: LoggedWorkout;
    colorClass: "strength" | "cardio" | "plyometric" | "mix";
    isActive: boolean;
    isDraft?: boolean;
    /** Receives the log id, so one stable handler serves every card. */
    onSelect: (id: string) => void;
}

const TYPE_LABELS = { strength: "Strength", cardio: "Cardio", plyometric: "Plyo", mix: "Mix" } as const;

function LogSidebarCard({ log, colorClass, isActive, isDraft, onSelect }: LogSidebarCardProps) {
    const exerciseCount = log.exercises.length;
    const setCount = log.exercises.reduce((n, ex) => n + ex.sets.length, 0);

    return (
        <div className={`sidebar-card ${colorClass}${isActive ? " active" : ""}${isDraft ? " draft" : ""}`} data-id={log.id} onClick={() => onSelect(log.id)}>
            <span className={`type-dot ${colorClass}`} data-label={TYPE_LABELS[colorClass]} />
            <p className="sidebar-card-name">{log.name || "Untitled workout"}</p>
            <span className="sidebar-card-meta">
                {isDraft ? <span className="draft-badge">Draft</span> : relativeDate(log.loggedAt)} · {exerciseCount} ex · {setCount} set{setCount !== 1 ? "s" : ""}
            </span>
        </div>
    );
}

export default memo(LogSidebarCard);
