import { useId, type ReactNode } from "react";
import { Area, CartesianGrid, ComposedChart, Line, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
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
    /** Step line through the highlighted points: "your record so far". Only for progressive PRs. */
    recordLine?: boolean;
    /** Lower is better (e.g. pace): flips the y-axis so "up" still means "improving". */
    invert?: boolean;
    height?: number;
    ariaLabel: string;
}

const DAY = 86_400_000;

/** More sessions than this and per-session markers turn into noise; only records keep a dot. */
const DENSE = 45;

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

/** Month ticks carry the year on the first tick and whenever it changes: "Oct '24  Feb '25  Jun  Oct". */
function formatTick(t: number, prev: number | undefined, spanDays: number): string {
    const d = new Date(t);
    if (spanDays <= 70) return d.toLocaleDateString("en-GB", { day: "numeric", month: "short" });
    const month = d.toLocaleDateString("en-GB", { month: "short" });
    const showYear = spanDays > 300 && (prev === undefined || new Date(prev).getFullYear() !== d.getFullYear());
    return showYear ? `${month} ’${String(d.getFullYear()).slice(2)}` : month;
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

type Row<P> =
    | (P & { best: number | null; bridge?: number; isolated?: boolean; gap?: undefined })
    | { t: number; value: null; best: number | null; bridge: number; gap: true };

/**
 * Chart rows: the sessions, plus a null row in every long break so the line
 * stops there instead of drawing a slope nobody trained through. The break is
 * bridged by a faint dotted line. `best` is the record so far (step line).
 */
function buildRows<P extends TrendPoint>(points: P[], recordLine: boolean): Row<P>[] {
    const intervals = points.slice(1).map((p, i) => p.t - points[i].t).sort((a, b) => a - b);
    const median = intervals.length ? intervals[Math.floor(intervals.length / 2)] : 0;
    const gapMs = Math.max(28 * DAY, 4 * median);

    const rows: Row<P>[] = [];
    let best: number | null = null;
    points.forEach((p, i) => {
        const prev = i > 0 ? points[i - 1] : undefined;
        const afterGap = prev !== undefined && p.t - prev.t > gapMs;
        if (prev && afterGap) {
            rows[rows.length - 1].bridge = prev.value;
            rows.push({ t: (prev.t + p.t) / 2, value: null, best, bridge: (prev.value + p.value) / 2, gap: true });
        }
        if (recordLine && p.highlight) best = p.value;
        rows.push({ ...p, best, bridge: afterGap ? p.value : undefined });
    });
    // A session alone between two breaks draws no line, so it always keeps its marker.
    rows.forEach((r, i) => {
        const breakBefore = i === 0 || rows[i - 1].gap === true;
        const breakAfter = i === rows.length - 1 || rows[i + 1].gap === true;
        if (!r.gap) r.isolated = breakBefore && breakAfter;
    });
    return rows;
}

/**
 * Your sessions over real time: every point on the line is a session you
 * did, at its actual date. Personal records get a larger filled dot; with
 * many sessions only those keep a marker. Long breaks interrupt the line.
 */
function TrendChart<P extends TrendPoint>({
    points, start, end, formatValue, renderTooltip, seriesLabel, highlightLabel, recordLine = false, invert, height = 260, ariaLabel,
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
    const showRecord = recordLine && hasHighlight;
    const dense = points.length > DENSE;
    const rows = buildRows(points, showRecord);
    const xTicks = timeTicks(from, end);

    return (
        <>
            <div className="viz-chart" style={{ height }} role="img" aria-label={ariaLabel}>
                <ResponsiveContainer width="100%" height="100%">
                    <ComposedChart data={rows} margin={{ top: 12, right: 12, bottom: 0, left: 0 }}>
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
                            reversed={invert}
                            tickFormatter={v => formatValue(v)}
                            tick={AXIS_TICK}
                            tickLine={false}
                            axisLine={false}
                            width={68}
                        />
                        <Tooltip
                            cursor={{ stroke: "var(--viz-axis)", strokeDasharray: "3 3" }}
                            content={({ active, payload }) => {
                                const row = payload?.[0]?.payload as Row<P> | undefined;
                                return active && row && !row.gap ? renderTooltip(row as P) : null;
                            }}
                        />
                        <Line
                            type="linear"
                            dataKey="bridge"
                            stroke="var(--viz-1)"
                            strokeOpacity={0.45}
                            strokeWidth={1.5}
                            strokeDasharray="2 4"
                            dot={false}
                            activeDot={false}
                            connectNulls={false}
                            isAnimationActive={false}
                        />
                        {showRecord && (
                            <Line
                                type="stepAfter"
                                dataKey="best"
                                stroke="var(--viz-1)"
                                strokeOpacity={0.55}
                                strokeWidth={1.5}
                                strokeDasharray="5 4"
                                dot={false}
                                activeDot={false}
                                connectNulls
                                isAnimationActive={false}
                            />
                        )}
                        <Area
                            type="linear"
                            dataKey="value"
                            stroke="var(--viz-1)"
                            strokeWidth={dense ? 1.75 : 2}
                            strokeLinejoin="round"
                            connectNulls={false}
                            fill={`url(#${gradientId})`}
                            baseValue={invert ? "dataMax" : "dataMin"}
                            isAnimationActive={false}
                            activeDot={{ r: 5.5, fill: "var(--viz-1)", stroke: "var(--viz-surface)", strokeWidth: 2 }}
                            dot={(props: { cx?: number; cy?: number; index?: number; payload?: Row<P> }) => {
                                const { cx, cy, index, payload } = props;
                                if (cx == null || cy == null || payload == null || payload.gap) return <g key={index} />;
                                if (payload.highlight) {
                                    return <circle key={index} cx={cx} cy={cy} r={dense ? 4 : 5} fill="var(--viz-1)" stroke="var(--viz-surface)" strokeWidth={2} />;
                                }
                                return dense && !payload.isolated
                                    ? <g key={index} />
                                    : <circle key={index} cx={cx} cy={cy} r={3} fill="var(--viz-surface)" stroke="var(--viz-1)" strokeWidth={1.75} />;
                            }}
                        />
                    </ComposedChart>
                </ResponsiveContainer>
            </div>
            <ul className="trend-legend">
                <li><span className={`trend-key trend-key-series${dense ? " plain" : ""}`} aria-hidden />{seriesLabel}</li>
                {hasHighlight && <li><span className="trend-key trend-key-dot filled" aria-hidden />{highlightLabel}</li>}
                {showRecord && <li><span className="trend-key trend-key-record" aria-hidden />Your record so far</li>}
            </ul>
        </>
    );
}

export default TrendChart;
