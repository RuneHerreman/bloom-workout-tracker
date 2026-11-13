import { useEffect, useMemo, useRef, useState, lazy, Suspense } from "react";
import { Link, useLocation } from "react-router-dom";
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
import BodyView from "./components/BodyView.tsx";
import GearDetail from "./components/GearDetail.tsx";
import { gearUsage, gearByBucket, type GearUse } from "./gearAnalysis.ts";
import { SHOE_LIFE_KM, resolveKind } from "../profile/gearKinds.ts";
import ConsistencyChart from "./components/ConsistencyChart.tsx";
import MuscleBalance from "./components/MuscleBalance.tsx";
import { AXIS_TICK, VizTooltip } from "./components/ChartParts.tsx";
import type { Highlight } from "./analysis.ts";
import PageLoader from "../../components/general/PageLoader.tsx";
import "../../assets/css/insights.css";

const InsightsHeatmap = lazy(() => import("./InsightsHeatmap.tsx"));

type Tab = "strength" | "cardio" | "body" | "gear";

const TAB_LABELS: Record<Tab, string> = { strength: "Strength", cardio: "Cardio", body: "Body", gear: "Gear" };

const gearKm = (v: number) => `${v >= 100 ? Math.round(v) : Math.round(v * 10) / 10} km`;
const gearKg = (v: number) => (v >= 10_000 ? `${(v / 1000).toFixed(1)} t` : `${Math.round(v)} kg`);

/** One line per item: its main numbers this period. */
function gearSummary(g: GearUse): string {
    const p = g.period;
    if (!p.sessions) return g.lastUsed ? "Not used this period" : "Not used yet";
    return [
        p.km > 0 && gearKm(p.km),
        p.minutes > 0 && !p.km && formatDuration(p.minutes),
        p.sets > 0 && `${p.sets} sets`,
        p.volumeKg > 0 && gearKg(p.volumeKg),
    ].filter(Boolean).join(" · ") || `${p.sessions} session${p.sessions === 1 ? "" : "s"}`;
}

/** The figure an item is mostly about: distance for shoes and bikes, volume for a belt, else sessions. */
function primary(g: GearUse): "km" | "volumeKg" | "sets" | "sessions" {
    return g.total.km > 0 ? "km" : g.total.volumeKg > 0 ? "volumeKg" : g.total.sets > 0 ? "sets" : "sessions";
}

function gearHighlights(items: GearUse[], icons: Record<string, string> | undefined, hasPrevious: boolean): Highlight[] {
    const out: Highlight[] = [];
    for (const g of items) {
        if (!g.listed || resolveKind(g.name, icons) !== "shoe" || g.total.km < SHOE_LIFE_KM * 0.8) continue;
        const past = g.total.km >= SHOE_LIFE_KM;
        out.push({
            id: `wear-${g.name}`, tone: "attention", kind: "distance", exerciseId: g.name,
            title: past ? `${g.name} are past a usual shoe life` : `${g.name} are nearly due`,
            detail: `${gearKm(g.total.km)} so far; running shoes usually last about ${SHOE_LIFE_KM} km.`,
        });
    }
    const used = items.filter(g => g.period.sessions > 0);
    const top = used[0];
    if (top) {
        const p = top.period;
        const before = top.previous.sessions;
        out.push({
            id: "top", tone: "neutral", kind: p.km > 0 ? "distance" : "prs", exerciseId: top.name,
            title: `Most used: ${top.name}`,
            detail: `${p.sessions} session${p.sessions === 1 ? "" : "s"}${p.km > 0 ? `, ${gearKm(p.km)}` : p.volumeKg > 0 ? `, ${gearKg(p.volumeKg)} lifted` : ""}${hasPrevious && before ? ` (${before} the period before)` : ""}.`,
        });
    }
    const idle = items.filter(g => g.listed && g.period.sessions === 0);
    if (idle.length && used.length) {
        out.push({
            id: "idle", tone: "neutral", kind: "consistency",
            title: `${idle.length} item${idle.length === 1 ? "" : "s"} not used this period`,
            detail: idle.slice(0, 3).map(g => g.name).join(", ") + (idle.length > 3 ? ` and ${idle.length - 3} more` : "") + ".",
        });
    }
    return out.slice(0, 4);
}

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
    const [selectedGear, setSelectedGear] = useState<string | null>(null);
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
        sub: [a.totalKm > 0 ? `${a.totalKm} km` : null, a.totalMinutes > 0 ? formatDuration(a.totalMinutes) : null].filter(Boolean).join(" · ") || "Logged without distance or time",
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

    // ── Gear ──────────────────────────────────────────────────────────────────
    const gear = useMemo(() => gearUsage(logs, exercises, range, user?.gear ?? []), [logs, exercises, range, user?.gear]);
    const gHighlights = useMemo(() => gearHighlights(gear, user?.gearIcons, period !== "all"), [gear, user?.gearIcons, period]);
    const gearItems: ExplorerItem[] = useMemo(() => gear.map(g => {
        const key = primary(g);
        const now = g.period[key], before = g.previous[key];
        const pct = before ? Math.round(((now - before) / before) * 100) : null;
        return {
            id: g.name,
            name: g.name,
            sub: gearSummary(g),
            spark: gearByBucket(g.sessions, range).points.map(b => b[key]),
            change: changeOf(period === "all" ? null : pct),
            meta: `${g.period.sessions} session${g.period.sessions !== 1 ? "s" : ""}`,
        };
    }), [gear, range, period]);
    const gearId = selectedGear && gear.some(g => g.name === selectedGear) ? selectedGear : gear[0]?.name ?? null;
    const gearDetail = gear.find(g => g.name === gearId) ?? null;

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
                                aria-label={p.label}
                                onClick={() => { periodTouched.current = true; setPeriod(p.value); }}>
                                <span className="seg-long">{p.label}</span><span className="seg-short" aria-hidden>{p.short}</span>
                            </button>
                        ))}
                    </div>
                    <div className="segmented" role="tablist" aria-label="Insights view">
                        {(["strength", "cardio", "body", "gear"] as Tab[]).map(t => (
                            <button key={t} role="tab" aria-selected={tab === t}
                                className={`segmented-item${tab === t ? " active" : ""}`} onClick={() => setTab(t)}>
                                {TAB_LABELS[t]}
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

                {!loading && !error && tab === "body" && <BodyView range={range} period={period} />}

                {!loading && !error && tab === "gear" && (
                    gear.length === 0 ? (
                        <p className="viz-empty viz-empty-page">
                            No gear yet. Add shoes, a bike or a belt in your <Link to="/profile#gear">profile</Link>, then pick it on exercises in your logbook.
                        </p>
                    ) : (
                        <>
                            <Highlights items={gHighlights} onSelectExercise={setSelectedGear} />
                            <Explorer
                                title="Gear"
                                items={gearItems}
                                selectedId={gearId}
                                onSelect={setSelectedGear}
                                searchPlaceholder="Search gear"
                            >
                                {gearDetail && (
                                    <GearDetail key={gearDetail.name} gear={gearDetail} kind={resolveKind(gearDetail.name, user?.gearIcons)} range={range} />
                                )}
                            </Explorer>
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
