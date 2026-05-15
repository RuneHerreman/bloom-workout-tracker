import type { LoggedWorkout } from "../api.ts";
import { relativeDate } from "../logbookUtils.ts";

interface LogSidebarCardProps {
    log: LoggedWorkout;
    colorClass: "strength" | "cardio" | "plyometric" | "mix";
    isActive: boolean;
    onSelect: () => void;
}

const TYPE_LABELS = { strength: "Strength", cardio: "Cardio", plyometric: "Plyo", mix: "Mix" } as const;

function LogSidebarCard({ log, colorClass, isActive, onSelect }: LogSidebarCardProps) {
    const exerciseCount = log.exercises.length;
    const setCount = log.exercises.reduce((n, ex) => n + ex.sets.length, 0);

    return (
        <div className={`sidebar-card ${colorClass}${isActive ? " active" : ""}`} onClick={onSelect}>
            <span className={`type-dot ${colorClass}`} data-label={TYPE_LABELS[colorClass]} />
            <p className="sidebar-card-name">{log.name}</p>
            <span className="sidebar-card-meta">
                {relativeDate(log.loggedAt)} · {exerciseCount} ex · {setCount} set{setCount !== 1 ? "s" : ""}
            </span>
        </div>
    );
}

export default LogSidebarCard;