import { useState, useEffect, useMemo, useRef, lazy, Suspense } from "react";
import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    LogarithmicScale,
    PointElement,
    LineElement,
    BarElement,
    ArcElement,
    DoughnutController,
    Tooltip,
    Legend,
    Filler,
    type ChartOptions,
    type TooltipItem,
} from "chart.js";
import { Bar, Line, Doughnut } from "react-chartjs-2";
import { ChevronDown, Search, Check } from "lucide-react";
import type { LoggedWorkout, ExerciseVolumeResponse, User } from "../../assets/js/data/apiTypes.ts";
import type { ExerciseType } from "../../types.ts";
import { getLogs, getVolume } from "../logbook/api.ts";
import { searchExercises } from "../exercises/api.ts";
import type { Exercise } from "../exercises/api.ts";
import { getMe } from "../auth/api.ts";
import {
    filterLogsByPeriod,
    filterVolumeByPeriod,
    extractCardioMonthly,
    getAvailableYears,
    buildVolumeChartSeries,
} from "./insightsTransforms.ts";
import {
    estimateMaxHr,
    buildHrZoneData,
    buildAtlCtlSeries,
    buildGpxCache,
    extractRoutePolylines,
    ZONE_COLORS,
    ZONE_LABELS,
} from "./cardioTransforms.ts";
import GeneralWidget from "../dashboard/components/GeneralWidget.tsx";
import WidgetHeader from "../dashboard/components/WidgetHeader.tsx";
import { useDarkModeContext } from "../../context/DarkModeContext.tsx";
import "../../assets/css/insights.css";

const InsightsHeatmap = lazy(() => import("./InsightsHeatmap.tsx"));

ChartJS.register(
    CategoryScale, LinearScale, LogarithmicScale,
    PointElement, LineElement, BarElement, ArcElement, DoughnutController,
    Tooltip, Legend, Filler
);

type Tab = "strength" | "cardio";

const STRENGTH_TYPES: ExerciseType[] = ["Strength", "Plyometric"];
const FIXED_PERIODS = ["1m", "3m", "6m", "1y"];
const PERIOD_LABELS: Record<string, string> = { "1m": "1M", "3m": "3M", "6m": "6M", "1y": "1Y", "max": "All" };
const PALETTE = [
    "#2D8055", // brand green
    "#E9762B", // amber-orange
    "#528B8D", // teal
    "#7B6B9E", // violet
    "#C4734A", // terracotta
    "#4A8FAE", // steel blue
    "#9E6B7A", // dusty rose
    "#6B9E3E", // lime
    "#8B6B3E", // warm brown
    "#5B7FAE", // periwinkle
    "#B08B3E", // gold
    "#7A4F9E", // plum
    "#3D9E7A", // emerald
    "#AE5B5B", // muted red
    "#5B8B6E", // sage
];

const TOOLTIP_STYLE = {
    backgroundColor: "rgba(255,255,255,0.92)",
    titleColor: "#333",
    bodyColor: "#666",
    borderColor: "#e3e3e3",
    borderWidth: 1,
    padding: 10,
};

function desaturate(hex: string, amount: number): string {
    const r = parseInt(hex.slice(1, 3), 16);
    const g = parseInt(hex.slice(3, 5), 16);
    const b = parseInt(hex.slice(5, 7), 16);
    const lum = Math.round(r * 0.299 + g * 0.587 + b * 0.114);
    const mix = (c: number) => Math.round(c + (lum - c) * amount);
    return `#${mix(r).toString(16).padStart(2, "0")}${mix(g).toString(16).padStart(2, "0")}${mix(b).toString(16).padStart(2, "0")}`;
}

