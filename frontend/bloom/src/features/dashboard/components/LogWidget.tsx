import { NavLink, useNavigate } from "react-router-dom";
import GeneralWidget from "./GeneralWidget.tsx";
import WidgetHeader from "./WidgetHeader.tsx";

export interface LogEntryData {
    id: string;
    name: string;
    date: string;
    exerciseCount: number;
    exerciseTypes: string[];
}

function LogEntry({ id, name, date, exerciseCount, exerciseTypes }: LogEntryData) {
    const navigate = useNavigate();
    return (
        <div className="log-entry" role="button" tabIndex={0} onClick={() => navigate("/logbook", { state: { selectedLogId: id } })} onKeyDown={e => e.key === "Enter" && navigate("/logbook", { state: { selectedLogId: id } })}>
            <div className="log-entry-header">
                <span className="log-entry-name">{name}</span>
                <span className="log-entry-date">{date}</span>
            </div>
            <span className="log-entry-count">{exerciseCount} Exercise{exerciseCount === 1 ? "" : "s"}</span>

            <div className="log-bars">
                {exerciseTypes.map((type, index) => (
                    <div
                        key={index}
                        className={`log-bar ${type.toLowerCase()}`}
                        style={{
                            flex: 1,
                            borderRadius: '2px',
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
        <GeneralWidget
            header={
                <WidgetHeader
                    title={"Last sessions"}
                    subtitle={"Recent"}
                    action={<NavLink to="/logbook" className="log-widget-view-all">View all →</NavLink>}
                />
            }
            content={
                <div className="log-widget">
                    {entries.map(entry => (
                        <LogEntry key={entry.id} {...entry}/>
                    ))}
                </div>
            }
        />
    );
}

export default LogWidget;
