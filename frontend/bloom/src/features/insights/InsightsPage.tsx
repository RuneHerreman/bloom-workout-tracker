import { useState, useEffect, useMemo, useRef } from "react";
import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    LogarithmicScale,
    PointElement,
    LineElement,
    BarElement,
    Tooltip,
    Legend,
    Filler,
    type ChartOptions,
    type TooltipItem,
} from "chart.js";
import { Bar, Line } from "react-chartjs-2";
import { ChevronDown, Search, Check } from "lucide-react";
import type { LoggedWorkout, ExerciseVolumeResponse } from "../../assets/js/data/apiTypes.ts";
import type { ExerciseType } from "../../types.ts";
import { getLogs, getVolume } from "../logbook/api.ts";
import { searchExercises } from "../exercises/api.ts";
import type { Exercise } from "../exercises/api.ts";
import {
    filterLogsByPeriod,
    filterVolumeByPeriod,
    extractCardioMonthly,
    getAvailableYears,
    buildVolumeChartSeries,
} from "./insightsTransforms.ts";
import GeneralWidget from "../dashboard/components/GeneralWidget.tsx";
import WidgetHeader from "../dashboard/components/WidgetHeader.tsx";
import { useDarkModeContext } from "../../context/DarkModeContext.tsx";
import "../../assets/css/insights.css";

ChartJS.register(CategoryScale, LinearScale, LogarithmicScale, PointElement, LineElement, BarElement, Tooltip, Legend, Filler);

type Tab = "strength" | "cardio";

const STRENGTH_TYPES: ExerciseType[] = ["Strength", "Plyometric"];
const FIXED_PERIODS = ["1m", "3m", "6m", "1y"];
const PERIOD_LABELS: Record<string, string> = { "1m": "1M", "3m": "3M", "6m": "6M", "1y": "1Y", "max": "All" };
const PALETTE = [
    "#558B6E", "#528B8D", "#7DB5A0", "#4A6E75", "#3E544B",
    "#6B8F71", "#5E7B9E", "#7A9E87", "#3D7A6E", "#8B7355",
    "#6B7A8B", "#9E7A5C", "#4A7C6E", "#7B6B8B", "#5C8B7A",
];

const TOOLTIP_STYLE = {
    backgroundColor: "rgba(255,255,255,0.92)",
    titleColor: "#333",
    bodyColor: "#666",
    borderColor: "#e3e3e3",
    borderWidth: 1,
    padding: 10,
};

