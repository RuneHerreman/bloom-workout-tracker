import type { ReactNode } from "react";

/** Tooltip body shared by every Insights chart; styled by .viz-tooltip in insights.css. */
export function VizTooltip({ title, rows }: { title: string; rows: { label: string; value: ReactNode; color?: string }[] }) {
    return (
        <div className="viz-tooltip">
            <p className="viz-tooltip-title">{title}</p>
            {rows.map(r => (
                <p key={r.label} className="viz-tooltip-row">
                    {r.color && <span className="viz-tooltip-swatch" style={{ background: r.color }} />}
                    <span className="viz-tooltip-label">{r.label}</span>
                    <span className="viz-tooltip-value">{r.value}</span>
                </p>
            ))}
        </div>
    );
}

/** Axis tick styling: recessive, tabular numbers. */
export const AXIS_TICK = { fill: "var(--viz-axis)", fontSize: 11 } as const;

/** Tiny trend line for list rows. Purely decorative — the row's text carries the numbers. */
export function Sparkline({ values, width = 72, height = 24 }: { values: number[]; width?: number; height?: number }) {
    if (values.length < 2) return <span className="sparkline-empty" style={{ width, height }} aria-hidden />;
    const min = Math.min(...values), max = Math.max(...values);
    const span = max - min || 1;
    const pad = 3;
    const pts = values.map((v, i) => [
        pad + (i / (values.length - 1)) * (width - pad * 2),
        pad + (1 - (v - min) / span) * (height - pad * 2),
    ]);
    const last = pts.at(-1)!;
    return (
        <svg width={width} height={height} className="sparkline" aria-hidden>
            <polyline points={pts.map(p => p.join(",")).join(" ")} fill="none" strokeWidth={1.5} strokeLinejoin="round" strokeLinecap="round" />
            <circle cx={last[0]} cy={last[1]} r={2.5} />
        </svg>
    );
}
