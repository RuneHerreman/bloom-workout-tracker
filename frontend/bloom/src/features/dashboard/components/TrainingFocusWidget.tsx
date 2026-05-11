import { useMemo, useState } from 'react';
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
    muscleSegments?: FocusSegment[];
}

function TrainingFocusWidget({ segments = [], muscleSegments = [] }: TrainingFocusWidgetProps) {
    const [view, setView] = useState<'type' | 'muscle'>('type');

    const activeSegments = view === 'type' ? segments : muscleSegments;

    const data: ChartData<'doughnut'> = useMemo(() => ({
        labels: activeSegments.map(s => s.label),
        datasets: [
            {
                data: activeSegments.map(s => s.value),
                backgroundColor: activeSegments.map(s => s.color),
                borderWidth: 0,
                hoverOffset: 15
            }
        ]
    }), [activeSegments]);

    const options: ChartOptions<'doughnut'> = useMemo(() => ({
        responsive: true,
        maintainAspectRatio: true,
        aspectRatio: 1,
        layout: {
            padding: 15 // This creates a buffer so the segment doesn't hit the edge
        },
        cutout: '70%',
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
                <div className="training-focus-content">
                    <div className="focus-chips">
                        <button
                            className={`focus-chip${view === 'type' ? ' active' : ''}`}
                            onClick={() => setView('type')}
                        >Type</button>
                        <button
                            className={`focus-chip${view === 'muscle' ? ' active' : ''}`}
                            onClick={() => setView('muscle')}
                        >Muscle group</button>
                    </div>
                    {activeSegments.length === 0
                        ? <p className="focus-empty">No data this month.</p>
                        : <div className="training-focus-row">
                            <div className="training-focus-chart">
                                <Doughnut
                                    data={data}
                                    options={options}
                                    aria-label="A doughnut chart showing your training focus distribution"
                                />
                            </div>
                            <ul className="focus-legend">
                                {(() => {
                                    const total = activeSegments.reduce((sum, s) => sum + s.value, 0);
                                    return [...activeSegments].sort((a, b) => b.value - a.value).map(s => (
                                        <li key={s.label} className="focus-legend-item">
                                            <span className="focus-legend-dot" style={{ background: s.color }} />
                                            <span className="focus-legend-label">{s.label}</span>
                                            <span className="focus-legend-count">{s.value}</span>
                                            <span className="focus-legend-pct">({String(Math.round((s.value / total) * 100)).padStart(2, '0')}%)</span>
                                        </li>
                                    ));
                                })()}
                            </ul>
                        </div>
                    }
                </div>
            }
        />
    );
}

export default TrainingFocusWidget;