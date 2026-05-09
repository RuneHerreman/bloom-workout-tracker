import { useState } from "react";

export interface ExerciseSeries {
    name: string;
    data: number[];     // monthly PR values
    color: string;
}

interface VolumeWidgetProps {
    series?: ExerciseSeries[];
    monthLabels?: string[];
}

const PLACEHOLDER: ExerciseSeries[] = [
    { name: "Bench Press",   color: "#003E1F", data: [70, 72, 75, 70, 80, 75, 78, 105, 85, 100, 90, 110] },
    { name: "Squat",         color: "#2D8055", data: [80, 85, 75, 90, 85, 95, 80, 90, 100, 85, 105, 115] },
    { name: "Deadlift",      color: "#E9762B", data: [100, 95, 110, 100, 105, 115, 100, 120, 110, 125, 115, 130] },
    { name: "Overhead Press",color: "#7B1616", data: [45, 47, 50, 48, 52, 50, 55, 52, 58, 55, 60, 62] },
    { name: "Row",           color: "#595959", data: [60, 65, 62, 68, 65, 70, 68, 72, 70, 75, 73, 78] },
];

const MONTHS = ["0","1","2","3","4","5","6","7","8","9","10","11"];

function LineChart({ series, labels }: { series: ExerciseSeries[]; labels: string[] }) {
    const allValues = series.flatMap(s => s.data);
    const minVal = Math.min(...allValues);
    const maxVal = Math.max(...allValues);
    const range = maxVal - minVal || 1;

    const W = 600;
    const H = 180;
    const padX = 24;
    const padY = 12;

    function toX(i: number) {
        return padX + (i / (labels.length - 1)) * (W - padX * 2);
    }

    function toY(v: number) {
        return H - padY - ((v - minVal) / range) * (H - padY * 2);
    }

    function polyline(data: number[]) {
        return data.map((v, i) => `${toX(i)},${toY(v)}`).join(" ");
    }

    const gridLines = 4;

    return (
        <svg viewBox={`0 0 ${W} ${H}`} width="100%" height="100%" preserveAspectRatio="none">
            {Array.from({ length: gridLines + 1 }, (_, i) => {
                const y = padY + (i / gridLines) * (H - padY * 2);
                const value = Math.round(maxVal - (i / gridLines) * range);
                return (
                    <g key={i}>
                        <line x1={padX} y1={y} x2={W - padX} y2={y} stroke="#E3E3E3" strokeWidth="1"/>
                        <text x={0} y={y + 4} fontSize="10" fill="#595959">{value}</text>
                    </g>
                );
            })}
            {labels.map((label, i) => (
                <text key={i} x={toX(i)} y={H} fontSize="10" fill="#595959" textAnchor="middle">
                    {label}
                </text>
            ))}
            {series.map((s, i) => (
                <polyline
                    key={i}
                    points={polyline(s.data)}
                    fill="none"
                    stroke={s.color}
                    strokeWidth="1.5"
                    strokeLinejoin="round"
                    strokeLinecap="round"
                />
            ))}
        </svg>
    );
}

function VolumeWidget({ series = PLACEHOLDER, monthLabels = MONTHS }: VolumeWidgetProps) {
    const [selected, setSelected] = useState(series[0]?.name ?? "");

    const active = selected
        ? series.filter(s => s.name === selected)
        : series;

    return (
        <div className="widget">
            <div className="volume-widget-header">
                <p className="widget-title" style={{ margin: 0 }}>History of top 5 exercises PR</p>
                <select value={selected} onChange={e => setSelected(e.target.value)}>
                    <option value="">All</option>
                    {series.map(s => (
                        <option key={s.name} value={s.name}>{s.name}</option>
                    ))}
                </select>
            </div>
            <div className="chart-area">
                <LineChart series={active} labels={monthLabels}/>
            </div>
        </div>
    );
}

export default VolumeWidget;
