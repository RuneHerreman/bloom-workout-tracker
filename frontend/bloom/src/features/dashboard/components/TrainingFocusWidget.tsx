export interface FocusSegment {
    label: string;
    value: number;
    color: string;
}

interface TrainingFocusWidgetProps {
    segments?: FocusSegment[];
}

const PLACEHOLDER: FocusSegment[] = [
    { label: "Strength",    value: 55, color: "#003E1F" },
    { label: "Plyometric",  value: 25, color: "#2D8055" },
    { label: "Cardio",      value: 20, color: "#E9762B" },
];

function DonutChart({ segments }: { segments: FocusSegment[] }) {
    const total = segments.reduce((acc, s) => acc + s.value, 0);
    const r = 36;
    const cx = 50;
    const cy = 50;
    const circumference = 2 * Math.PI * r;
    const gap = 2;

    let cumulative = 0;

    return (
        <svg viewBox="0 0 100 100" width="110" height="110">
            {segments.map((seg, i) => {
                const proportion = seg.value / total;
                const dashLength = proportion * circumference - gap;
                const dashOffset = (circumference / 4) - (cumulative / total) * circumference;
                cumulative += seg.value;

                return (
                    <circle
                        key={i}
                        cx={cx} cy={cy} r={r}
                        fill="none"
                        stroke={seg.color}
                        strokeWidth="14"
                        strokeDasharray={`${dashLength} ${circumference - dashLength}`}
                        strokeDashoffset={dashOffset}
                    />
                );
            })}
        </svg>
    );
}

function TrainingFocusWidget({ segments = PLACEHOLDER }: TrainingFocusWidgetProps) {
    return (
        <div className="widget training-focus-widget">
            <p className="widget-title">Training focus</p>
            <div className="training-focus-chart">
                <DonutChart segments={segments}/>
            </div>
        </div>
    );
}

export default TrainingFocusWidget;
