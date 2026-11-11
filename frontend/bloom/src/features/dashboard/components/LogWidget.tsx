import { NavLink, useNavigate } from "react-router-dom";
import GeneralWidget from "./GeneralWidget.tsx";
import WidgetHeader from "./WidgetHeader.tsx";
import { formatWorkoutDate } from "../../../utils/workoutUtils.ts";
import type { LogEntryData } from "../transforms.ts";

/** Recent sessions; the bar strip has one segment per exercise, coloured by type. */
function LogWidget({ entries = [] }: { entries?: LogEntryData[] }) {
    const navigate = useNavigate();
    const open = (id: string) => navigate("/logbook", { state: { selectedLogId: id } });

    return (
        <GeneralWidget
            className="log-widget-card"
            header={
                <WidgetHeader
                    title="Recent workouts"
                    action={<NavLink to="/logbook" className="widget-link">View all →</NavLink>}
                />
            }
            content={
                entries.length === 0
                    ? <p className="widget-empty">Your logged workouts show up here.</p>
                    : (
                        <ul className="log-list">
                            {entries.map(e => (
                                <li key={e.id}>
                                    <button type="button" className="log-row" onClick={() => open(e.id)}>
                                        <span className="log-row-top">
                                            <span className="log-row-name">{e.name}</span>
                                            <span className="log-row-date">{formatWorkoutDate(new Date(e.date))}</span>
                                        </span>
                                        <span className="log-row-meta">
                                            {e.exerciseTypes.length} exercise{e.exerciseTypes.length === 1 ? "" : "s"}
                                        </span>
                                        <span className="log-bars" aria-hidden>
                                            {e.exerciseTypes.map((type, i) => <span key={i} className={`log-bar ${type}`} />)}
                                        </span>
                                    </button>
                                </li>
                            ))}
                        </ul>
                    )
            }
        />
    );
}

export default LogWidget;
