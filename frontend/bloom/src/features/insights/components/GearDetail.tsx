import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Area, AreaChart, Bar, BarChart, CartesianGrid, ReferenceLine, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import type { Range } from "../analysis.ts";
import { formatDuration, formatPace } from "../analysis.ts";
import { cumulativeKm, gearByBucket, type GearTotals, type GearUse } from "../gearAnalysis.ts";
import { AXIS_TICK, VizTooltip } from "./ChartParts.tsx";
import { formatTick, niceAxis, timeTicks } from "./chartAxis.ts";
import { SHOE_LIFE_KM, type GearKind } from "../../profile/gearKinds.ts";
import GearIcon from "../../profile/GearIcon.tsx";

type Metric = "km" | "minutes" | "sets" | "volumeKg" | "sessions";

const METRIC_LABEL: Record<Metric, string> = { km: "Distance", minutes: "Time", sets: "Sets", volumeKg: "Volume", sessions: "Sessions" };

const DAY = 86_400_000;
const fmtDate = (iso: string) => new Date(iso).toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" });
const km = (v: number) => `${v >= 100 ? Math.round(v).toLocaleString("en-GB") : (Math.round(v * 10) / 10).toLocaleString("en-GB")} km`;
const kg = (v: number) => (v >= 10_000 ? `${(v / 1000).toLocaleString("en-GB", { maximumFractionDigits: 1 })} t` : `${Math.round(v).toLocaleString("en-GB")} kg`);
const n = (v: number) => Math.round(v).toLocaleString("en-GB");

function format(metric: Metric, v: number) {
    return metric === "km" ? km(v) : metric === "minutes" ? formatDuration(v) : metric === "volumeKg" ? kg(v) : n(v);
}

/** "+12% on the period before", or a word when there's nothing to compare. */
function change(now: number, before: number): { text: string; tone: "up" | "down" | "flat" } | null {
    if (!before) return null;
    const pct = Math.round(((now - before) / before) * 100);
    return { text: `${pct > 0 ? "+" : pct < 0 ? "−" : ""}${Math.abs(pct)}% on the period before`, tone: pct > 0 ? "up" : pct < 0 ? "down" : "flat" };
}

function Tile({ label, value, sub, delta }: { label: string; value: string; sub: string; delta?: ReturnType<typeof change> }) {
    return (
        <div>
            <dt>{label}</dt>
            <dd>{value}</dd>
            <span className={delta && delta.tone !== "flat" ? `text-${delta.tone}` : undefined}>{delta?.text ?? sub}</span>
        </div>
    );
}

/** Which numbers mean something for this item: a belt has no distance, shoes no volume. */
function relevant(t: GearTotals): Metric[] {
    const out: Metric[] = [];
    if (t.km > 0) out.push("km");
    if (t.minutes > 0) out.push("minutes");
    if (t.sets > 0) out.push("sets");
    if (t.volumeKg > 0) out.push("volumeKg");
    out.push("sessions");
    return out;
}

