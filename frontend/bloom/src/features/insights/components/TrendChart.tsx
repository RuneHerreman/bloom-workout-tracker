import { useId, type ReactNode } from "react";
import { Area, AreaChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { AXIS_TICK } from "./ChartParts.tsx";

export interface TrendPoint {
    /** Timestamp (ms). */
    t: number;
    value: number;
    highlight?: boolean;
}

interface TrendChartProps<P extends TrendPoint> {
    points: P[];
    start: Date;
    end: Date;
    formatValue: (v: number) => string;
    renderTooltip: (p: P) => ReactNode;
    /** What a point is, for the legend (e.g. "Heaviest set per session"). */
    seriesLabel: string;
    /** Legend label for highlighted (larger) dots, e.g. "New PR". */
    highlightLabel: string;
    /** Lower is better (e.g. pace): flips the y-axis so "up" still means "improving". */
    invert?: boolean;
    height?: number;
    ariaLabel: string;
}

const DAY = 86_400_000;

/** ~6 evenly spaced ticks on week or month boundaries. */
function timeTicks(start: Date, end: Date): number[] {
    const span = end.getTime() - start.getTime();
    const ticks: number[] = [];
    if (span <= 70 * DAY) {
        const step = span <= 35 * DAY ? 7 : 14;
        for (let t = start.getTime(); t <= end.getTime(); t += step * DAY) ticks.push(t);
    } else {
        const months = Math.max(1, Math.round(span / (30.44 * DAY) / 6));
        for (let d = new Date(start.getFullYear(), start.getMonth() + 1, 1); d <= end; d = new Date(d.getFullYear(), d.getMonth() + months, 1)) {
            ticks.push(d.getTime());
        }
    }
    return ticks;
}

function formatTick(t: number, spanDays: number): string {
    const d = new Date(t);
    if (spanDays <= 70) return d.toLocaleDateString("en-GB", { day: "numeric", month: "short" });
    return d.getMonth() === 0
        ? d.toLocaleDateString("en-GB", { month: "short", year: "numeric" })
        : d.toLocaleDateString("en-GB", { month: "short" });
}

/** Rounds the value range out to 1/2/2.5/5 × 10ⁿ steps so ticks read 60, 80, 100… */
function niceAxis(min: number, max: number, count = 4): { domain: [number, number]; ticks: number[] } {
    const raw = Math.max((max - min) / count, 1e-6);
    const mag = 10 ** Math.floor(Math.log10(raw));
    const step = [1, 2, 2.5, 5, 10].map(m => m * mag).find(s => s >= raw) ?? 10 * mag;
    const lo = Math.max(0, Math.floor(min / step) * step);
    const hi = Math.ceil(max / step) * step;
    const ticks: number[] = [];
    for (let v = lo; v <= hi + step / 2; v += step) ticks.push(Math.round(v * 100) / 100);
    return { domain: [lo, hi], ticks };
}

/**
 * Your sessions over real time: every point on the line is a session you
 * did, at its actual date. Personal records get a larger filled dot.
 */
function TrendChart<P extends TrendPoint>({
    points, start, end, formatValue, renderTooltip, seriesLabel, highlightLabel, invert, height = 260, ariaLabel,
}: TrendChartProps<P>) {
    const gradientId = useId().replace(/:/g, "");
    // Start just before the first session rather than at an empty period start;
    // keep the real end so a recent break still shows as a gap.
    const firstT = points.length ? points[0].t : start.getTime();
    const from = new Date(Math.max(start.getTime(), firstT - 14 * DAY));
    const spanDays = (end.getTime() - from.getTime()) / DAY;
    const values = points.map(p => p.value);
    const { domain, ticks } = niceAxis(Math.min(...values), Math.max(...values));
    const hasHighlight = points.some(p => p.highlight);

    return (
        <>
            <div className="viz-chart" style={{ height }} role="img" aria-label={ariaLabel}>
                <ResponsiveContainer width="100%" height="100%">
                    <AreaChart data={points} margin={{ top: 12, right: 12, bottom: 0, left: 0 }}>
                        <defs>
                            <linearGradient id={gradientId} x1="0" y1={invert ? "1" : "0"} x2="0" y2={invert ? "0" : "1"}>
                                <stop offset="0%" stopColor="var(--viz-1)" stopOpacity={0.16} />
                                <stop offset="100%" stopColor="var(--viz-1)" stopOpacity={0} />
                            </linearGradient>
                        </defs>
                        <CartesianGrid vertical={false} stroke="var(--viz-grid)" />
                        <XAxis
                            dataKey="t"
                            type="number"
                            scale="time"
                            domain={[from.getTime(), end.getTime()]}
                            ticks={timeTicks(from, end)}
                            tickFormatter={t => formatTick(t, spanDays)}
                            tick={AXIS_TICK}
                            tickLine={false}
                            axisLine={false}
                            tickMargin={8}
                        />
                        <YAxis
                            domain={domain}
                            ticks={ticks}
                            reversed={invert}
                            tickFormatter={v => formatValue(v)}
                            tick={AXIS_TICK}
                            tickLine={false}
                            axisLine={false}
                            width={68}
                        />
                        <Tooltip
                            cursor={{ stroke: "var(--viz-axis)", strokeDasharray: "3 3" }}
                            content={({ active, payload }) => (active && payload?.length ? renderTooltip(payload[0].payload as P) : null)}
                        />
                        <Area
                            type="linear"
                            dataKey="value"
                            stroke="var(--viz-1)"
                            strokeWidth={2}
                            strokeLinejoin="round"
                            fill={`url(#${gradientId})`}
                            baseValue={invert ? "dataMax" : "dataMin"}
                            isAnimationActive={false}
                            activeDot={{ r: 5.5, fill: "var(--viz-1)", stroke: "var(--viz-surface)", strokeWidth: 2 }}
                            dot={(props: { cx?: number; cy?: number; index?: number; payload?: P }) => {
                                const { cx, cy, index, payload } = props;
                                if (cx == null || cy == null) return <g key={index} />;
                                return payload?.highlight
                                    ? <circle key={index} cx={cx} cy={cy} r={5} fill="var(--viz-1)" stroke="var(--viz-surface)" strokeWidth={2} />
                                    : <circle key={index} cx={cx} cy={cy} r={3} fill="var(--viz-surface)" stroke="var(--viz-1)" strokeWidth={1.75} />;
                            }}
                        />
                    </AreaChart>
                </ResponsiveContainer>
            </div>
            <ul className="trend-legend">
                <li><span className="trend-key trend-key-series" aria-hidden />{seriesLabel}</li>
                {hasHighlight && <li><span className="trend-key trend-key-dot filled" aria-hidden />{highlightLabel}</li>}
            </ul>
        </>
    );
}

export default TrendChart;
