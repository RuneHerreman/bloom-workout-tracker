import { useEffect, useMemo, useState } from "react";
import { ChevronLeft, ChevronRight, Trophy, Sprout, Flame, CalendarDays, Dumbbell } from "lucide-react";
import HeaderComponent from "../../components/general/HeaderComponent.tsx";
import InfoHint from "../../components/general/InfoHint.tsx";
import Skeleton from "../../components/general/Skeleton.tsx";
import { getLeaderboard, metricValue, METRICS, type Leaderboard, type LeaderboardEntry, type LeaderboardMetric } from "./api.ts";
import "../../assets/css/leaderboard.css";

interface Ranked extends LeaderboardEntry {
    rank: number;
    value: number;
}

const METRIC_ICONS = { volume: Dumbbell, prs: Flame, days: CalendarDays } as const;

function formatValue(value: number, metric: LeaderboardMetric): string {
    if (metric !== "volume") return String(value);
    if (value >= 10_000) return `${(value / 1000).toFixed(1)} t`;
    return `${Math.round(value).toLocaleString("en-US")} kg`;
}

function monthLabel(year: number, month: number): string {
    return new Date(Date.UTC(year, month - 1, 1)).toLocaleDateString("en-US", { month: "long", year: "numeric", timeZone: "UTC" });
}

/** Competition ranking (1, 1, 3) over users with activity; inactive users are split out. */
function rank(entries: LeaderboardEntry[], metric: LeaderboardMetric): { ranked: Ranked[]; idle: LeaderboardEntry[] } {
    const active = entries
        .map(e => ({ ...e, value: metricValue(e, metric) }))
        .filter(e => e.value > 0)
        .sort((a, b) => b.value - a.value || a.username.localeCompare(b.username));
    const ranked: Ranked[] = [];
    active.forEach((e, i) => {
        const prev = ranked[i - 1];
        ranked.push({ ...e, rank: prev && prev.value === e.value ? prev.rank : i + 1 });
    });
    const idle = entries.filter(e => metricValue(e, metric) <= 0);
    return { ranked, idle };
}

function Initial({ name, size = "md" }: { name: string; size?: "md" | "lg" }) {
    return <span className={`lb-avatar lb-avatar-${size}`} aria-hidden>{name.charAt(0).toUpperCase()}</span>;
}

