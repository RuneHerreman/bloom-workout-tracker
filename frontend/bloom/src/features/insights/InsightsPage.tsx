import { useEffect, useMemo, useRef, useState, lazy, Suspense } from "react";
import { useLocation } from "react-router-dom";
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import type { LoggedWorkout, User } from "../../assets/js/data/apiTypes.ts";
import { getLogs, peekLogs } from "../logbook/api.ts";
import { searchExercises, peekExercises } from "../exercises/api.ts";
import type { Exercise } from "../exercises/api.ts";
import { getMe, peekMe } from "../auth/api.ts";
import {
    PERIODS, periodRange, previousRange, suggestPeriod, inRange,
    exerciseProgress, consistency, muscleBalance, strengthHighlights,
    cardioSessions, cardioActivities, cardioByBucket, cardioHighlights, formatDuration,
    type Period,
} from "./analysis.ts";
import { estimateMaxHr, buildHrZoneData, buildGpxCache, extractRoutePolylines, ZONE_LABELS } from "./cardioTransforms.ts";
import Highlights from "./components/Highlights.tsx";
import Explorer, { type ExplorerItem } from "./components/Explorer.tsx";
import StrengthDetail from "./components/StrengthDetail.tsx";
import CardioDetail from "./components/CardioDetail.tsx";
import ConsistencyChart from "./components/ConsistencyChart.tsx";
import MuscleBalance from "./components/MuscleBalance.tsx";
import { AXIS_TICK, VizTooltip } from "./components/ChartParts.tsx";
import PageLoader from "../../components/general/PageLoader.tsx";
import "../../assets/css/insights.css";

const InsightsHeatmap = lazy(() => import("./InsightsHeatmap.tsx"));

type Tab = "strength" | "cardio";

const fmtRange = (start: Date, end: Date) => {
    const sameYear = start.getFullYear() === end.getFullYear();
    const f = (d: Date, y: boolean) => d.toLocaleDateString("en-GB", { day: "numeric", month: "short", ...(y ? { year: "numeric" } : {}) });
    return `${f(start, !sameYear)} – ${f(end, true)}`;
};

function changeOf(pct: number | null): ExplorerItem["change"] {
    if (pct === null) return null;
    return { text: `${pct > 0 ? "+" : ""}${pct}%`, direction: pct > 0 ? "up" : pct < 0 ? "down" : "flat" };
}

