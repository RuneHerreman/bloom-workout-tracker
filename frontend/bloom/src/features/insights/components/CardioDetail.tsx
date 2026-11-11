import { useState } from "react";
import { useNavigate } from "react-router-dom";
import type { CardioActivity, CardioSession, Range } from "../analysis.ts";
import { formatDuration, formatPace } from "../analysis.ts";
import TrendChart, { type TrendPoint } from "./TrendChart.tsx";
import { VizTooltip } from "./ChartParts.tsx";

type Metric = "distance" | "pace" | "duration";

interface Point extends TrendPoint { session: CardioSession }

const fmtDate = (iso: string) => new Date(iso).toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" });

function CardioDetail({ activity, range }: { activity: CardioActivity; range: Range }) {
    const hasPace = activity.sessions.filter(s => s.pace !== null).length >= 2;
    // Activities like bouldering are logged by time only; untimed ones by distance only.
    const hasDistance = activity.sessions.some(s => s.km > 0);
    const hasDuration = activity.sessions.some(s => s.minutes > 0);
    const [metric, setMetric] = useState<Metric>("distance");
    const fallback: Metric = hasDistance || !hasDuration ? "distance" : "duration";
    const active: Metric =
        (metric === "pace" && !hasPace) || (metric === "distance" && !hasDistance) || (metric === "duration" && !hasDuration) ? fallback : metric;
    const navigate = useNavigate();

    const metrics: { value: Metric; label: string }[] = [
        ...(hasDistance ? [{ value: "distance" as Metric, label: "Distance" }] : []),
        ...(hasPace ? [{ value: "pace" as Metric, label: "Pace" }] : []),
        ...(hasDuration ? [{ value: "duration" as Metric, label: "Duration" }] : []),
    ];

    const sessions = active === "pace" ? activity.sessions.filter(s => s.pace !== null) : activity.sessions;
    const longest = Math.max(...activity.sessions.map(s => s.km));
    const points: Point[] = sessions.map(s => ({
        t: new Date(s.date).getTime(),
        value: active === "distance" ? s.km : active === "pace" ? Math.round(s.pace! * 100) / 100 : s.minutes,
        highlight: active === "distance" ? s.km === longest && s.km > 0 : active === "pace" ? s.pace === activity.fastestPace : false,
        session: s,
    }));
    const format = (v: number) => (active === "distance" ? `${v} km` : active === "pace" ? formatPace(v) : formatDuration(v));

    return (
        <div className="detail">
            <header className="detail-head">
                <div>
                    <h3 className="detail-title">{activity.name}</h3>
                    <p className="detail-sub">
                        {activity.sessions.length} session{activity.sessions.length !== 1 ? "s" : ""}
                        {activity.paceChangeSec !== null && (
                            <> · <span className={activity.paceChangeSec <= 0 ? "text-up" : "text-down"}>
                                {Math.abs(activity.paceChangeSec)} s/km {activity.paceChangeSec <= 0 ? "faster" : "slower"} than earlier
                            </span></>
                        )}
                    </p>
                </div>
                <div className="segmented segmented-sm" role="radiogroup" aria-label="Metric">
                    {metrics.map(m => (
                        <button key={m.value} role="radio" aria-checked={active === m.value}
                            className={`segmented-item${active === m.value ? " active" : ""}`}
                            onClick={() => setMetric(m.value)}>
                            {m.label}
                        </button>
                    ))}
                </div>
            </header>

            <TrendChart
                points={points}
                start={range.start}
                end={range.end}
                invert={active === "pace"}
                formatValue={format}
                ariaLabel={`${activity.name}: ${active} per session`}
                seriesLabel={active === "distance" ? "Distance per session" : active === "pace" ? "Average pace per session" : "Duration per session"}
                highlightLabel={active === "pace" ? "Fastest" : active === "distance" ? "Longest" : ""}
                renderTooltip={p => (
                    <VizTooltip
                        title={fmtDate(p.session.date)}
                        rows={[
                            { label: "Distance", value: `${p.session.km} km`, color: active === "distance" ? "var(--viz-1)" : undefined },
                            { label: "Duration", value: formatDuration(p.session.minutes), color: active === "duration" ? "var(--viz-1)" : undefined },
                            ...(p.session.pace ? [{ label: "Pace", value: formatPace(p.session.pace), color: active === "pace" ? "var(--viz-1)" : undefined }] : []),
                        ]}
                    />
                )}
            />
            {active === "pace" && <p className="detail-note">Faster is higher on this chart.</p>}

            <dl className="records">
                <div><dt>Total distance</dt><dd>{activity.totalKm} km</dd><span>this period</span></div>
                <div><dt>Total time</dt><dd>{formatDuration(activity.totalMinutes)}</dd><span>this period</span></div>
                <div><dt>Longest</dt><dd>{longest} km</dd><span>single session</span></div>
                <div><dt>Fastest pace</dt><dd>{activity.fastestPace ? formatPace(activity.fastestPace) : "—"}</dd><span>{activity.fastestPace ? "best average" : "needs distance + time"}</span></div>
            </dl>

            <div className="history">
                <h4 className="history-title">Recent sessions</h4>
                <table className="history-table">
                    <thead><tr><th>Date</th><th className="num">Distance</th><th className="num">Duration</th><th className="num">Pace</th></tr></thead>
                    <tbody>
                        {[...activity.sessions].reverse().slice(0, 6).map(s => (
                            <tr key={s.logId + s.exerciseId} onClick={() => navigate("/logbook", { state: { selectedLogId: s.logId } })} title="Open this workout">
                                <td>{fmtDate(s.date)}</td>
                                <td className="num">{s.km} km</td>
                                <td className="num">{formatDuration(s.minutes)}</td>
                                <td className="num">{s.pace ? formatPace(s.pace) : "—"}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
}

export default CardioDetail;
