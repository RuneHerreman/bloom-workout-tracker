import { useMemo } from 'react';
import GeneralWidget from "./GeneralWidget.tsx";
import {
    Chart as ChartJS,
    ArcElement,
    Tooltip,
    Legend
} from 'chart.js';
import { Doughnut } from 'react-chartjs-2';
import type { TooltipItem, ChartOptions, ChartData } from 'chart.js';
import WidgetHeader from "./WidgetHeader.tsx";

ChartJS.register(ArcElement, Tooltip, Legend);

export interface FocusSegment {
    label: string;
    value: number;
    color: string;
}

interface TrainingFocusWidgetProps {
    segments?: FocusSegment[];
}

function TrainingFocusWidget({ segments = [] }: TrainingFocusWidgetProps) {

    const data: ChartData<'doughnut'> = useMemo(() => ({
        labels: segments.map(s => s.label),
        datasets: [
            {
                data: segments.map(s => s.value),
                backgroundColor: segments.map(s => s.color),
                borderWidth: 0,
                hoverOffset: 15
            }
        ]
    }), [segments]);

    const options: ChartOptions<'doughnut'> = useMemo(() => ({
        responsive: true,
        maintainAspectRatio: true,
        aspectRatio: 1,
        layout: {
            padding: 15 // This creates a buffer so the segment doesn't hit the edge
        },
        cutout: '60%',
        plugins: {
            legend: {
                display: false
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
                    label: function(context: TooltipItem<'doughnut'>) {
                        return ` ${context.label}: ${context.raw}`;
                    }
                }

            },
        }
    }), []);

    return (
        <GeneralWidget
            header={
                <WidgetHeader title={"Check your split"} subtitle={"Training Focus"}/>
            }
            content={
                segments.length === 0
                    ? <div className="training-focus-content empty-state"><p>No training data available.</p></div>
                    : <div className="training-focus-content">
                        <div className="training-focus-chart">
                            <Doughnut
                                data={data}
                                options={options}
                                aria-label="A doughnut chart showing your training focus distribution"
                            />
                        </div>
                    </div>
            }
        />
    );
}

export default TrainingFocusWidget;