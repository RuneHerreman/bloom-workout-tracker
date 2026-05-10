import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend
} from 'chart.js';
import { Line } from 'react-chartjs-2';

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
    "#52C98A", // 1. Vibrant Green (Bright, energetic brand anchor)
    "#7B5EA7", // 2. Purple Accent (Perfect complementary contrast to green)
    "#2DA89A", // 3. Cyan Accent (Modern, fresh, and ties the cool tones together)
    "#4A6FA5", // 4. Steel Blue Accent (Smooth, professional, and distinct)
    "#003E1F", // 5. Brand Primary (Deep grounding color so it doesn't look like a circus)
];

function VolumeWidget({ series = [], monthLabels = [] }: VolumeWidgetProps) {
    const chartData = {
        labels: monthLabels,
        datasets: series.map((s, i) => {
            const color = PALETTE[i % PALETTE.length];
            const fillColor = `${color}1A`;

            return {
                label: s.name,
                data: s.data,
                borderColor: color,
                backgroundColor: fillColor,
                borderWidth: 3,
                fill: true,
                tension: 0.4,
                pointRadius: 0,
                pointHoverRadius: 6,
                pointBackgroundColor: color,
                pointBorderColor: '#fff',
                pointBorderWidth: 2,
                hitRadius: 30,
                hoverBorderWidth: 4
            };
        })
    };

    const options = {
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
            }
        },
        scales: {
            y: {
                beginAtZero: true,
                border: { display: false },
                grid: {
                    color: '#F0F0F0',
                },
                ticks: {
                    color: '#999',
                    padding: 10
                }
            },
            x: {
                border: { display: false },
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
    };

    return (
        <div className="widget volume-widget">
            <div className="volume-widget-header">
                <p className="widget-title" style={{ fontWeight: '600', marginBottom: '1rem' }}>
                    History of 5 most recent PRs
                </p>
            </div>
            <div className="chart-area" style={{ height: '300px' }}>
                <Line data={chartData} options={options} />
            </div>
        </div>
    );
}

export default VolumeWidget;
