// The body log as chart series: raw readings, their smoothed trend, and the
// numbers for the stat row. All maths comes from utils/bodyMath.

import type { BodyEntry } from "../../assets/js/data/apiTypes.ts";
import {
    BODY_FAT_RATE_MIN, BODY_FAT_TREND_ALPHA, TREND_ALPHA, bodyComposition, bodyFatRatePerMonth, weeklyRate, weightTrend,
    type WeighIn,
} from "../../utils/bodyMath.ts";

export type BodySeries = "weight" | "bodyFat" | "lean";

const DAY = 86_400_000;

export interface SeriesPoint {
    date: string;
    /** Local midnight, ms: for the chart's time axis. */
    t: number;
    /** Whole days since the epoch: for the maths (gaps are plain differences). */
    day: number;
    raw: number;
    trend: number;
}

export const SERIES: Record<BodySeries, { label: string; unit: string; alpha: number }> = {
    weight: { label: "Weight", unit: "kg", alpha: TREND_ALPHA },
    bodyFat: { label: "Body fat", unit: "%", alpha: BODY_FAT_TREND_ALPHA },
    lean: { label: "Lean mass", unit: "kg", alpha: BODY_FAT_TREND_ALPHA },
};

/** Body fat and lean mass need at least this many readings before they're shown. */
export const MIN_BODY_FAT_READINGS = 2;

function parts(date: string): [number, number, number] {
    const [y, m, d] = date.split("-").map(Number);
    return [y, m, d];
}
export const dayIndex = (date: string) => { const [y, m, d] = parts(date); return Math.round(Date.UTC(y, m - 1, d) / DAY); };
export const localTime = (date: string) => { const [y, m, d] = parts(date); return new Date(y, m - 1, d).getTime(); };

function valueOf(e: BodyEntry, series: BodySeries): number | null {
    if (series === "weight") return e.weightKg;
    if (e.bodyFatPct == null) return null;
    return series === "bodyFat" ? e.bodyFatPct : bodyComposition(e.weightKg, e.bodyFatPct).leanKg;
}

/**
 * Every reading of a series with its trend, oldest first. The trend runs over the
 * whole log, so a period's first point already carries the history before it.
 */
export function buildSeries(entries: BodyEntry[], series: BodySeries): SeriesPoint[] {
    const rows = entries
        .map(e => ({ e, v: valueOf(e, series) }))
        .filter((r): r is { e: BodyEntry; v: number } => r.v !== null)
        .sort((a, b) => a.e.date.localeCompare(b.e.date));
    const trend = weightTrend(rows.map(r => ({ day: dayIndex(r.e.date), kg: r.v })), SERIES[series].alpha);
    return rows.map((r, i) => ({ date: r.e.date, t: localTime(r.e.date), day: dayIndex(r.e.date), raw: r.v, trend: trend[i] }));
}

/** How far before a period's first reading the change baseline may be. */
export const BASELINE_MAX_GAP_DAYS = 14;

export interface SeriesRate {
    value: number;
    /** "week" for weight, "month" for body fat and lean mass. */
    per: "week" | "month";
    /** Weight only: the rate as % of body weight per week. */
    pct?: number;
}

export interface SeriesStats {
    points: SeriesPoint[];
    /** Trend at the last reading in the period. */
    now: number;
    /** Trend change over the period: from the reading just before it (or its first) to its last. */
    change: number | null;
    rate: SeriesRate | null;
    low: number;
    high: number;
}

/** The period's readings and the stat-row numbers, or null when the period has none. */
export function seriesStats(all: SeriesPoint[], series: BodySeries, start: Date, end: Date): SeriesStats | null {
    // Readings are dated by calendar day: one on the period's first day counts in full.
    const from = new Date(start.getFullYear(), start.getMonth(), start.getDate()).getTime(), to = end.getTime();
    const points = all.filter(p => p.t >= from && p.t <= to);
    if (!points.length) return null;

    // The baseline is the reading just before the period, but only if it's recent: a
    // weigh-in from months earlier says nothing about this period's change.
    const prior = all.filter(p => p.t < points[0].t).at(-1);
    const before = prior && points[0].day - prior.day <= BASELINE_MAX_GAP_DAYS ? prior : undefined;
    const base = before ?? points[0];
    const last = points.at(-1)!;
    const change = before || points.length > 1 ? last.trend - base.trend : null;

    const weighIns: WeighIn[] = points.map(p => ({ day: p.day, kg: p.raw }));
    let rate: SeriesRate | null = null;
    if (series === "weight") {
        const r = weeklyRate(weighIns);
        if (r) rate = { value: r.kgPerWeek, per: "week", pct: r.pctPerWeek };
    } else if (series === "bodyFat") {
        const r = bodyFatRatePerMonth(weighIns);
        if (r !== null) rate = { value: r, per: "month" };
    } else {
        const r = weeklyRate(weighIns, BODY_FAT_RATE_MIN);
        if (r) rate = { value: (r.kgPerWeek / 7) * 30, per: "month" };
    }

    const trends = points.map(p => p.trend);
    return { points, now: last.trend, change, rate, low: Math.min(...trends), high: Math.max(...trends) };
}
