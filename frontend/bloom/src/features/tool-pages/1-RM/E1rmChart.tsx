import { useMemo, useState } from "react";
import { CartesianGrid, ComposedChart, Line, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { AXIS_TICK, VizTooltip } from "../../insights/components/ChartParts.tsx";
import { formatTick, niceAxis, timeTicks } from "../../insights/components/chartAxis.ts";
import { round1 } from "../../../utils/bodyMath.ts";
import type { BestSet } from "./liftHistory.ts";

export const RANGES = [
    { key: "3m", label: "3m", days: 91 },
    { key: "6m", label: "6m", days: 182 },
    { key: "1y", label: "1y", days: 365 },
    { key: "all", label: "All", days: Infinity },
] as const;
export type RangeKey = (typeof RANGES)[number]["key"];

const DAY = 86_400_000;
/** How quickly the trend follows new sessions: low enough that one light day barely moves it. */
const ALPHA = 0.3;

interface Row { t: number; value: number; trend: number; best: number; record: boolean; set: BestSet }

const fmtDate = (d: string) => new Date(d).toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" });
const fmtSet = (s: BestSet) => `${round1(s.weightKg)} kg × ${s.reps}${s.rir ? ` @ ${s.rir} RIR` : ""}`;

/**
 * Estimated 1RM per session, read for its direction rather than its noise: each
 * session is a faint dot, the smoothed trend and your record so far carry the line.
 * Smoothing and records run over all sessions, so a short range starts warm.
 */
function E1rmChart({ sessions, range, name }: { sessions: BestSet[]; range: RangeKey; name: string }) {
    const [now] = useState(() => Date.now());
    const rows = useMemo(() => {
        const all: Row[] = [];
        let trend = 0, best = 0;
        for (let i = 0; i < sessions.length; i++) {
            const s = sessions[i];
            trend = i === 0 ? s.e1rm : trend + ALPHA * (s.e1rm - trend);
            const record = s.e1rm > best + 0.05;
            if (record) best = s.e1rm;
            all.push({ t: new Date(s.date).getTime(), value: round1(s.e1rm), trend: round1(trend), best: round1(best), record, set: s });
        }
        const days = RANGES.find(r => r.key === range)!.days;
        const from = now - days * DAY;
        return all.filter(r => r.t >= from);
    }, [sessions, range, now]);

    if (rows.length < 2) {
        return <p className="tool-muted e1rm-empty">Fewer than two sessions in this range. Pick a longer one.</p>;
    }

    const start = new Date(rows[0].t - 7 * DAY);
    const end = new Date(now);
    const spanDays = (end.getTime() - start.getTime()) / DAY;
    const values = rows.flatMap(r => [r.value, r.best]);
    const { domain, ticks } = niceAxis(Math.min(...values), Math.max(...values));
    const xTicks = timeTicks(start, end);

    return (
        <>
            <div className="viz-chart e1rm-chart" role="img" aria-label={`${name}: estimated 1RM per session, with trend and record`}>
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
                                return active && r ? (
                                    <VizTooltip title={fmtDate(r.set.date)} rows={[
                                        { label: "Session best", value: `${r.value} kg`, color: "var(--viz-1)" },
                                        { label: "From set", value: fmtSet(r.set) },
                                        { label: "Trend", value: `${r.trend} kg` },
                                        ...(r.record ? [{ label: "New record", value: "★" }] : []),
                                    ]} />
                                ) : null;
                            }}
                        />
                        <Line type="stepAfter" dataKey="best" stroke="var(--viz-axis)" strokeWidth={1.5} strokeDasharray="5 4"
                            dot={false} activeDot={false} isAnimationActive={false} />
                        <Line type="linear" dataKey="value" stroke="none" isAnimationActive={false} activeDot={false}
                            dot={(p: { cx?: number; cy?: number; index?: number; payload?: Row }) => {
                                if (p.cx == null || p.cy == null || !p.payload) return <g key={p.index} />;
                                return p.payload.record
                                    ? <circle key={p.index} cx={p.cx} cy={p.cy} r={4.5} fill="var(--viz-1)" stroke="var(--viz-surface)" strokeWidth={2} />
                                    : <circle key={p.index} cx={p.cx} cy={p.cy} r={2.5} fill="var(--viz-1)" fillOpacity={0.28} />;
                            }} />
                        <Line type="monotone" dataKey="trend" stroke="var(--viz-1)" strokeWidth={2.5} strokeLinecap="round"
                            dot={false} activeDot={{ r: 4.5, fill: "var(--viz-1)", stroke: "var(--viz-surface)", strokeWidth: 2 }} isAnimationActive={false} />
                    </ComposedChart>
                </ResponsiveContainer>
            </div>
            <ul className="trend-legend">
                <li><span className="trend-key trend-key-series plain" aria-hidden />Trend</li>
                <li><span className="trend-key e1rm-key-dot" aria-hidden />Session best</li>
                <li><span className="trend-key trend-key-dot filled" aria-hidden />New record</li>
                <li><span className="trend-key trend-key-record e1rm-key-record" aria-hidden />Record so far</li>
            </ul>
        </>
    );
}

export default E1rmChart;
