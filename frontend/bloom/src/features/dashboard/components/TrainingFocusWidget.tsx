import { useMemo } from 'react';
import {
    Chart as ChartJS,
    ArcElement,
    Tooltip,
    Legend
} from 'chart.js';
import { Doughnut } from 'react-chartjs-2';
import type { TooltipItem, ChartOptions, ChartData } from 'chart.js';

ChartJS.register(ArcElement, Tooltip, Legend);

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

function TrainingFocusWidget({ segments = PLACEHOLDER }: TrainingFocusWidgetProps) {

    const data: ChartData<'doughnut'> = useMemo(() => ({
        labels: segments.map(s => s.label),
        datasets: [
            {
                data: segments.map(s => s.value),
                backgroundColor: segments.map(s => s.color),
                borderWidth: 0,
                hoverOffset: 4
            }
        ]
    }), [segments]);

    const options: ChartOptions<'doughnut'> = useMemo(() => ({
        responsive: true,
        maintainAspectRatio: false, // Allows the parent div to dictate height
        cutout: '70%',
        plugins: {
            legend: {
                display: true,
                position: 'bottom',
                labels: {
                    padding: 10
                }
            },
            tooltip: {
                callbacks: {
                    label: function(context: TooltipItem<'doughnut'>) {
                        // Added a unit (e.g., '%') for better UX. Adjust as needed!
                        return ` ${context.label}: ${context.raw}%`;
                    }
                }
            }
        }
    }), []);

    if (!segments || segments.length === 0) {
        return (
            <div className="widget training-focus-widget">
                <p className="widget-title">Training focus</p>
                <div className="training-focus-content empty-state">
                    <p>No training data available.</p>
                </div>
            </div>
        );
    }

    return (
        <div className="widget training-focus-widget">
            <p className="widget-title">Training focus</p>
            <div className="training-focus-content" style={{ position: 'relative', height: '250px' }}>
                <div className="training-focus-chart" style={{ width: '100%', height: '100%' }}>
                    {/* 4. Added accessibility attributes */}
                    <Doughnut
                        data={data}
                        options={options}
                        aria-label="A doughnut chart showing your training focus distribution"
                    />
                </div>
            </div>
        </div>
    );
}

export default TrainingFocusWidget;