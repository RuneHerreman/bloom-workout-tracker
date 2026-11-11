import { Bar, BarChart, CartesianGrid, ReferenceLine, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import type { ConsistencyPoint } from "../analysis.ts";
import { AXIS_TICK, VizTooltip } from "./ChartParts.tsx";

interface ConsistencyChartProps {
    points: ConsistencyPoint[];
    unit: "week" | "month";
    /** Target workouts per week from the user's profile. */
    goalPerWeek: number | null;
}

function ConsistencyChart({ points, unit, goalPerWeek }: ConsistencyChartProps) {
    const goal = goalPerWeek ? (unit === "week" ? goalPerWeek : Math.round(goalPerWeek * 4.35)) : null;
    const hasCardio = points.some(p => p.cardio > 0);
    const max = Math.max(goal ?? 0, ...points.map(p => p.strength + p.cardio), 1);

    return (
        <>
            <div className="viz-chart" role="img" aria-label={`Workouts per ${unit}`}>
                <ResponsiveContainer width="100%" height="100%">
                    <BarChart data={points} margin={{ top: 8, right: 8, bottom: 0, left: -24 }} barCategoryGap="22%">
                        <CartesianGrid vertical={false} stroke="var(--viz-grid)" />
                        <XAxis dataKey="label" tick={AXIS_TICK} tickLine={false} axisLine={false} interval="preserveStartEnd" minTickGap={12} />
                        <YAxis allowDecimals={false} domain={[0, Math.ceil(max * 1.15)]} tick={AXIS_TICK} tickLine={false} axisLine={false} width={44} />
                        <Tooltip
                            cursor={{ fill: "var(--viz-cursor)" }}
                            content={({ active, payload }) => {
                                if (!active || !payload?.length) return null;
                                const p = payload[0].payload as ConsistencyPoint;
                                return (
                                    <VizTooltip
                                        title={p.longLabel}
                                        rows={[
                                            { label: "Strength", value: p.strength, color: "var(--viz-1)" },
                                            ...(hasCardio ? [{ label: "Cardio", value: p.cardio, color: "var(--viz-2)" }] : []),
                                        ]}
                                    />
                                );
                            }}
                        />
                        {goal && (
                            <ReferenceLine
                                y={goal}
                                stroke="var(--viz-goal)"
                                strokeDasharray="4 4"
                                label={{ value: `Goal ${goal}`, position: "insideTopRight", fill: "var(--viz-axis)", fontSize: 11 }}
                            />
                        )}
                        <Bar dataKey="strength" stackId="w" fill="var(--viz-1)" maxBarSize={28} radius={hasCardio ? 0 : [4, 4, 0, 0]} />
                        {hasCardio && <Bar dataKey="cardio" stackId="w" fill="var(--viz-2)" maxBarSize={28} radius={[4, 4, 0, 0]} />}
                    </BarChart>
                </ResponsiveContainer>
            </div>
            {hasCardio && (
                <ul className="viz-legend">
                    <li><span className="viz-legend-swatch" style={{ background: "var(--viz-1)" }} />Strength</li>
                    <li><span className="viz-legend-swatch" style={{ background: "var(--viz-2)" }} />Cardio</li>
                </ul>
            )}
        </>
    );
}

export default ConsistencyChart;