function GearDetail({ gear, kind, range }: { gear: GearUse; kind: GearKind; range: Range }) {
    const navigate = useNavigate();
    const metrics = relevant(gear.total);
    const [picked, setPicked] = useState<Metric>(metrics[0]);
    const metric = metrics.includes(picked) ? picked : metrics[0];
    const p = gear.period, prev = gear.previous, all = gear.total;
    const { unit, points } = gearByBucket(gear.sessions, range);
    const lifetime = all.km > 0 ? cumulativeKm(gear.sessions) : [];
    const wear = kind === "shoe" && all.km > 0 ? all.km / SHOE_LIFE_KM : null;
    const recent = [...gear.sessions].reverse().slice(0, 6);
    const empty = p.sessions === 0;

    const tiles: React.ReactNode[] = [];
    if (all.km > 0) tiles.push(<Tile key="km" label="Distance" value={km(p.km)} sub="this period" delta={change(p.km, prev.km)} />);
    if (all.minutes > 0) tiles.push(<Tile key="time" label="Time" value={formatDuration(p.minutes)} sub="this period" delta={change(p.minutes, prev.minutes)} />);
    if (all.km > 0 && all.minutes > 0) tiles.push(<Tile key="pace" label="Average pace" value={p.pace ? formatPace(p.pace) : "—"} sub={p.pace ? "over sessions with both" : "needs distance and time"} />);
    if (all.km > 0) tiles.push(<Tile key="long" label="Longest" value={p.longestKm ? km(p.longestKm) : "—"} sub="single session" />);
    if (all.sets > 0) tiles.push(<Tile key="sets" label="Sets" value={n(p.sets)} sub={`${n(p.reps)} reps`} delta={change(p.sets, prev.sets)} />);
    if (all.volumeKg > 0) tiles.push(<Tile key="vol" label="Volume" value={kg(p.volumeKg)} sub="weight × reps" delta={change(p.volumeKg, prev.volumeKg)} />);
    tiles.push(<Tile key="sessions" label="Sessions" value={n(p.sessions)} sub={`in ${n(p.workouts)} workout${p.workouts === 1 ? "" : "s"}`} delta={change(p.sessions, prev.sessions)} />);
    tiles.push(<Tile key="life" label="All time" value={all.km > 0 ? km(all.km) : `${n(all.sessions)} sessions`} sub={gear.firstUsed ? `since ${fmtDate(gear.firstUsed)}` : "not used yet"} />);

    const values = lifetime.map(x => x.km);
    const axis = values.length ? niceAxis(0, Math.max(...values, wear !== null ? SHOE_LIFE_KM : 0)) : null;
    const lifeStart = lifetime.length ? new Date(lifetime[0].t - 3 * DAY) : null;
    const lifeEnd = new Date(range.end);
    const lifeTicks = lifeStart ? timeTicks(lifeStart, lifeEnd) : [];
    const lifeSpan = lifeStart ? (lifeEnd.getTime() - lifeStart.getTime()) / DAY : 0;

    return (
        <div className="detail">
            <header className="detail-head">
                <div className="gear-detail-title">
                    <span className="gear-detail-icon"><GearIcon kind={kind} size={18} /></span>
                    <div>
                        <h3 className="detail-title">{gear.name}</h3>
                        <p className="detail-sub">
                            {empty ? "Not used this period" : `${p.sessions} session${p.sessions === 1 ? "" : "s"} this period`}
                            {gear.lastUsed && <> · last used {fmtDate(gear.lastUsed)}</>}
                            {!gear.listed && <> · no longer in your gear</>}
                        </p>
                    </div>
                </div>
                {!empty && metrics.length > 1 && (
                    <div className="segmented segmented-sm" role="radiogroup" aria-label="Metric">
                        {metrics.map(m => (
                            <button key={m} role="radio" aria-checked={metric === m}
                                className={`segmented-item${metric === m ? " active" : ""}`} onClick={() => setPicked(m)}>
                                {METRIC_LABEL[m]}
                            </button>
                        ))}
                    </div>
                )}
            </header>

            {wear !== null && (
                <div className={`gear-detail-wear${wear >= 1 ? " worn" : wear >= 0.8 ? " due" : ""}`}>
                    <div className="gear-detail-wear-head">
                        <span>{wear >= 1 ? "Past a usual shoe life" : wear >= 0.8 ? "Nearly due for new ones" : "Shoe life"}</span>
                        <b>{km(all.km)} <small>of about {SHOE_LIFE_KM} km</small></b>
                    </div>
                    <span className="gear-wear-track"><span style={{ width: `${Math.min(1, wear) * 100}%` }} /></span>
                </div>
            )}

            {!empty && (
                <div className="viz-chart gear-chart" role="img" aria-label={`${gear.name}: ${METRIC_LABEL[metric].toLowerCase()} per ${unit}`}>
                    <ResponsiveContainer width="100%" height="100%">
                        <BarChart data={points} margin={{ top: 8, right: 8, bottom: 0, left: -8 }}>
                            <CartesianGrid vertical={false} stroke="var(--viz-grid)" />
                            <XAxis dataKey="label" tick={AXIS_TICK} tickLine={false} axisLine={false} interval="preserveStartEnd" minTickGap={12} />
                            <YAxis tick={AXIS_TICK} tickLine={false} axisLine={false} width={56} allowDecimals={false}
                                tickFormatter={v => (metric === "minutes" ? formatDuration(v) : metric === "km" ? `${v} km` : metric === "volumeKg" ? kg(v) : String(v))} />
                            <Tooltip
                                cursor={{ fill: "var(--viz-cursor)" }}
                                content={({ active, payload }) => {
                                    if (!active || !payload?.length) return null;
                                    const b = payload[0].payload as typeof points[number];
                                    return <VizTooltip title={b.longLabel} rows={[
                                        { label: METRIC_LABEL[metric], value: format(metric, b[metric]), color: "var(--viz-1)" },
                                        ...(metric !== "sessions" ? [{ label: "Sessions", value: b.sessions }] : []),
                                    ]} />;
                                }}
                            />
                            <Bar dataKey={metric} fill="var(--viz-1)" radius={[4, 4, 0, 0]} maxBarSize={28} />
                        </BarChart>
                    </ResponsiveContainer>
                </div>
            )}

            <dl className="records gear-records">
                {tiles}
            </dl>

            {lifetime.length >= 2 && axis && lifeStart && (
                <div className="gear-lifetime">
                    <h4 className="history-title">Distance over its life</h4>
                    <div className="viz-chart gear-life-chart" role="img" aria-label={`${gear.name}: total distance over time, ${km(all.km)} now`}>
                        <ResponsiveContainer width="100%" height="100%">
                            <AreaChart data={lifetime} margin={{ top: 8, right: 12, bottom: 0, left: -8 }}>
                                <CartesianGrid vertical={false} stroke="var(--viz-grid)" />
                                <XAxis dataKey="t" type="number" scale="time" domain={[lifeStart.getTime(), lifeEnd.getTime()]} ticks={lifeTicks}
                                    tickFormatter={(t: number, i: number) => formatTick(t, lifeTicks[i - 1], lifeSpan)}
                                    tick={AXIS_TICK} tickLine={false} axisLine={false} tickMargin={8} />
                                <YAxis domain={axis.domain} ticks={axis.ticks} tickFormatter={v => `${v} km`} tick={AXIS_TICK} tickLine={false} axisLine={false} width={56} />
                                {wear !== null && <ReferenceLine y={SHOE_LIFE_KM} stroke="var(--viz-axis)" strokeDasharray="5 4" />}
                                <Tooltip
                                    cursor={{ stroke: "var(--viz-axis)", strokeDasharray: "3 3" }}
                                    content={({ active, payload }) => {
                                        const r = payload?.[0]?.payload as typeof lifetime[number] | undefined;
                                        return active && r ? <VizTooltip title={fmtDate(r.date)} rows={[
                                            { label: "Total", value: km(r.km), color: "var(--viz-1)" },
                                            { label: "This session", value: km(r.added) },
                                        ]} /> : null;
                                    }}
                                />
                                <Area type="stepAfter" dataKey="km" stroke="var(--viz-1)" strokeWidth={2} fill="var(--viz-1)" fillOpacity={0.12} isAnimationActive={false} />
                            </AreaChart>
                        </ResponsiveContainer>
                    </div>
                </div>
            )}

            <div className="gear-detail-lists">
                {gear.usedFor.length > 0 && (
                    <div className="history">
                        <h4 className="history-title">Used for</h4>
                        <ul className="gear-used-for">
                            {gear.usedFor.slice(0, 6).map(u => (
                                <li key={u.exerciseId}>
                                    <span>{u.name}</span>
                                    <span className="gear-used-bar"><span style={{ width: `${(u.sessions / gear.usedFor[0].sessions) * 100}%` }} /></span>
                                    <b>{u.sessions}</b>
                                </li>
                            ))}
                        </ul>
                    </div>
                )}
                {recent.length > 0 && (
                    <div className="history">
                        <h4 className="history-title">Recent sessions</h4>
                        <table className="history-table">
                            <thead>
                                <tr>
                                    <th>Date</th>
                                    {all.km > 0 && <th className="num">Distance</th>}
                                    {all.minutes > 0 && <th className="num">Time</th>}
                                    {all.sets > 0 && <th className="num">Sets</th>}
                                    {all.volumeKg > 0 && <th className="num">Volume</th>}
                                </tr>
                            </thead>
                            <tbody>
                                {recent.map(s => (
                                    <tr key={s.logId + s.exerciseId} onClick={() => navigate("/logbook", { state: { selectedLogId: s.logId } })} title="Open this workout">
                                        <td>{fmtDate(s.date)}</td>
                                        {all.km > 0 && <td className="num">{s.km ? km(s.km) : "—"}</td>}
                                        {all.minutes > 0 && <td className="num">{s.minutes ? formatDuration(s.minutes) : "—"}</td>}
                                        {all.sets > 0 && <td className="num">{s.sets || "—"}</td>}
                                        {all.volumeKg > 0 && <td className="num">{s.volumeKg ? kg(s.volumeKg) : "—"}</td>}
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>
        </div>
    );
}

export default GearDetail;