function LeaderboardPage() {
    const now = new Date();
    const current = { year: now.getUTCFullYear(), month: now.getUTCMonth() + 1 };
    const [period, setPeriod] = useState(current);
    const [metric, setMetric] = useState<LeaderboardMetric>("volume");
    // The result remembers which month it belongs to, so switching months shows
    // the skeleton immediately without resetting state inside the effect.
    const [result, setResult] = useState<{ key: string; data?: Leaderboard; error?: string } | null>(null);
    const periodKey = `${period.year}-${period.month}`;
    const settled = result?.key === periodKey ? result : null;
    const data = settled?.data ?? null;
    const error = settled?.error ?? null;

    const isCurrentMonth = period.year === current.year && period.month === current.month;

    useEffect(() => {
        let cancelled = false;
        const key = `${period.year}-${period.month}`;
        getLeaderboard(period.year, period.month)
            .then(d => { if (!cancelled) setResult({ key, data: d }); })
            .catch(() => { if (!cancelled) setResult({ key, error: "Couldn't load the leaderboard." }); });
        return () => { cancelled = true; };
    }, [period.year, period.month]);

    function shift(delta: number) {
        setPeriod(p => {
            const d = new Date(Date.UTC(p.year, p.month - 1 + delta, 1));
            return { year: d.getUTCFullYear(), month: d.getUTCMonth() + 1 };
        });
    }

    const { ranked, idle } = useMemo(() => rank(data?.entries ?? [], metric), [data, metric]);
    const podium = ranked.slice(0, 3);
    const max = ranked[0]?.value ?? 0;
    const me = ranked.find(e => e.isCurrentUser);
    const meIdle = idle.find(e => e.isCurrentUser);
    const ahead = me ? ranked.filter(e => e.value > me.value).at(-1) : undefined;
    const metricInfo = METRICS.find(m => m.value === metric)!;

    const totals = useMemo(() => (data?.entries ?? []).reduce(
        (t, e) => ({ volume: t.volume + e.totalVolumeKg, prs: t.prs + e.newPrs, workouts: t.workouts + e.workouts }),
        { volume: 0, prs: 0, workouts: 0 }
    ), [data]);

    return (
        <div className="leaderboard-page">
            <HeaderComponent
                title="Leaderboard"
                subtitle="Community"
                action={
                    <div className="lb-month-switch" aria-label="Choose month">
                        <button type="button" onClick={() => shift(-1)} aria-label="Previous month"><ChevronLeft size={16} /></button>
                        <span className="lb-month-label" aria-live="polite">{monthLabel(period.year, period.month)}</span>
                        <button type="button" onClick={() => shift(1)} disabled={isCurrentMonth} aria-label="Next month"><ChevronRight size={16} /></button>
                    </div>
                }
            />

            <div className="lb-body">
                <div className="lb-toolbar">
                    <div className="lb-tabs" role="tablist" aria-label="Ranking metric">
                        {METRICS.map(m => {
                            const Icon = METRIC_ICONS[m.value];
                            return (
                                <button
                                    key={m.value}
                                    type="button"
                                    role="tab"
                                    aria-selected={metric === m.value}
                                    className={`lb-tab${metric === m.value ? " active" : ""}`}
                                    onClick={() => setMetric(m.value)}
                                >
                                    <Icon size={14} /> {m.label}
                                </button>
                            );
                        })}
                    </div>
                    <span className="lb-toolbar-hint">
                        {metricInfo.describe}
                        <InfoHint title="How it's scored" label="How the leaderboard is scored">
                            <ul className="info-hint-scale">
                                <li><b>Volume</b> reps × weight, warm-ups excluded, lbs converted to kg</li>
                                <li><b>New PRs</b> beat your best from any earlier month</li>
                                <li><b>Active days</b> distinct days you logged a workout</li>
                            </ul>
                            <p>Months run on UTC. Only usernames and totals are shared.</p>
                        </InfoHint>
                    </span>
                </div>

                {error && <div className="error-banner">{error}</div>}

                {!data && !error ? (
                    <div className="lb-skeleton" role="status" aria-label="Loading leaderboard">
                        <div className="lb-podium">
                            {[1, 0, 2].map(i => (
                                <div key={i} className={`lb-podium-slot place-${i + 1}`}>
                                    <Skeleton width="3rem" height="3rem" radius="50%" />
                                    <Skeleton width="5rem" height="0.8rem" />
                                    <Skeleton width="100%" height={`${[7, 5, 4][i]}rem`} radius="0.5rem 0.5rem 0 0" />
                                </div>
                            ))}
                        </div>
                        {[0, 1, 2, 3].map(i => <Skeleton key={i} height="3.25rem" radius="var(--widget-border-radius)" />)}
                    </div>
                ) : data && ranked.length === 0 ? (
                    <div className="lb-empty">
                        <Sprout size={36} className="lb-empty-icon" />
                        <p className="lb-empty-title">Nothing has bloomed yet</p>
                        <p className="lb-empty-sub">
                            {isCurrentMonth
                                ? `No one has scored any ${metricInfo.label.toLowerCase()} in ${monthLabel(period.year, period.month)} — log a workout to take first place.`
                                : `No ${metricInfo.label.toLowerCase()} were recorded in ${monthLabel(period.year, period.month)}.`}
                        </p>
                    </div>
                ) : data && (
                    <>
                        <div className="lb-stats">
                            <div><span className="lb-stat-num">{formatValue(totals.volume, "volume")}</span><span className="lb-stat-label">lifted together</span></div>
                            <div><span className="lb-stat-num">{totals.prs}</span><span className="lb-stat-label">new PRs</span></div>
                            <div><span className="lb-stat-num">{totals.workouts}</span><span className="lb-stat-label">workouts</span></div>
                        </div>

                        <ol className="lb-podium" aria-label="Top three">
                            {[1, 0, 2].map(i => {
                                const e = podium[i];
                                if (!e) return <li key={i} className={`lb-podium-slot place-${i + 1} empty`} aria-hidden />;
                                return (
                                    <li key={e.username} className={`lb-podium-slot place-${i + 1}${e.isCurrentUser ? " me" : ""}`}>
                                        {i === 0 && <Trophy size={18} className="lb-crown" aria-hidden />}
                                        <Initial name={e.username} size="lg" />
                                        <span className="lb-podium-name">{e.username}{e.isCurrentUser && <em> (you)</em>}</span>
                                        <span className="lb-podium-value">{formatValue(e.value, metric)}</span>
                                        <div className="lb-podium-block">
                                            <span className="lb-podium-rank">{e.rank}</span>
                                        </div>
                                    </li>
                                );
                            })}
                        </ol>

                        {me && me.rank > 3 && (
                            <div className="lb-you">
                                <span>You're <b>#{me.rank}</b> of {ranked.length}</span>
                                {ahead && (
                                    <span className="lb-you-gap">
                                        {formatValue(ahead.value - me.value, metric)}{metric !== "volume" ? ` ${metricInfo.unit}` : ""} behind {ahead.username}
                                    </span>
                                )}
                            </div>
                        )}
                        {meIdle && (
                            <div className="lb-you">
                                <span>You haven't scored any {metricInfo.label.toLowerCase()} this month yet.</span>
                            </div>
                        )}

                        <ol className="lb-list">
                            {ranked.map(e => (
                                <li key={e.username} className={`lb-row${e.isCurrentUser ? " me" : ""}`}>
                                    <span className={`lb-rank${e.rank <= 3 ? ` top-${e.rank}` : ""}`}>{e.rank}</span>
                                    <Initial name={e.username} />
                                    <div className="lb-row-main">
                                        <div className="lb-row-top">
                                            <span className="lb-row-name">{e.username}{e.isCurrentUser && <span className="lb-you-tag">You</span>}</span>
                                            <span className="lb-row-value">{formatValue(e.value, metric)}{metric !== "volume" && <small> {metricInfo.unit}</small>}</span>
                                        </div>
                                        <div className="lb-bar" aria-hidden>
                                            <span style={{ width: `${max ? Math.max(4, (e.value / max) * 100) : 0}%` }} />
                                        </div>
                                        <div className="lb-row-meta">
                                            {metric !== "volume" && <span>{formatValue(e.totalVolumeKg, "volume")}</span>}
                                            {metric !== "prs" && <span>{e.newPrs} PR{e.newPrs !== 1 ? "s" : ""}</span>}
                                            {metric !== "days" && <span>{e.activeDays} active day{e.activeDays !== 1 ? "s" : ""}</span>}
                                            <span>{e.workouts} workout{e.workouts !== 1 ? "s" : ""}</span>
                                        </div>
                                    </div>
                                </li>
                            ))}
                        </ol>

                        {idle.length > 0 && (
                            <p className="lb-idle">
                                <Sprout size={13} /> Yet to sprout: {idle.map(e => e.isCurrentUser ? `${e.username} (you)` : e.username).join(", ")}
                            </p>
                        )}
                    </>
                )}
            </div>
        </div>
    );
}

export default LeaderboardPage;
