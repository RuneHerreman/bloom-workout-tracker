import { useState } from "react";
import { CartesianGrid, ComposedChart, Line, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { AXIS_TICK, VizTooltip } from "../../insights/components/ChartParts.tsx";
import { formatTick, niceAxis, timeTicks } from "../../insights/components/chartAxis.ts";
import type { SeriesPoint } from "../../insights/bodySeries.ts";

const DAY = 86_400_000;

interface Row { t: number; raw?: number; trend?: number; plan?: number }

const fmt = (n: number) => `${n.toLocaleString("en-GB", { maximumFractionDigits: 1 })} kg`;
const fmtDate = (t: number) => new Date(t).toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" });

/**
 * Your weight trend over the recent past, and the plan carried on from where
 * the trend is today: the two lines should point the same way. Weigh-ins are
 * faint dots, so a single heavy morning doesn't read as a change of course.
 */
function PlanChart({ points, kgPerWeek, weeks }: { points: SeriesPoint[]; kgPerWeek: number; weeks: number }) {
    const [now] = useState(() => Date.now());
    const last = points[points.length - 1];
    const startKg = last.trend;
    const rows: Row[] = [
        ...points.map(p => ({ t: p.t, raw: p.raw, trend: p.trend })),
        { t: now, plan: startKg },
        { t: now + weeks * 7 * DAY, plan: startKg + kgPerWeek * weeks },
    ];
    // Hand the trend over to the plan at its last point, so the lines meet.
    rows[points.length - 1].plan = startKg;

    const start = new Date(points[0].t - 3 * DAY);
    const end = new Date(now + weeks * 7 * DAY);
    const spanDays = (end.getTime() - start.getTime()) / DAY;
    const values = rows.flatMap(r => [r.raw, r.trend, r.plan]).filter((v): v is number => v !== undefined);
    const { domain, ticks } = niceAxis(Math.min(...values), Math.max(...values));
    const xTicks = timeTicks(start, end);

    return (
        <>
            <div className="viz-chart plan-chart" role="img" aria-label={`Weight trend for the last weeks, and the plan from today: ${fmt(startKg)} to ${fmt(startKg + kgPerWeek * weeks)} in ${weeks} weeks`}>
                <ResponsiveContainer width="100%" height="100%">
                    <ComposedChart data={rows} margin={{ top: 10, right: 12, bottom: 0, left: 0 }}>
                        <CartesianGrid vertical={false} stroke="var(--viz-grid)" />
                        <XAxis dataKey="t" type="number" scale="time" domain={[start.getTime(), end.getTime()]} ticks={xTicks}
                            tickFormatter={(t: number, i: number) => formatTick(t, xTicks[i - 1], spanDays)}
                            tick={AXIS_TICK} tickLine={false} axisLine={false} tickMargin={8} />
                        <YAxis domain={domain} ticks={ticks} tickFormatter={v => `${v} kg`} tick={AXIS_TICK} tickLine={false} axisLine={false} width={56} />
                        <Tooltip
                            cursor={{ stroke: "var(--viz-axis)", strokeDasharray: "3 3" }}
                            content={({ active, payload }) => {
                                const r = payload?.[0]?.payload as Row | undefined;
                                if (!active || !r) return null;
                                const rows = [
                                    ...(r.raw !== undefined ? [{ label: "Weigh-in", value: fmt(r.raw) }] : []),
                                    ...(r.trend !== undefined ? [{ label: "Trend", value: fmt(r.trend), color: "var(--viz-1)" }] : []),
                                    ...(r.plan !== undefined && r.trend === undefined ? [{ label: "Plan", value: fmt(r.plan), color: "var(--viz-axis)" }] : []),
                                ];
                                return <VizTooltip title={r.t > now ? `${fmtDate(r.t)} (plan)` : fmtDate(r.t)} rows={rows} />;
                            }}
                        />
                        <Line type="linear" dataKey="raw" stroke="none" isAnimationActive={false} activeDot={false}
                            dot={(p: { cx?: number; cy?: number; index?: number }) => p.cx == null || p.cy == null
                                ? <g key={p.index} />
                                : <circle key={p.index} cx={p.cx} cy={p.cy} r={2.5} fill="var(--viz-1)" fillOpacity={0.28} />} />
                        <Line type="monotone" dataKey="trend" stroke="var(--viz-1)" strokeWidth={2.5} strokeLinecap="round"
                            dot={false} activeDot={{ r: 4.5, fill: "var(--viz-1)", stroke: "var(--viz-surface)", strokeWidth: 2 }} connectNulls={false} isAnimationActive={false} />
                        <Line type="linear" dataKey="plan" stroke="var(--viz-axis)" strokeWidth={2} strokeDasharray="6 5"
                            dot={false} activeDot={false} connectNulls isAnimationActive={false} />
                    </ComposedChart>
                </ResponsiveContainer>
            </div>
            <ul className="trend-legend">
                <li><span className="trend-key trend-key-series plain" aria-hidden />Your trend</li>
                <li><span className="trend-key e1rm-key-dot" aria-hidden />Weigh-ins</li>
                <li><span className="trend-key trend-key-record e1rm-key-record" aria-hidden />The plan</li>
            </ul>
        </>
    );
}

export default PlanChart;