function InsightsPage() {
    const [tab, setTab] = useState<Tab>("strength");
    const { dark } = useDarkModeContext();
    const [period, setPeriod] = useState("1y");
    const [typeFilters, setTypeFilters] = useState<ExerciseType[]>([...STRENGTH_TYPES]);
    const [muscleFilters, setMuscleFilters] = useState<string[]>([]);
    const [selectedExercises, setSelectedExercises] = useState<Set<string>>(new Set());
    const [exercisePickerOpen, setExercisePickerOpen] = useState(false);
    const [exerciseSearch, setExerciseSearch] = useState("");
    const [logs, setLogs] = useState<LoggedWorkout[]>([]);
    const [volumeData, setVolumeData] = useState<ExerciseVolumeResponse[]>([]);
    const [, setExercises] = useState<Exercise[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const pickerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        Promise.all([getLogs(), getVolume(), searchExercises()])
            .then(([l, v, e]) => { setLogs(l); setVolumeData(v); setExercises(e); })
            .catch(e => setError(e instanceof Error ? e.message : "Failed to load insights"))
            .finally(() => setLoading(false));
    }, []);

    useEffect(() => {
        if (!exercisePickerOpen) return;
        function onDown(e: MouseEvent) {
            if (pickerRef.current && !pickerRef.current.contains(e.target as Node)) {
                setExercisePickerOpen(false);
            }
        }
        document.addEventListener("mousedown", onDown);
        return () => document.removeEventListener("mousedown", onDown);
    }, [exercisePickerOpen]);

    const gridColor = dark ? "rgba(255,255,255,0.06)" : "#F0F0F0";
    const tickColor = dark ? "#666" : "#999";

    const prLineOptions = useMemo((): ChartOptions<"line"> => ({
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: { display: false },
            tooltip: {
                ...TOOLTIP_STYLE,
                padding: 12,
                boxPadding: 6,
                usePointStyle: true,
                callbacks: { label: (ctx: TooltipItem<"line">) => ` ${ctx.dataset.label}: ${ctx.raw} kg` },
            },
        },
        scales: {
            y: {
                type: "logarithmic",
                border: { display: false },
                grid: { color: gridColor },
                ticks: { callback: (v) => `${v} kg`, color: tickColor },
            },
            x: {
                border: { display: false },
                offset: true,
                grid: { color: gridColor },
                ticks: { color: tickColor, padding: 10 },
            },
        },
    }), [dark, gridColor, tickColor]);

    const barOptions = useMemo((): ChartOptions<"bar"> => ({
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false }, tooltip: TOOLTIP_STYLE },
        scales: {
            x: { border: { display: false }, grid: { color: gridColor }, ticks: { color: tickColor } },
            y: { border: { display: false }, grid: { display: false }, ticks: { color: tickColor } },
        },
    }), [dark, gridColor, tickColor]);

    const lineOptions = useMemo((): ChartOptions<"line"> => ({
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false }, tooltip: TOOLTIP_STYLE },
        scales: {
            x: { border: { display: false }, grid: { color: gridColor }, ticks: { color: tickColor } },
            y: { beginAtZero: true, border: { display: false }, grid: { color: gridColor }, ticks: { color: tickColor } },
        },
    }), [dark, gridColor, tickColor]);

    const availableYears = useMemo(() => getAvailableYears(volumeData, logs), [volumeData, logs]);
    const periods = useMemo(() => [...FIXED_PERIODS, ...availableYears.map(String), "max"], [availableYears]);
    const filteredLogs = useMemo(() => filterLogsByPeriod(logs, period), [logs, period]);
    const filteredVolume = useMemo(() => filterVolumeByPeriod(volumeData, period), [volumeData, period]);

    const allMuscles = useMemo(() => {
        const s = new Set<string>();
        filteredVolume.forEach(ex => ex.targetMuscles.forEach(m => s.add(m)));
        return [...s].sort();
    }, [filteredVolume]);

    const prFilteredVolume = useMemo(() => {
        let v = filteredVolume.filter(ex => typeFilters.includes(ex.exerciseType as ExerciseType));
        if (muscleFilters.length > 0) {
            v = v.filter(ex => ex.targetMuscles.some(m => muscleFilters.includes(m)));
        }
        return v;
    }, [filteredVolume, typeFilters, muscleFilters]);

    const volumeChart = useMemo(() => buildVolumeChartSeries(prFilteredVolume, Infinity), [prFilteredVolume]);

    // Stable color map keyed by exercise name so colors don't shift when filtering
    const exerciseColorMap = useMemo(() => {
        const map = new Map<string, string>();
        volumeChart.series.forEach((s, i) => map.set(s.name, PALETTE[i % PALETTE.length]));
        return map;
    }, [volumeChart]);

    const filteredExerciseNames = useMemo(() => {
        const all = volumeChart.series.map(s => s.name);
        if (!exerciseSearch) return all;
        const q = exerciseSearch.toLowerCase();
        return all.filter(n => n.toLowerCase().includes(q));
    }, [volumeChart, exerciseSearch]);

    const prLineData = useMemo(() => ({
        labels: volumeChart.labels,
        datasets: volumeChart.series
            .filter(s => selectedExercises.size === 0 || selectedExercises.has(s.name))
            .map(s => {
                const color = exerciseColorMap.get(s.name) ?? PALETTE[0];
                return {
                    label: s.name,
                    data: s.data,
                    spanGaps: false,
                    borderColor: color,
                    backgroundColor: `${color}22`,
                    borderWidth: 2,
                    fill: false,
                    tension: 0.3,
                    pointRadius: 4,
                    pointHoverRadius: 7,
                    pointBackgroundColor: color,
                    pointBorderWidth: 0,
                    hitRadius: 20,
                };
            }),
    }), [volumeChart, selectedExercises, exerciseColorMap]);

    // Cardio
    const cardioMonths = useMemo(() => extractCardioMonthly(filteredLogs), [filteredLogs]);
    const cardioLabels = useMemo(() => cardioMonths.map(m => m.label), [cardioMonths]);

    const sessionsData = useMemo(() => ({
        labels: cardioLabels,
        datasets: [{ label: "Sessions", data: cardioMonths.map(m => m.sessions), backgroundColor: "#7DB5A0", borderRadius: 4 }],
    }), [cardioMonths, cardioLabels]);

    const distanceData = useMemo(() => ({
        labels: cardioLabels,
        datasets: [{
            label: "Distance (km)",
            data: cardioMonths.map(m => Math.round(m.totalDistanceKm * 10) / 10),
            borderColor: "#7DB5A0",
            backgroundColor: "#7DB5A033",
            borderWidth: 2,
            fill: true,
            tension: 0.4,
            pointRadius: 3,
            pointBackgroundColor: "#7DB5A0",
            pointBorderWidth: 0,
        }],
    }), [cardioMonths, cardioLabels]);

    const durationData = useMemo(() => ({
        labels: cardioLabels,
        datasets: [{ label: "Duration (min)", data: cardioMonths.map(m => Math.round(m.totalDurationMin)), backgroundColor: "#528B8D", borderRadius: 4 }],
    }), [cardioMonths, cardioLabels]);

    function toggleType(t: ExerciseType) {
        setTypeFilters(prev => prev.includes(t) ? prev.filter(x => x !== t) : [...prev, t]);
    }

    function toggleMuscle(m: string) {
        setMuscleFilters(prev => prev.includes(m) ? prev.filter(x => x !== m) : [...prev, m]);
    }

    function toggleExercise(name: string) {
        setSelectedExercises(prev => {
            const next = new Set(prev);
            if (next.has(name)) next.delete(name); else next.add(name);
            return next;
        });
    }

    const pickerLabel = selectedExercises.size === 0
        ? "All exercises"
        : `${selectedExercises.size} selected`;

    return (
        <div className="insights-page">
            <header className="page-header">
                <div>
                    <p>Analytics</p>
                    <h1>Insights</h1>
                </div>
                <div className="page-header-right">
                    <div className="insights-tabs">
                        <button
                            className={`insights-tab${tab === "strength" ? " active" : ""}`}
                            onClick={() => setTab("strength")}
                        >
                            Strength
                        </button>
                        <button
                            className={`insights-tab${tab === "cardio" ? " active" : ""}`}
                            onClick={() => setTab("cardio")}
                        >
                            Cardio
                        </button>
                    </div>
                </div>
            </header>

            <div className="insights-period-bar">
                <div className="filter-chips">
                    {periods.map(p => (
                        <button
                            key={p}
                            className={`filter-chip strength${period === p ? " active" : ""}`}
                            onClick={() => setPeriod(p)}
                        >
                            {PERIOD_LABELS[p] ?? p}
                        </button>
                    ))}
                </div>
            </div>

            <div className="insights-body">
                {loading && <p className="insights-empty">Loading…</p>}
                {error && <div className="error-banner">{error}</div>}

                {!loading && !error && tab === "strength" && (
                    <section className="insights-section">
                        <div className="insights-section-header">
                            <span className="insights-section-title">PR History</span>
                            <div className="insights-filters">
                                <div className="filter-chips">
                                    {STRENGTH_TYPES.map(t => (
                                        <button
                                            key={t}
                                            className={`filter-chip strength${typeFilters.includes(t) ? " active" : ""}`}
                                            onClick={() => toggleType(t)}
                                        >
                                            {t}
                                        </button>
                                    ))}
                                </div>
                                {allMuscles.length > 0 && (
                                    <div className="filter-chips" style={{ flexWrap: "wrap" }}>
                                        {allMuscles.map(m => (
                                            <button
                                                key={m}
                                                className={`filter-chip strength${muscleFilters.includes(m) ? " active" : ""}`}
                                                onClick={() => toggleMuscle(m)}
                                            >
                                                {m}
                                            </button>
                                        ))}
                                    </div>
                                )}
                            </div>

                            {/* Exercise picker */}
                            <div className="insights-exercise-picker" ref={pickerRef}>
                                <button
                                    className={`insights-picker-trigger${selectedExercises.size > 0 ? " has-selection" : ""}`}
                                    onClick={() => setExercisePickerOpen(o => !o)}
                                >
                                    {pickerLabel}
                                    <ChevronDown size={11} />
                                </button>

                                {exercisePickerOpen && (
                                    <div className="insights-picker-dropdown">
                                        <div className="insights-picker-search">
                                            <Search size={12} />
                                            <input
                                                type="text"
                                                placeholder="Search exercises…"
                                                value={exerciseSearch}
                                                onChange={e => setExerciseSearch(e.target.value)}
                                                autoFocus
                                            />
                                        </div>
                                        <div className="insights-picker-list">
                                            {filteredExerciseNames.map(name => {
                                                const color = exerciseColorMap.get(name) ?? PALETTE[0];
                                                const checked = selectedExercises.has(name);
                                                return (
                                                    <button
                                                        key={name}
                                                        className={`insights-picker-item${checked ? " selected" : ""}`}
                                                        onClick={() => toggleExercise(name)}
                                                    >
                                                        <span className="insights-picker-dot" style={{ background: color }} />
                                                        <span className="insights-picker-name">{name}</span>
                                                        {checked && <Check size={11} />}
                                                    </button>
                                                );
                                            })}
                                            {filteredExerciseNames.length === 0 && (
                                                <p className="insights-picker-empty">No exercises found</p>
                                            )}
                                        </div>
                                        {selectedExercises.size > 0 && (
                                            <button
                                                className="insights-picker-clear"
                                                onClick={() => setSelectedExercises(new Set())}
                                            >
                                                Show all
                                            </button>
                                        )}
                                    </div>
                                )}
                            </div>
                        </div>

                        <GeneralWidget
                            content={
                                volumeChart.series.length === 0
                                    ? <p className="insights-empty">No data for this period.</p>
                                    : (
                                        <div className="insights-pr-chart">
                                            <Line data={prLineData} options={prLineOptions} />
                                        </div>
                                    )
                            }
                        />
                    </section>
                )}

                {!loading && !error && tab === "cardio" && (
                    cardioMonths.length === 0
                        ? <p className="insights-empty">No cardio sessions logged in this period.</p>
                        : (
                            <>
                                <div className="insights-grid-2">
                                    <GeneralWidget
                                        header={<WidgetHeader title="Sessions" subtitle="Monthly cardio sessions" />}
                                        content={
                                            <div className="insights-chart-area">
                                                <Bar data={sessionsData} options={barOptions} />
                                            </div>
                                        }
                                    />
                                    <GeneralWidget
                                        header={<WidgetHeader title="Distance" subtitle="Monthly total · km" />}
                                        content={
                                            <div className="insights-chart-area">
                                                <Line data={distanceData} options={lineOptions} />
                                            </div>
                                        }
                                    />
                                </div>
                                <GeneralWidget
                                    header={<WidgetHeader title="Duration" subtitle="Monthly total · minutes" />}
                                    content={
                                        <div className="insights-chart-area">
                                            <Bar data={durationData} options={barOptions} />
                                        </div>
                                    }
                                />
                            </>
                        )
                )}
            </div>
        </div>
    );
}

export default InsightsPage;
