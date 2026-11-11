import type { ReactNode } from "react";

interface StatWidgetProps {
    label: string;
    value: string | number;
    unit?: string;
    /** Context line under the number, e.g. "3 by this point in October". */
    caption?: ReactNode;
    /** Change against the comparison in the caption; only shown when both sides have data. */
    changePercent?: number;
    /** Small visual under the number (e.g. the week's day markers). */
    children?: ReactNode;
}

function StatWidget({ label, value, unit, caption, changePercent, children }: StatWidgetProps) {
    const direction = changePercent === undefined ? null : changePercent > 0 ? "up" : changePercent < 0 ? "down" : "flat";
    return (
        <div className="stat-widget">
            <span className="stat-label">{label}</span>
            <div className="stat-value">{value}{unit && <span className="stat-unit">{unit}</span>}</div>
            {children}
            {(caption || direction) && (
                <p className="stat-caption">
                    {direction && (
                        <span className={`stat-change ${direction}`}>
                            {direction === "up" ? "↑" : direction === "down" ? "↓" : "→"} {Math.abs(changePercent!)}%
                        </span>
                    )}
                    {caption}
                </p>
            )}
        </div>
    );
}

export default StatWidget;
