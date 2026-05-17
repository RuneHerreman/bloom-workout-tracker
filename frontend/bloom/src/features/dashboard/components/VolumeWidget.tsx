import { useMemo } from "react";
import GeneralWidget from "./GeneralWidget.tsx";
import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend,
    type TooltipItem
} from 'chart.js';
import { Line } from 'react-chartjs-2';
import WidgetHeader from "./WidgetHeader.tsx";

ChartJS.register(
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend
);

export interface ExerciseSeries {
    name: string;
    data: number[];     // monthly PR values
}

interface VolumeWidgetProps {
    series?: ExerciseSeries[];
    monthLabels?: string[];
}

const PALETTE = [
    "#558B6E", // 1. Vibrant Green (Muted Sage anchor)
    "#5C7A67", // 2. Fern Accent (More green than the grey Eucalyptus)
    "#528B8D", // 3. Cyan Accent (A dusty, saturated Teal/Cyan)
    "#4A6E75", // 4. Steel Blue Accent (A deep Slate-Teal—no more plain grey)
    "#3E544B", // 5. Brand Primary (Deep but soft forest)
];

function VolumeWidget({ series = [], monthLabels = [] }: VolumeWidgetProps) {
    const chartData = useMemo(() => ({
        labels: monthLabels,
        datasets: series.map((s, i) => {
            const color = PALETTE[i % PALETTE.length];
            return {
                label: s.name,
                data: s.data,
                borderColor: color,
                backgroundColor: color,
                borderWidth: 3,
                fill: false,
                tension: 0.4,
                pointRadius: 3,
                pointHoverRadius: 6,
                pointBackgroundColor: color,
                pointBorderColor: '#fff',
                pointBorderWidth: 2,
                hitRadius: 30,
                hoverBorderWidth: 4
            };
        })
    }), [series, monthLabels]);

    const isSingleMonth = monthLabels.length <= 1;

    const options = useMemo(() => ({
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                display: true,
                position: 'bottom' as const,
                labels: {
                    usePointStyle: true,
                    pointStyle: 'rectRounded',
                    padding: 20,
                    font: { size: 12, family: 'sans-serif' }
                }
            },
            tooltip: {
                backgroundColor: 'rgba(255, 255, 255, 0.9)',
                titleColor: '#333',
                bodyColor: '#666',
                borderColor: '#e3e3e3',
                borderWidth: 1,
                padding: 12,
                boxPadding: 6,
                usePointStyle: true,
                callbacks: {
                    label: function(context: TooltipItem<'line'>) {
                        return ` ${context.dataset.label}: ${context.raw} kg`;
                    }
                }

            }
        },
        scales: {
            y: {
                beginAtZero: true,
                grace: '15%',
                border: { display: false },
                grid: {
                    color: '#F0F0F0',
                },
                ticks: {
                    callback: function(value: number | string) {
                        return value + " kg";
                    }
                },
            },
            x: {
                border: { display: false },
                offset: isSingleMonth,
                grid: {
                    display: true,
                },
                stacked: false,
                ticks: {
                    color: '#999',
                    padding: 10
                }
            }
        }
    }), [isSingleMonth]);

    return (
        <GeneralWidget
            header={
                <WidgetHeader
                    title={"History of 5 most recent PRs"}
                    subtitle={"Volume logged · Top 5 PR progression"}
                />

            }
            content={
                <div className="volume-widget">
                    <div className="chart-area">
                        <Line data={chartData} options={options} />
                    </div>
                </div>
            }
        />
    );
}

export default VolumeWidget;
