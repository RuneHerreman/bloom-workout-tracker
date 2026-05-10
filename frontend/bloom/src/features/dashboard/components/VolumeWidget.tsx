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

const MONTHS = ["1","2","3","4","5","6","7","8","9","10","11","12"];

function VolumeWidget({ series = PLACEHOLDER, monthLabels = MONTHS }: VolumeWidgetProps) {
    const chartData = {
        labels: monthLabels,
        datasets: series.map(s => ({
            label: s.name,
            data: s.data,
            borderColor: s.color,
            backgroundColor: s.color,
            tension: 0.4,
            fill: true,
            pointRadius: 3,
        }))
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
                    padding: 20
                }
            },
        },
        tension: 0.1,
        scales: {
            y: {
                beginAtZero: true,
                grid: {
                    color: '#E3E3E3',
                }
            },
            x: {
                grid: {
                    display: false,
                }
            }
        }
    };

    return (
        <div className="widget volume-widget">
            <div className="volume-widget-header">
                <p className="widget-title">History of top exercises PR</p>
            </div>
            <div className="chart-area">
                <Line data={chartData} options={options} />
            </div>
        </div>
    );
}

export default VolumeWidget;
