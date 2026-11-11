import { useLayoutEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";
import GeneralWidget from "./GeneralWidget.tsx";
import WidgetHeader from "./WidgetHeader.tsx";
import type { ActivityData, ActivityKind } from "../transforms.ts";

const KIND_LABEL: Record<ActivityKind, string> = {
    none: "Rest day", future: "", strength: "Strength", cardio: "Cardio", both: "Strength and cardio",
};

const DAY_LABELS = ["Mon", "", "Wed", "", "Fri", "", ""];

/** A year of training, one square per day, weeks as columns (Monday on top). */
function ActivityWidget({ data }: { data: ActivityData }) {
    const navigate = useNavigate();
    const scrollRef = useRef<HTMLDivElement>(null);

    // On narrow screens the map scrolls sideways: start at the most recent weeks.
    useLayoutEffect(() => {
        const el = scrollRef.current;
        if (el) el.scrollLeft = el.scrollWidth;
    }, []);

    return (
        <GeneralWidget
            className="activity-widget"
            header={
                <WidgetHeader
                    title="Last 12 months"
                    subtitle={`${data.total} workouts · ${data.perWeek} a week`}
                    action={
                        <ul className="activity-legend" aria-label="Legend">
                            <li><span className="activity-cell" data-kind="strength" />Strength</li>
                            <li><span className="activity-cell" data-kind="cardio" />Cardio</li>
                            <li><span className="activity-cell" data-kind="both" />Both</li>
                        </ul>
                    }
                />
            }
            content={
                <div className="activity-scroll" ref={scrollRef}>
                    <div className="activity-map">
                        {data.months.map(m => (
                            <span key={m.column} className="activity-month" style={{ gridColumn: m.column + 2 }}>{m.label}</span>
                        ))}
                        {DAY_LABELS.map((d, r) => (
                            <span key={r} className="activity-day" style={{ gridRow: r + 2 }}>{d}</span>
                        ))}
                        {data.weeks.map((week, c) => week.map((day, r) => {
                            const clickable = !!day.logId;
                            const label = day.kind === "future" ? undefined : `${new Date(day.date).toLocaleDateString("en-GB", { weekday: "short", day: "numeric", month: "short", year: "numeric" })}: ${day.names.length ? day.names.join(", ") : KIND_LABEL[day.kind]}`;
                            return (
                                <span
                                    key={day.date}
                                    className={`activity-cell${clickable ? " clickable" : ""}`}
                                    data-kind={day.kind}
                                    style={{ gridColumn: c + 2, gridRow: r + 2 }}
                                    title={label}
                                    onClick={clickable ? () => navigate("/logbook", { state: { selectedLogId: day.logId } }) : undefined}
                                />
                            );
                        }))}
                    </div>
                </div>
            }
        />
    );
}

export default ActivityWidget;
