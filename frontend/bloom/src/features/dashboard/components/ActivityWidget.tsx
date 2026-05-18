// Each cell represents one day. Level 0 = no activity, 1–3 = increasing intensity.
import { useNavigate } from "react-router-dom";
import GeneralWidget from "./GeneralWidget.tsx";
import WidgetHeader from "./WidgetHeader.tsx";

export interface ActivityDay {
    date: string;   // ISO date string
    level: 0 | 1 | 2 | 3;
    logId?: string;
}

interface ActivityWidgetProps {
    data?: ActivityDay[];
}

function generatePlaceholder(): ActivityDay[] {
    const today = new Date();
    const days: ActivityDay[] = [];

    for (let w = 51; w >= 0; w--) {
        for (let d = 6; d >= 0; d--) {
            const date = new Date(today);
            date.setDate(today.getDate() - w * 7 - d);
            const rand = Math.random();
            const level = rand < 0.55 ? 0 : rand < 0.75 ? 1 : rand < 0.9 ? 2 : 3;
            days.push({ date: date.toISOString().slice(0, 10), level: level as 0 | 1 | 2 | 3 });
        }
    }
    return days;
}

function ActivityWidget({ data }: ActivityWidgetProps) {
    const cells = data ?? generatePlaceholder();
    const navigate = useNavigate();

    return (
        <GeneralWidget
            header={
                <WidgetHeader
                    title={"Workouts logged this year"}
                    subtitle={`Year · ${new Date().getFullYear()}`}
                />
            }
            content={
                <div className="activity-grid">
                    {cells.map(cell => (
                        <div
                            key={cell.date}
                            className="activity-cell"
                            data-level={cell.level}
                            title={new Date(cell.date).toLocaleDateString("en-GB", { day: "numeric", month: "long", year: "numeric" })}
                            onClick={() => cell.logId && navigate("/logbook", { state: { selectedLogId: cell.logId } })}
                        />
                    ))}
                </div>
            }
        />
    );
}

export default ActivityWidget;
