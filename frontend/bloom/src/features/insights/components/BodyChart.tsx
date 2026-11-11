import { CartesianGrid, ComposedChart, Line, ResponsiveContainer, Scatter, Tooltip, XAxis, YAxis } from "recharts";
import { AXIS_TICK, VizTooltip } from "./ChartParts.tsx";
import { formatTick, niceAxis, timeTicks } from "./chartAxis.ts";
import type { SeriesPoint } from "../bodySeries.ts";

const DAY = 86_400_000;

interface BodyChartProps {
    points: SeriesPoint[];
    start: Date;
    end: Date;
    unit: string;
    /** Space kept above and below the data, in the series' unit, so a flat month doesn't look like a cliff. */
    pad: number;
    rawLabel: string;
    trendLabel: string;
    ariaLabel: string;
}

const fmt = (v: number, unit: string) => `${(Math.round(v * 10) / 10).toLocaleString("en-GB")}${unit === "%" ? "%" : ` ${unit}`}`;
const fmtDay = (t: number) => new Date(t).toLocaleDateString("en-GB", { weekday: "short", day: "numeric", month: "short", year: "numeric" });

/** Readings as small grey dots, the trend as the line people read. */
function BodyChart({ points, start, end, unit, pad, rawLabel, trendLabel, ariaLabel }: BodyChartProps) {
    const firstT = points[0]?.t ?? start.getTime();
    const from = new Date(Math.max(start.getTime(), firstT - 3 * DAY));
    const spanDays = (end.getTime() - from.getTime()) / DAY;
    const values = points.flatMap(p => [p.raw, p.trend]);
    const { domain, ticks } = niceAxis(Math.min(...values) - pad, Math.max(...values) + pad);
    const xTicks = timeTicks(from, end);
    // Many readings: smaller, fainter dots so the trend stays the thing you read.
    const dense = points.length > 60;

    return (
        <>
            <div className="viz-chart" style={{ height: 260 }} role="img" aria-label={ariaLabel}>
                <ResponsiveContainer width="100%" height="100%">
                    <ComposedChart data={points} margin={{ top: 12, right: 12, bottom: 0, left: 0 }}>
                        <CartesianGrid vertical={false} stroke="var(--viz-grid)" />
                        <XAxis
                            dataKey="t"
                            type="number"
                            scale="time"
                            domain={[from.getTime(), end.getTime()]}
                            ticks={xTicks}
                            tickFormatter={(t: number, i: number) => formatTick(t, xTicks[i - 1], spanDays)}
                            tick={AXIS_TICK}
                            tickLine={false}
                            axisLine={false}
                            tickMargin={8}
                        />
                        <YAxis
                            domain={domain}
                            ticks={ticks}
                            allowDataOverflow
                            tickFormatter={v => fmt(v, unit)}
                            tick={AXIS_TICK}
                            tickLine={false}
                            axisLine={false}
                            width={60}
                        />
                        <Tooltip
                            cursor={{ stroke: "var(--viz-axis)", strokeDasharray: "3 3" }}
                            content={({ active, payload }) => {
                                const p = payload?.[0]?.payload as SeriesPoint | undefined;
                                return active && p ? (
                                    <VizTooltip
                                        title={fmtDay(p.t)}
                                        rows={[
                                            { label: rawLabel, value: fmt(p.raw, unit), color: "var(--viz-axis)" },
                                            { label: trendLabel, value: fmt(p.trend, unit), color: "var(--viz-1)" },
                                        ]}
                                    />
                                ) : null;
                            }}
                        />
                        <Scatter
                            dataKey="raw"
                            isAnimationActive={false}
                            shape={(props: { cx?: number; cy?: number }) =>
                                props.cx == null || props.cy == null ? <g /> : (
                                    <circle cx={props.cx} cy={props.cy} r={dense ? 2 : 3}
                                        fill="var(--viz-axis)" fillOpacity={dense ? 0.45 : 0.7} />
                                )}
                        />
                        <Line
                            type="monotone"
                            dataKey="trend"
                            stroke="var(--viz-1)"
                            strokeWidth={2}
                            dot={false}
                            activeDot={{ r: 4.5, fill: "var(--viz-1)", stroke: "var(--viz-surface)", strokeWidth: 2 }}
                            isAnimationActive={false}
                        />
                    </ComposedChart>
                </ResponsiveContainer>
            </div>
            <ul className="trend-legend">
                <li><span className="trend-key body-key-raw" aria-hidden />{rawLabel}</li>
                <li><span className="trend-key trend-key-series plain" aria-hidden />{trendLabel}</li>
            </ul>
        </>
    );
}

export default BodyChart;
