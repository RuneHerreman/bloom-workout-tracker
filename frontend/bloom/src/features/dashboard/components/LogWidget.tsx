import { NavLink } from "react-router-dom";

export interface LogEntryData {
    id: string;
    name: string;
    date: string;
    exerciseCount: number;
    cardioRatio: number;    // 0–1 fraction of cardio vs strength sets
}

const PLACEHOLDER: LogEntryData[] = [
    { id: "1", name: "Push",  date: "Tuesday 14 January, 2026",    exerciseCount: 7, cardioRatio: 0.14 },
    { id: "2", name: "Pull",  date: "Sunday 12 January, 2026",     exerciseCount: 6, cardioRatio: 0 },
    { id: "3", name: "Legs",  date: "Friday 10 January, 2026",     exerciseCount: 8, cardioRatio: 0.12 },
    { id: "4", name: "Push",  date: "Wednesday 8 January, 2026",   exerciseCount: 7, cardioRatio: 0.14 },
    { id: "5", name: "Cardio",date: "Monday 6 January, 2026",      exerciseCount: 3, cardioRatio: 1 },
];

function LogEntry({ name, date, exerciseCount, cardioRatio }: LogEntryData) {
    const strengthRatio = 1 - cardioRatio;

    return (
        <div className="log-entry">
            <div className="log-entry-header">
                <span className="log-entry-name">{name}</span>
                <span className="log-entry-date">{date}</span>
            </div>
            <span className="log-entry-count">{exerciseCount} Exercises</span>
            <div className="log-bars">
                {cardioRatio > 0 && (
                    <div className="log-bar cardio" style={{ width: `${cardioRatio * 100}%` }}/>
                )}
                {strengthRatio > 0 && (
                    <div className="log-bar strength" style={{ width: `${strengthRatio * 100}%` }}/>
                )}
            </div>
        </div>
    );
}

interface LogWidgetProps {
    entries?: LogEntryData[];
}

function LogWidget({ entries = PLACEHOLDER }: LogWidgetProps) {
    return (
        <div className="widget log-widget">
            <div className="log-widget-header">
                <p className="widget-title" style={{ margin: 0 }}>Recent Logs</p>
                <NavLink to="/logbook" className="log-widget-view-all">View All</NavLink>
            </div>
            {entries.map(entry => (
                <LogEntry key={entry.id} {...entry}/>
            ))}
        </div>
    );
}

export default LogWidget;
