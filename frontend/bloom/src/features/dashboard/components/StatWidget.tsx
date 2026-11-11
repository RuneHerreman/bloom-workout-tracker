import GeneralWidget from "./GeneralWidget.tsx";

interface StatWidgetProps {
    label: string;
    value: string | number;
    unit: string;
    /** Context line under the number, e.g. "12 last month". */
    caption?: string;
    /** Month-over-month change; only shown when both months have data. */
    changePercent?: number;
}

function StatWidget({ label, value, unit, caption, changePercent }: StatWidgetProps) {
    const isPositive = changePercent !== undefined && changePercent >= 0;

    return (
        <GeneralWidget
            className="stat-widget"
            content={<>
                <span className="stat-label">{label}</span>
                <div className="stat-value">{value}<span className="stat-unit">{unit}</span></div>
                {(caption || changePercent !== undefined) && (
                    <p className="stat-caption">
                        {caption}
                        {changePercent !== undefined && (
                            <span className={`stat-change ${isPositive ? "positive" : "negative"}`}>
                                {isPositive ? "↑" : "↓"} {Math.abs(changePercent)}%
                            </span>
                        )}
                    </p>
                )}
            </>}
        />
    );
}

export default StatWidget;