function fmtZoneTime(seconds: number): string {
    const h = Math.floor(seconds / 3600);
    const m = Math.floor((seconds % 3600) / 60);
    if (h > 0) return `${h}h ${m}m`;
    return `${m}m`;
}

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
    const [user, setUser] = useState<User | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const pickerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        Promise.all([getLogs(), getVolume(), searchExercises(), getMe()])
            .then(([l, v, e, u]) => { setLogs(l); setVolumeData(v); setExercises(e); setUser(u); })
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
    const tooltipStyle = useMemo(() => dark ? {
        backgroundColor: "rgba(30,30,28,0.97)",
        titleColor: "#f2efe8",
        bodyColor: "#a8a49c",
        borderColor: "#46463e",
        borderWidth: 1,
        padding: 10,
    } : TOOLTIP_STYLE, [dark]);

    const barOptions = useMemo((): ChartOptions<"bar"> => ({
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false }, tooltip: tooltipStyle },
        scales: {
            x: { border: { display: false }, grid: { color: gridColor }, ticks: { color: tickColor } },
            y: { border: { display: false }, grid: { display: false }, ticks: { color: tickColor } },
        },
    }), [dark, gridColor, tickColor, tooltipStyle]);

    const lineOptions = useMemo((): ChartOptions<"line"> => ({
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false }, tooltip: tooltipStyle },
        transitions: { active: { animation: { duration: 150 } } },
        scales: {
            x: { border: { display: false }, grid: { color: gridColor }, ticks: { color: tickColor } },
            y: { beginAtZero: true, border: { display: false }, grid: { color: gridColor }, ticks: { color: tickColor } },
        },
    }), [dark, gridColor, tickColor, tooltipStyle]);

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

    const isSinglePrMonth = volumeChart.labels.length <= 1;

    const prLineOptions = useMemo((): ChartOptions<"line"> => ({
        responsive: true,
        maintainAspectRatio: false,
        transitions: { active: { animation: { duration: 150 } } },
        plugins: {
            legend: { display: false },
            tooltip: {
                ...tooltipStyle,
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
                offset: isSinglePrMonth,
                grid: { color: gridColor },
                ticks: { color: tickColor, padding: 10 },
            },
        },
    }), [dark, gridColor, tickColor, tooltipStyle, isSinglePrMonth]);

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

    const prLineData = useMemo(() => {
        const filtered = volumeChart.series
            .filter(s => selectedExercises.size === 0 || selectedExercises.has(s.name));

        // volumeChart.series is pre-sorted by most recent PR date — take the first 5
        const featured = new Set(volumeChart.series.slice(0, 5).map(s => s.name));

        const datasets = filtered.map(s => {
            const color = exerciseColorMap.get(s.name) ?? PALETTE[0];
            const full = featured.has(s.name);
            const dimColor = `${desaturate(color, 0.8)}20`;
            return {
                label: s.name,
                data: s.data,
                spanGaps: false,
                borderColor: full ? color : dimColor,
                backgroundColor: `${color}22`,
                borderWidth: full ? 2 : 1.5,
                fill: false,
                tension: 0.3,
                pointRadius: full ? 4 : 2,
                pointHoverRadius: 7,
                pointBackgroundColor: full ? color : dimColor,
                pointBorderWidth: 0,
                hitRadius: 20,
            };
        });

        // Render dimmed lines first so featured lines draw on top
        datasets.sort((a, b) => {
            const fa = featured.has(a.label ?? "");
            const fb = featured.has(b.label ?? "");
            return fa === fb ? 0 : fa ? 1 : -1;
        });

        return { labels: volumeChart.labels, datasets };
    }, [volumeChart, selectedExercises, exerciseColorMap]);

    // ── Cardio base ────────────────────────────────────────────────────────────
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

    // ── Cardio advanced ────────────────────────────────────────────────────────
    const maxHr = useMemo(() => user?.birthDate ? estimateMaxHr(user.birthDate) : 190, [user]);
    const gpxCache = useMemo(() => buildGpxCache(logs), [logs]);
    const routePolylines = useMemo(() => extractRoutePolylines(filteredLogs, gpxCache), [filteredLogs, gpxCache]);
    const hrZoneData = useMemo(() => buildHrZoneData(filteredLogs, maxHr, gpxCache), [filteredLogs, maxHr, gpxCache]);
    const atlCtlSeries = useMemo(() => buildAtlCtlSeries(filteredLogs, maxHr, gpxCache), [filteredLogs, maxHr, gpxCache]);

    const hrDonutData = useMemo(() => ({
        labels: ZONE_LABELS,
        datasets: [{
            data: hrZoneData.overall.map(z => Math.round(z.seconds / 60)),
            backgroundColor: ZONE_COLORS,
            borderWidth: 0,
            hoverOffset: 6,
        }],
    }), [hrZoneData]);

    const hrDonutOptions = useMemo((): ChartOptions<"doughnut"> => ({
        responsive: true,
        maintainAspectRatio: false,
        cutout: "68%",
        plugins: {
            legend: { display: false },
            tooltip: {
                ...tooltipStyle,
                callbacks: {
                    label: (ctx: TooltipItem<"doughnut">) =>
                        ` ${ZONE_LABELS[ctx.dataIndex]}: ${fmtZoneTime((ctx.raw as number) * 60)}`,
                },
            },
        },
    }), [tooltipStyle]);

    const hrStackedData = useMemo(() => {
        const recentSessions = hrZoneData.sessions.slice(-20);
        return {
            labels: recentSessions.map(s => s.label),
            datasets: ZONE_LABELS.map((label, zi) => ({
                label,
                data: recentSessions.map(s => Math.round(s.zones[zi] / 60)),
                backgroundColor: ZONE_COLORS[zi],
                stack: "zones",
                borderWidth: 0,
                borderRadius: zi === 4 ? 3 : 0,
            })),
        };
    }, [hrZoneData]);

    const hrStackedOptions = useMemo((): ChartOptions<"bar"> => ({
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: { display: false },
            tooltip: {
                ...tooltipStyle,
                callbacks: {
                    label: (ctx: TooltipItem<"bar">) =>
                        ` ${ctx.dataset.label}: ${fmtZoneTime((ctx.raw as number) * 60)}`,
                },
            },
        },
        scales: {
            x: {
                stacked: true,
                border: { display: false },
                grid: { display: false },
                ticks: { color: tickColor, maxRotation: 45, font: { size: 10 } },
            },
            y: {
                stacked: true,
                border: { display: false },
                grid: { color: gridColor },
                ticks: { color: tickColor, callback: v => `${v}m` },
            },
        },
    }), [dark, gridColor, tickColor, tooltipStyle]);

    const atlCtlChartData = useMemo(() => {
        const pts = atlCtlSeries;
        const labels = pts.map(p => {
            const d = new Date(p.date);
            return d.toLocaleDateString("en-GB", { day: "numeric", month: "short" });
        });
        return {
            labels,
            datasets: [
                {
                    label: "Fitness (CTL)",
                    data: pts.map(p => Math.round(p.ctl * 10) / 10),
                    borderColor: "#558B6E",
                    backgroundColor: "#558B6E22",
                    borderWidth: 2,
                    fill: true,
                    tension: 0.4,
                    pointRadius: 0,
                    pointHoverRadius: 5,
                    pointBorderWidth: 0,
                    pointBackgroundColor: "#558B6E",
                },
                {
                    label: "Fatigue (ATL)",
                    data: pts.map(p => Math.round(p.atl * 10) / 10),
                    borderColor: "#E9762B",
                    backgroundColor: "#E9762B18",
                    borderWidth: 2,
                    fill: true,
                    tension: 0.4,
                    pointRadius: 0,
                    pointHoverRadius: 5,
                    pointBorderWidth: 0,
                    pointBackgroundColor: "#E9762B",
                },
                {
                    label: "Form (CTL−ATL)",
                    data: pts.map(p => Math.round(p.form * 10) / 10),
                    borderColor: "#528B8D",
                    backgroundColor: "transparent",
                    borderWidth: 1.5,
                    borderDash: [5, 4],
                    fill: false,
                    tension: 0.4,
                    pointRadius: 0,
                    pointHoverRadius: 4,
                    pointBorderWidth: 0,
                    pointBackgroundColor: "#528B8D",
                },
            ],
        };
    }, [atlCtlSeries]);

    const atlCtlOptions = useMemo((): ChartOptions<"line"> => ({
        responsive: true,
        maintainAspectRatio: false,
        interaction: { mode: "index", intersect: false },
        transitions: { active: { animation: { duration: 150 } } },
        plugins: {
            legend: { display: true, position: "top", align: "end", labels: { color: tickColor, font: { size: 11 }, boxWidth: 12, padding: 16, usePointStyle: true } },
            tooltip: {
                ...tooltipStyle,
                padding: 12,
                boxPadding: 6,
                usePointStyle: true,
            },
        },
        scales: {
            x: {
                border: { display: false },
                grid: { color: gridColor },
                ticks: { color: tickColor, maxTicksLimit: 10, font: { size: 10 } },
            },
            y: {
                border: { display: false },
                grid: { color: gridColor },
                ticks: { color: tickColor },
            },
        },
    }), [dark, gridColor, tickColor, tooltipStyle]);

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
                            className={`filter-chip${period === p ? " active" : ""}`}
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
                            <div className="insights-filter-row">
                                <div className="insights-filters">
                                    <div className="filter-chips">
                                        {STRENGTH_TYPES.map(t => (
                                            <button
                                                key={t}
                                                className={`filter-chip${typeFilters.includes(t) ? " active" : ""}`}
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
                                                    className={`filter-chip${muscleFilters.includes(m) ? " active" : ""}`}
                                                    onClick={() => toggleMuscle(m)}
                                                >
                                                    {m}
                                                </button>
                                            ))}
                                        </div>
                                    )}
                                </div>

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
                    <>
                        {/* Route Heatmap */}
                        {routePolylines.length > 0 && (
                            <section className="insights-section">
                                <div className="insights-section-header">
                                    <span className="insights-section-title">Route Map</span>
                                </div>
                                <GeneralWidget
                                    className="general-widget--map"
                                    content={
                                        <Suspense fallback={<div className="insights-heatmap insights-heatmap-loading" />}>
                                            <InsightsHeatmap routes={routePolylines} />
                                        </Suspense>
                                    }
                                />
                            </section>
                        )}

                        {/* ATL / CTL Fitness-Fatigue-Form */}
                        {atlCtlSeries.length > 1 && (
                            <section className="insights-section">
                                <div className="insights-section-header">
                                    <span className="insights-section-title">Fitness · Fatigue · Form</span>
                                </div>
                                <GeneralWidget
                                    content={
                                        <div className="insights-atl-chart">
                                            <Line data={atlCtlChartData} options={atlCtlOptions} />
                                        </div>
                                    }
                                />
                            </section>
                        )}

                        {/* HR Zones */}
                        {hrZoneData.hasData && (
                            <section className="insights-section">
                                <div className="insights-section-header">
                                    <span className="insights-section-title">Heart Rate Zones</span>
                                </div>
                                <div className="insights-grid-2">
                                    <GeneralWidget
                                        header={<WidgetHeader title="Distribution" subtitle="Overall time in zone" />}
                                        content={
                                            <div className="insights-zone-donut-wrap">
                                                <div className="insights-zone-donut">
                                                    <Doughnut data={hrDonutData} options={hrDonutOptions} />
                                                </div>
                                                <div className="insights-zone-legend">
                                                    {hrZoneData.overall.map((z, i) => (
                                                        <div key={i} className="insights-zone-row">
                                                            <span className="insights-zone-dot" style={{ background: z.color }} />
                                                            <span className="insights-zone-label">{z.label}</span>
                                                            <span className="insights-zone-time">{fmtZoneTime(z.seconds)}</span>
                                                        </div>
                                                    ))}
                                                </div>
                                            </div>
                                        }
                                    />
                                    <GeneralWidget
                                        header={<WidgetHeader title="Per Session" subtitle="Recent 20 sessions" />}
                                        content={
                                            <div className="insights-chart-area">
                                                <Bar data={hrStackedData} options={hrStackedOptions} />
                                            </div>
                                        }
                                    />
                                </div>
                            </section>
                        )}

                        {/* Existing cardio stats */}
                        {cardioMonths.length === 0 ? (
                            <p className="insights-empty">No cardio sessions logged in this period.</p>
                        ) : (
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
                        )}
                    </>
                )}
            </div>
        </div>
    );
}

export default InsightsPage;