function InsightsPage() {
    const [tab, setTab] = useState<Tab>("strength");
    const [logs, setLogs] = useState<LoggedWorkout[]>(() => peekLogs() ?? []);
    const [exercises, setExercises] = useState<Record<string, Exercise>>(
        () => Object.fromEntries((peekExercises() ?? []).map(e => [e.id, e]))
    );
    const [user, setUser] = useState<User | null>(() => peekMe() ?? null);
    const [loading, setLoading] = useState(() => !(peekLogs() && peekExercises()));
    const [error, setError] = useState<string | null>(null);
    // Opened from a dashboard lift or record: show that exercise over the same six months.
    const linkedExercise = (useLocation().state as { exerciseId?: string } | null)?.exerciseId ?? null;
    // Open on the shortest period that has training in it; the user's choice wins afterwards.
    const [period, setPeriod] = useState<Period>(() => (linkedExercise ? "6m" : suggestPeriod(peekLogs() ?? [])));
    const periodTouched = useRef(linkedExercise !== null);
    const [selectedStrength, setSelectedStrength] = useState<string | null>(linkedExercise);
    const [selectedCardio, setSelectedCardio] = useState<string | null>(null);
    const explorerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        Promise.all([getLogs(), searchExercises(), getMe()])
            .then(([l, e, u]) => {
                setLogs(l);
                setUser(u);
                setExercises(Object.fromEntries(e.map(x => [x.id, x])));
                if (!periodTouched.current) setPeriod(suggestPeriod(l));
            })
            .catch(e => setError(e instanceof Error ? e.message : "Failed to load insights"))
            .finally(() => setLoading(false));
    }, []);

    const range = useMemo(() => periodRange(period, logs), [period, logs]);
    const periodLogs = useMemo(() => logs.filter(l => inRange(l.loggedAt, range)), [logs, range]);
    const goal = user?.activeDays || null;
    const lastWorkout = useMemo(
        () => periodLogs.reduce<Date | null>((d, l) => { const t = new Date(l.loggedAt); return !d || t > d ? t : d; }, null),
        [periodLogs]
    );

    // ── Strength ──────────────────────────────────────────────────────────────
    const progress = useMemo(() => exerciseProgress(logs, exercises, range), [logs, exercises, range]);
    const consistencyData = useMemo(() => consistency(periodLogs, range), [periodLogs, range]);
    const balance = useMemo(() => muscleBalance(periodLogs, exercises, range), [periodLogs, exercises, range]);
    const sHighlights = useMemo(
        () => strengthHighlights(progress, balance, consistencyData.perWeek, goal, range),
        [progress, balance, consistencyData.perWeek, goal, range]
    );
    const strengthItems: ExplorerItem[] = useMemo(() => progress.map(p => ({
        id: p.id,
        name: p.name,
        sub: p.muscles.join(", "),
        spark: p.sessions.map(s => s.heaviest),
        change: changeOf(p.changePct),
        meta: `${p.sessions.length} session${p.sessions.length !== 1 ? "s" : ""}`,
    })), [progress]);
    const defaultStrength = sHighlights.find(h => h.exerciseId)?.exerciseId ?? progress[0]?.id ?? null;
    const strengthId = selectedStrength && progress.some(p => p.id === selectedStrength) ? selectedStrength : defaultStrength;
    const strengthDetail = progress.find(p => p.id === strengthId) ?? null;

    function focusExercise(id: string) {
        setSelectedStrength(id);
        explorerRef.current?.scrollIntoView({ behavior: "smooth", block: "start" });
    }

    // ── Cardio ────────────────────────────────────────────────────────────────
    const allCardio = useMemo(() => cardioSessions(logs, exercises), [logs, exercises]);
    const cardioNow = useMemo(() => allCardio.filter(s => inRange(s.date, range)), [allCardio, range]);
    const cardioPrev = useMemo(() => {
        const prev = previousRange(range);
        return allCardio.filter(s => inRange(s.date, prev));
    }, [allCardio, range]);
    const activities = useMemo(() => cardioActivities(cardioNow), [cardioNow]);
    const cHighlights = useMemo(() => cardioHighlights(cardioNow, cardioPrev, period !== "all"), [cardioNow, cardioPrev, period]);
    const cardioBuckets = useMemo(() => cardioByBucket(cardioNow, range), [cardioNow, range]);
    const cardioItems: ExplorerItem[] = useMemo(() => activities.map(a => ({
        id: a.id,
        name: a.name,
        sub: `${a.totalKm} km · ${formatDuration(a.totalMinutes)}`,
        spark: a.sessions.map(s => s.km),
        change: a.paceChangeSec === null ? null : {
            text: `${a.paceChangeSec <= 0 ? "−" : "+"}${Math.abs(a.paceChangeSec)}s/km`,
            direction: a.paceChangeSec < 0 ? "up" : a.paceChangeSec > 0 ? "down" : "flat",
        },
        meta: `${a.sessions.length} session${a.sessions.length !== 1 ? "s" : ""}`,
    })), [activities]);
    const cardioId = selectedCardio && activities.some(a => a.id === selectedCardio) ? selectedCardio : activities[0]?.id ?? null;
    const cardioDetail = activities.find(a => a.id === cardioId) ?? null;

    const maxHr = useMemo(() => (user?.birthDate ? estimateMaxHr(user.birthDate) : 190), [user]);
    const gpxCache = useMemo(() => buildGpxCache(logs), [logs]);
    const routes = useMemo(() => extractRoutePolylines(periodLogs, gpxCache), [periodLogs, gpxCache]);
    const hr = useMemo(() => buildHrZoneData(periodLogs, maxHr, gpxCache), [periodLogs, maxHr, gpxCache]);
    const zoneTotal = hr.overall.reduce((n, z) => n + z.seconds, 0);

    const hasStrength = progress.length > 0;
    const hasCardio = cardioNow.length > 0;

    return (
        <div className="insights-page">
            <header className="page-header">
                <div className="page-header-titles">
                    <h1>Insights</h1>
                    <p className="header-subtitle">{fmtRange(range.start, range.end)}</p>
                </div>
                <div className="page-header-right insights-controls">
                    <div className="segmented" role="radiogroup" aria-label="Period">
                        {PERIODS.map(p => (
                            <button key={p.value} role="radio" aria-checked={period === p.value}
                                className={`segmented-item${period === p.value ? " active" : ""}`}
                                onClick={() => { periodTouched.current = true; setPeriod(p.value); }}>
                                {p.label}
                            </button>
                        ))}
                    </div>
                    <div className="segmented" role="tablist" aria-label="Insights view">
                        {(["strength", "cardio"] as Tab[]).map(t => (
                            <button key={t} role="tab" aria-selected={tab === t}
                                className={`segmented-item${tab === t ? " active" : ""}`} onClick={() => setTab(t)}>
                                {t === "strength" ? "Strength" : "Cardio"}
                            </button>
                        ))}
                    </div>
                </div>
            </header>

            <div className="insights-body">
                {loading && <PageLoader label="Loading your history…" />}
                {error && <div className="error-banner">{error}</div>}

                {!loading && !error && tab === "strength" && (
                    !hasStrength ? (
                        <p className="viz-empty viz-empty-page">No strength sessions between {fmtRange(range.start, range.end)}. Try a longer period.</p>
                    ) : (
                        <>
                            <Highlights items={sHighlights} onSelectExercise={focusExercise} />

                            <div ref={explorerRef} className="scroll-anchor">
                                <Explorer
                                    title="Exercises"
                                    items={strengthItems}
                                    selectedId={strengthId}
                                    onSelect={setSelectedStrength}
                                    searchPlaceholder="Search exercises or muscles"
                                >
                                    {strengthDetail && <StrengthDetail key={strengthDetail.id} exercise={strengthDetail} range={range} />}
                                </Explorer>
                            </div>

                            <div className="insights-grid">
                                <section className="panel">
                                    <header className="panel-head">
                                        <h2 className="panel-title">Consistency</h2>
                                        <p className="panel-desc">
                                            {consistencyData.perWeek} workouts a week{goal ? ` · goal ${goal}` : ""}
                                        </p>
                                    </header>
                                    <ConsistencyChart points={consistencyData.points} unit={consistencyData.unit} goalPerWeek={goal} />
                                </section>
                                <section className="panel">
                                    <header className="panel-head">
                                        <h2 className="panel-title">Muscle balance</h2>
                                        <p className="panel-desc">Working sets per training week</p>
                                    </header>
                                    <MuscleBalance muscles={balance} periodEnd={lastWorkout ?? range.end} />
                                </section>
                            </div>
                        </>
                    )
                )}

                {!loading && !error && tab === "cardio" && (
                    !hasCardio ? (
                        <p className="viz-empty viz-empty-page">No cardio sessions between {fmtRange(range.start, range.end)}. Try a longer period.</p>
                    ) : (
                        <>
                            <Highlights items={cHighlights} />

                            <Explorer
                                title="Activities"
                                items={cardioItems}
                                selectedId={cardioId}
                                onSelect={setSelectedCardio}
                                searchPlaceholder="Search activities"
                            >
                                {cardioDetail && <CardioDetail key={cardioDetail.id} activity={cardioDetail} range={range} />}
                            </Explorer>

                            <div className="insights-grid">
                                <section className="panel">
                                    <header className="panel-head">
                                        <h2 className="panel-title">Distance per {cardioBuckets.unit}</h2>
                                        <p className="panel-desc">All cardio activities combined</p>
                                    </header>
                                    <div className="viz-chart" role="img" aria-label={`Distance per ${cardioBuckets.unit}`}>
                                        <ResponsiveContainer width="100%" height="100%">
                                            <BarChart data={cardioBuckets.points} margin={{ top: 8, right: 8, bottom: 0, left: -16 }}>
                                                <CartesianGrid vertical={false} stroke="var(--viz-grid)" />
                                                <XAxis dataKey="label" tick={AXIS_TICK} tickLine={false} axisLine={false} interval="preserveStartEnd" minTickGap={12} />
                                                <YAxis tick={AXIS_TICK} tickLine={false} axisLine={false} width={48} tickFormatter={v => `${v} km`} />
                                                <Tooltip
                                                    cursor={{ fill: "var(--viz-cursor)" }}
                                                    content={({ active, payload }) => {
                                                        if (!active || !payload?.length) return null;
                                                        const p = payload[0].payload as typeof cardioBuckets.points[number];
                                                        return <VizTooltip title={p.longLabel} rows={[
                                                            { label: "Distance", value: `${p.km} km`, color: "var(--viz-1)" },
                                                            { label: "Sessions", value: p.sessions },
                                                            { label: "Time", value: formatDuration(p.minutes) },
                                                        ]} />;
                                                    }}
                                                />
                                                <Bar dataKey="km" fill="var(--viz-1)" radius={[4, 4, 0, 0]} maxBarSize={28} />
                                            </BarChart>
                                        </ResponsiveContainer>
                                    </div>
                                </section>

                                {routes.length > 0 ? (
                                    <section className="panel panel-map">
                                        <header className="panel-head">
                                            <h2 className="panel-title">Routes</h2>
                                            <p className="panel-desc">{routes.length} GPS track{routes.length !== 1 ? "s" : ""}</p>
                                        </header>
                                        <Suspense fallback={<div className="insights-heatmap insights-heatmap-loading" />}>
                                            <InsightsHeatmap routes={routes} />
                                        </Suspense>
                                    </section>
                                ) : (
                                    <section className="panel panel-placeholder">
                                        <h2 className="panel-title">Routes</h2>
                                        <p className="panel-desc">Attach a GPX file to a cardio exercise in your logbook to see your routes here.</p>
                                    </section>
                                )}
                            </div>

                            {hr.sessions.length >= 3 && zoneTotal > 0 && (
                                <section className="panel">
                                    <header className="panel-head">
                                        <h2 className="panel-title">Heart-rate zones</h2>
                                        <p className="panel-desc">Time in zone across {hr.sessions.length} sessions · max HR {maxHr} bpm</p>
                                    </header>
                                    <div className="zone-bar" role="img" aria-label="Time in heart-rate zones">
                                        {hr.overall.map((z, i) => z.seconds > 0 && (
                                            <span key={i} style={{ flexGrow: z.seconds, background: `var(--zone-${i + 1})` }}
                                                title={`${ZONE_LABELS[i]}: ${formatDuration(z.seconds / 60)}`} />
                                        ))}
                                    </div>
                                    <ul className="zone-legend">
                                        {hr.overall.map((z, i) => (
                                            <li key={i}>
                                                <span className="zone-swatch" style={{ background: `var(--zone-${i + 1})` }} />
                                                <span className="zone-name">{ZONE_LABELS[i]}</span>
                                                <span className="zone-time">{formatDuration(z.seconds / 60)} · {Math.round((z.seconds / zoneTotal) * 100)}%</span>
                                            </li>
                                        ))}
                                    </ul>
                                </section>
                            )}
                        </>
                    )
                )}
            </div>
        </div>
    );
}

export default InsightsPage;
