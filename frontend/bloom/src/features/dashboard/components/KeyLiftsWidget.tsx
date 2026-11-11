import { NavLink, useNavigate } from "react-router-dom";
import { Line, LineChart, ReferenceLine, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import GeneralWidget from "./GeneralWidget.tsx";
import WidgetHeader from "./WidgetHeader.tsx";
import { VizTooltip } from "../../insights/components/ChartParts.tsx";
import type { KeyLift, LiftPoint } from "../transforms.ts";

const fmtKg = (v: number) => `${Math.round(v * 10) / 10} kg`;
const fmtDay = (d: string | number) => new Date(d).toLocaleDateString("en-GB", { day: "numeric", month: "short" });
const fmtMonth = (d: string) => new Date(d).toLocaleDateString("en-GB", { month: "short", year: "numeric" });

/** Heaviest set per session over six months; the record shows as a dashed line when it's in reach. */
function LiftChart({ lift }: { lift: KeyLift }) {
    const values = lift.points.map(p => p.value);
    const lo = Math.min(...values), recentHi = Math.max(...values);
    // A record far above recent training would flatten the line; then it stays in the record column only.
    const showRecord = lift.record.weight <= recentHi * 1.25;
    const hi = showRecord ? Math.max(lift.record.weight, recentHi) : recentHi;
    const pad = Math.max(1, (hi - lo) * 0.12);
    return (
        <div className="lift-chart">
            <ResponsiveContainer width="100%" height="100%">
                <LineChart data={lift.points} margin={{ top: 4, right: 4, bottom: 4, left: 4 }}>
                    <XAxis dataKey="t" type="number" scale="time" domain={["dataMin", "dataMax"]} hide />
                    <YAxis domain={[Math.max(0, lo - pad), hi + pad]} hide />
                    {showRecord && <ReferenceLine y={lift.record.weight} stroke="var(--viz-1)" strokeOpacity={0.5} strokeDasharray="3 3" />}
                    <Tooltip
                        cursor={{ stroke: "var(--viz-axis)", strokeDasharray: "3 3" }}
                        wrapperStyle={{ zIndex: 5 }}
                        content={({ active, payload }) => {
                            const p = payload?.[0]?.payload as LiftPoint | undefined;
                            return active && p ? (
                                <VizTooltip
                                    title={fmtDay(p.t)}
                                    rows={[
                                        { label: "Heaviest set", value: `${fmtKg(p.value)} × ${p.reps}`, color: "var(--viz-1)" },
                                        ...(p.record ? [{ label: "New record", value: "★" }] : []),
                                    ]}
                                />
                            ) : null;
                        }}
                    />
                    <Line
                        type="linear"
                        dataKey="value"
                        stroke="var(--viz-1)"
                        strokeWidth={1.5}
                        strokeLinejoin="round"
                        isAnimationActive={false}
                        activeDot={{ r: 3.5, fill: "var(--viz-1)", stroke: "var(--viz-surface)", strokeWidth: 1.5 }}
                        dot={(props: { cx?: number; cy?: number; index?: number; payload?: LiftPoint }) =>
                            props.payload?.record && props.cx != null && props.cy != null
                                ? <circle key={props.index} cx={props.cx} cy={props.cy} r={2.5} fill="var(--viz-1)" />
                                : <g key={props.index} />}
                    />
                </LineChart>
            </ResponsiveContainer>
        </div>
    );
}

/** One row per lift: name, six-month line, last session, record. */
function KeyLiftsWidget({ lifts }: { lifts: KeyLift[] }) {
    const navigate = useNavigate();
    const open = (id: string) => navigate("/insights", { state: { exerciseId: id } });

    return (
        <GeneralWidget
            className="key-lifts-widget"
            header={
                <WidgetHeader
                    title="Key lifts"
                    subtitle="Your most-trained lifts, heaviest set per session over 6 months"
                    action={<NavLink to="/insights" className="widget-link">All exercises →</NavLink>}
                />
            }
            content={
                lifts.length === 0
                    ? <p className="widget-empty">Log a lift a couple of times and its progress shows up here.</p>
                    : (
                        <div className="lift-table" role="table" aria-label="Key lifts">
                            <div className="lift-row lift-row-head" role="row">
                                <span role="columnheader">Lift</span>
                                <span role="columnheader">Last 6 months</span>
                                <span role="columnheader" className="num">Last session</span>
                                <span role="columnheader" className="num">Record</span>
                            </div>
                            {lifts.map(l => (
                                <div
                                    key={l.id}
                                    className="lift-row"
                                    role="row"
                                    tabIndex={0}
                                    onClick={() => open(l.id)}
                                    onKeyDown={e => e.key === "Enter" && open(l.id)}
                                >
                                    <span role="cell" className="lift-name-cell">
                                        <span className="lift-name">{l.name}</span>
                                        <span className="lift-sub">{l.sessions} sessions</span>
                                    </span>
                                    <span role="cell"><LiftChart lift={l} /></span>
                                    <span role="cell" className="num">
                                        <span className="lift-strong">{fmtKg(l.last.weight)} <span className="lift-sub">× {l.last.reps}</span></span>
                                        <span className="lift-sub">{fmtDay(l.last.date)}</span>
                                    </span>
                                    <span role="cell" className="num">
                                        <span className="lift-strong">
                                            {l.lastWasRecord && <span className="lift-star" title="Set in your last session">★ </span>}
                                            {fmtKg(l.record.weight)}
                                        </span>
                                        <span className="lift-sub">{fmtMonth(l.record.date)}</span>
                                    </span>
                                </div>
                            ))}
                        </div>
                    )
            }
        />
    );
}

export default KeyLiftsWidget;
