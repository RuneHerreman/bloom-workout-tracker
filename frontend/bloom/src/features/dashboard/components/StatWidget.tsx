import GeneralWidget from "./GeneralWidget.tsx";

interface StatWidgetProps {
    label: string;
    value: string | number;
    changePercent?: number;
    subtext?: string;
    unit: string;
}

function StatWidget({ label, value, changePercent, subtext, unit }: StatWidgetProps) {
    const isPositive = changePercent !== undefined && changePercent >= 0;

    return (
        <GeneralWidget
            className={"stat-widget"}
            content={<>
                <span className="stat-label">{label}</span>

                <div className="stat-value">{value}<span className="stat-unit">{unit}</span></div>
                {subtext && <p className="stat-subtext">{subtext}</p>}

                {changePercent !== undefined && (
                    <span className={`stat-change ${isPositive ? "positive" : "negative"}`}>
                    {isPositive ? "↗" : "↘"} {Math.abs(changePercent)}%
                </span>
                )}
            </>}
        />
    );
}

export default StatWidget;
