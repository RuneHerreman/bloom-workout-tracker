interface StatWidgetProps {
    label: string;
    value: string | number;
    changePercent?: number;
    subtext?: string;
}

function StatWidget({ label, value, changePercent, subtext }: StatWidgetProps) {
    const isPositive = changePercent !== undefined && changePercent >= 0;

    return (
        <div className="widget stat-widget">
            <div className="stat-widget-header">
                <span className="stat-label">{label}</span>
                {changePercent !== undefined && (
                    <span className={`stat-change ${isPositive ? "positive" : "negative"}`}>
                        {isPositive ? "↗" : "↘"} {Math.abs(changePercent)}%
                    </span>
                )}
            </div>
            <div className="stat-value">{value}</div>
            {subtext && <p className="stat-subtext">{subtext}</p>}
        </div>
    );
}

export default StatWidget;
