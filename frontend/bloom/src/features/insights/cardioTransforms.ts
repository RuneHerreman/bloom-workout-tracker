import type { LoggedWorkout } from "../../assets/js/data/apiTypes.ts";
import { parseGpxTrackPoints } from "../logbook/gpxUtils.ts";

// ── HR Zones ──────────────────────────────────────────────────────────────────

export const ZONE_COLORS = ["#3B7FC4", "#2D9C8C", "#F5B800", "#E9762B", "#C0392B"];
export const ZONE_LABELS = ["Z1 Easy", "Z2 Aerobic", "Z3 Tempo", "Z4 Threshold", "Z5 Max"];
export const ZONE_THRESHOLDS = [0.6, 0.7, 0.8, 0.9, 1.0]; // upper bounds as fraction of maxHR

export function estimateMaxHr(birthDateStr: string): number {
    const birth = new Date(birthDateStr);
    const today = new Date();
    const age = today.getFullYear() - birth.getFullYear() -
        (today < new Date(today.getFullYear(), birth.getMonth(), birth.getDate()) ? 1 : 0);
    return 220 - age;
}

function hrZoneIndex(hr: number, maxHr: number): number {
    const frac = hr / maxHr;
    if (frac < 0.6) return 0;
    if (frac < 0.7) return 1;
    if (frac < 0.8) return 2;
    if (frac < 0.9) return 3;
    return 4;
}

export interface HrZoneSecs {
    zone: number;
    label: string;
    color: string;
    seconds: number;
}

export interface SessionZoneData {
    label: string;
    date: string;
    zones: number[]; // seconds per zone (length 5)
    totalSecs: number;
}

export interface HrZoneResult {
    overall: HrZoneSecs[];
    sessions: SessionZoneData[];
    hasData: boolean;
}

export function buildHrZoneData(logs: LoggedWorkout[], maxHr: number): HrZoneResult {
    const overallSecs = [0, 0, 0, 0, 0];
    const sessions: SessionZoneData[] = [];

    for (const log of logs) {
        for (const ex of log.exercises) {
            if (!ex.gpxData) continue;
            const pts = parseGpxTrackPoints(ex.gpxData);
            const hrPts = pts.filter(p => p.hr !== undefined && p.elapsedMs !== undefined);
            if (hrPts.length < 2) continue;

            const zoneSecs = [0, 0, 0, 0, 0];
            for (let i = 1; i < hrPts.length; i++) {
                const avgHr = (hrPts[i].hr! + hrPts[i - 1].hr!) / 2;
                const dtSec = (hrPts[i].elapsedMs! - hrPts[i - 1].elapsedMs!) / 1000;
                if (dtSec <= 0 || dtSec > 300) continue;
                const zone = hrZoneIndex(avgHr, maxHr);
                zoneSecs[zone] += dtSec;
                overallSecs[zone] += dtSec;
            }

            const totalSecs = zoneSecs.reduce((a, b) => a + b, 0);
            if (totalSecs < 60) continue;

            const d = new Date(log.loggedAt);
            sessions.push({
                label: log.name || d.toLocaleDateString("en-GB", { day: "numeric", month: "short" }),
                date: log.loggedAt,
                zones: zoneSecs,
                totalSecs,
            });
        }
    }

    sessions.sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime());

    const overall: HrZoneSecs[] = ZONE_LABELS.map((label, i) => ({
        zone: i + 1,
        label,
        color: ZONE_COLORS[i],
        seconds: overallSecs[i],
    }));

    const hasData = overallSecs.some(s => s > 0);
    return { overall, sessions, hasData };
}

// ── ATL / CTL ─────────────────────────────────────────────────────────────────

export interface AtlCtlPoint {
    date: string;      // ISO date string
    load: number;
    atl: number;       // Acute Training Load (7-day EMA)
    ctl: number;       // Chronic Training Load (42-day EMA)
    form: number;      // CTL - ATL
}

function sessionLoad(log: LoggedWorkout, maxHr: number): number {
    let totalLoad = 0;

    for (const ex of log.exercises) {
        if (!ex.gpxData) continue;
        const pts = parseGpxTrackPoints(ex.gpxData);
        const hrPts = pts.filter(p => p.hr !== undefined && p.elapsedMs !== undefined);
        if (hrPts.length < 2) continue;

        let durationMin = 0;
        let hrSum = 0;
        let hrCount = 0;

        for (let i = 1; i < hrPts.length; i++) {
            const dtSec = (hrPts[i].elapsedMs! - hrPts[i - 1].elapsedMs!) / 1000;
            if (dtSec <= 0 || dtSec > 300) continue;
            durationMin += dtSec / 60;
            hrSum += (hrPts[i].hr! + hrPts[i - 1].hr!) / 2;
            hrCount++;
        }

        if (durationMin < 1 || hrCount === 0) continue;
        const avgHr = hrSum / hrCount;
        const intensity = (avgHr / maxHr) ** 2;
        totalLoad += durationMin * intensity;
    }

    return totalLoad;
}

export function buildAtlCtlSeries(logs: LoggedWorkout[], maxHr: number): AtlCtlPoint[] {
    if (logs.length === 0 || maxHr <= 0) return [];

    const loadByDate = new Map<string, number>();
    for (const log of logs) {
        const dateKey = log.loggedAt.slice(0, 10);
        const load = sessionLoad(log, maxHr);
        if (load > 0) loadByDate.set(dateKey, (loadByDate.get(dateKey) ?? 0) + load);
    }

    if (loadByDate.size === 0) return [];

    const allDates = Array.from(loadByDate.keys()).sort();
    const firstDate = new Date(allDates[0]);
    const lastDate = new Date(allDates[allDates.length - 1]);

    const kAtl = 1 - Math.exp(-1 / 7);
    const kCtl = 1 - Math.exp(-1 / 42);

    let atl = 0, ctl = 0;
    const result: AtlCtlPoint[] = [];
    const cur = new Date(firstDate);

    while (cur <= lastDate) {
        const key = cur.toISOString().slice(0, 10);
        const load = loadByDate.get(key) ?? 0;
        atl = atl + kAtl * (load - atl);
        ctl = ctl + kCtl * (load - ctl);
        result.push({ date: key, load, atl, ctl, form: ctl - atl });
        cur.setDate(cur.getDate() + 1);
    }

    return result;
}

// ── Route Heatmap ─────────────────────────────────────────────────────────────

export interface RoutePolyline {
    id: string;
    name: string;
    positions: [number, number][];
    date: string;
    distanceKm: number;
}

export function extractRoutePolylines(logs: LoggedWorkout[]): RoutePolyline[] {
    const routes: RoutePolyline[] = [];

    for (const log of logs) {
        for (let ei = 0; ei < log.exercises.length; ei++) {
            const ex = log.exercises[ei];
            if (!ex.gpxData) continue;
            const pts = parseGpxTrackPoints(ex.gpxData);
            if (pts.length < 2) continue;
            const distanceKm = pts[pts.length - 1].distanceKm;
            routes.push({
                id: `${log.id}-${ei}`,
                name: log.name || "Run",
                positions: pts.map(p => [p.lat, p.lon]),
                date: log.loggedAt,
                distanceKm,
            });
        }
    }

    return routes.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
}
