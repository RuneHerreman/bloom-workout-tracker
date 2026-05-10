import { NavLink } from "react-router-dom";

export interface LogEntryData {
    id: string;
    name: string;
    date: string;
    exerciseCount: number;
    // New: Array of types, e.g., ["strength", "cardio", "strength"]
    exerciseTypes: string[];
}

function LogEntry({ name, date, exerciseCount, exerciseTypes }: LogEntryData) {
    return (
        <div className="log-entry">
            <div className="log-entry-header">
                <span className="log-entry-name">{name}</span>
                <span className="log-entry-date">{date}</span>
            </div>
            <span className="log-entry-count">{exerciseCount} Exercise{exerciseCount === 1 ? "" : "s"}</span>

            <div className="log-bars" style={{
                display: 'flex',
                gap: '2px',
                height: '6px',
                marginTop: '8px',
                width: '100%'
            }}>
                {exerciseTypes.map((type, index) => (
                    <div
                        key={index}
                        className={`log-bar ${type.toLowerCase()}`}
                        style={{
                            flex: 1, // Each exercise gets equal width
                            borderRadius: '2px',
                            // Fallback colors if CSS classes aren't loaded
                            backgroundColor: type.toLowerCase() === 'cardio' ? '#E9762B' : '#003E1F'
                        }}
                    />
                ))}
            </div>
        </div>
    );
}

interface LogWidgetProps {
    entries?: LogEntryData[];
}

function LogWidget({ entries = [] }: LogWidgetProps) {
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
