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
    "#003E1F", // 1. Brand Primary (Deep Green)
    "#7B5EA7", // 2. Purple Accent (High Contrast)
    "#2D8055", // 3. Brand Mid-Green
    "#8B5E3C", // 4. Brown/Bronze Accent
    "#1A6640", // 5. Forest Green
    "#4A6FA5", // 6. Steel Blue Accent
    "#52C98A", // 7. Vibrant Green
    "#1B6E6E", // 8. Petrol/Teal Accent
    "#1C4B33", // 9. Muted Dark Green
    "#8CC63F", // 10. Lime/Citrus Accent
    "#0D5230", // 11. Deepest Green
    "#2DA89A"  // 12. Cyan Accent
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
                    History of top exercises PR
                </p>
            </div>
            <div className="chart-area" style={{ height: '300px' }}>
                <Line data={chartData} options={options} />
            </div>
        </div>
    );
}

export default VolumeWidget;
