export interface GpxTrackPoint {
    lat: number;
    lon: number;
    distanceKm: number;
    elapsedMs?: number;
    ele?: number;
    hr?: number;
    cad?: number;
    speedKph?: number;
    power?: number;
    atemp?: number;
    grade?: number;
}

export interface KmSplit {
    km: number;
    durationMs: number;
    pace: number; // min/km
    avgHr?: number;
}

export interface GpxStats {
    distanceKm: number;
    elevationGainM: number;
    durationMs: number;
}

function haversineM(lat1: number, lon1: number, lat2: number, lon2: number): number {
    const R = 6371000;
    const toRad = (d: number) => (d * Math.PI) / 180;
    const dLat = toRad(lat2 - lat1);
    const dLon = toRad(lon2 - lon1);
    const a =
        Math.sin(dLat / 2) ** 2 +
        Math.cos(toRad(lat1)) * Math.cos(toRad(lat2)) * Math.sin(dLon / 2) ** 2;
    return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
}

export function parseGpx(xml: string): GpxStats | null {
    try {
        const doc = new DOMParser().parseFromString(xml, "application/xml");
        if (doc.querySelector("parsererror")) return null;

        const points = Array.from(doc.querySelectorAll("trkpt"));
        if (points.length < 2) return null;

        let distanceM = 0, elevationGainM = 0;
        let prevLat: number | null = null, prevLon: number | null = null, prevEle: number | null = null;

        for (const pt of points) {
            const lat = parseFloat(pt.getAttribute("lat") ?? "");
            const lon = parseFloat(pt.getAttribute("lon") ?? "");
            const ele = parseFloat(pt.querySelector("ele")?.textContent ?? "");
            if (prevLat !== null && prevLon !== null && !isNaN(lat) && !isNaN(lon))
                distanceM += haversineM(prevLat, prevLon, lat, lon);
            if (prevEle !== null && !isNaN(ele) && ele - prevEle > 0)
                elevationGainM += ele - prevEle;
            if (!isNaN(lat)) prevLat = lat;
            if (!isNaN(lon)) prevLon = lon;
            if (!isNaN(ele)) prevEle = ele;
        }

        const firstTime = points[0].querySelector("time")?.textContent;
        const lastTime  = points[points.length - 1].querySelector("time")?.textContent;
        const durationMs = firstTime && lastTime
            ? new Date(lastTime).getTime() - new Date(firstTime).getTime() : 0;

        return { distanceKm: distanceM / 1000, elevationGainM, durationMs };
    } catch { return null; }
}

function getExt(pt: Element, ...names: string[]): number | undefined {
    for (const name of names) {
        const el = pt.getElementsByTagNameNS("*", name)[0];
        if (el) {
            const v = parseFloat(el.textContent ?? "");
            if (!isNaN(v)) return v;
        }
    }
    return undefined;
}

function smooth(points: GpxTrackPoint[], key: keyof GpxTrackPoint, window: number): void {
    const vals = points.map(p => p[key] as number | undefined);
    for (let i = 0; i < points.length; i++) {
        const lo = Math.max(0, i - window), hi = Math.min(points.length - 1, i + window);
        const slice = vals.slice(lo, hi + 1).filter((v): v is number => v !== undefined);
        if (slice.length > 0)
            (points[i] as unknown as Record<string, unknown>)[key] = slice.reduce((a, b) => a + b, 0) / slice.length;
    }
}

export function parseGpxTrackPoints(xml: string): GpxTrackPoint[] {
    try {
        const doc = new DOMParser().parseFromString(xml, "application/xml");
        if (doc.querySelector("parsererror")) return [];
        const rawPts = Array.from(doc.querySelectorAll("trkpt"));
        const points: GpxTrackPoint[] = [];
        let cumDist = 0;
        let firstTimeMs: number | null = null;
        let prev: { lat: number; lon: number; ele: number | undefined; timeMs: number | null } | null = null;

        for (const pt of rawPts) {
            const lat = parseFloat(pt.getAttribute("lat") ?? "");
            const lon = parseFloat(pt.getAttribute("lon") ?? "");
            if (isNaN(lat) || isNaN(lon)) continue;

            const timeText = pt.querySelector("time")?.textContent;
            const timeMs   = timeText ? new Date(timeText).getTime() : null;
            if (timeMs !== null && firstTimeMs === null) firstTimeMs = timeMs;
            const eleRaw   = pt.querySelector("ele")?.textContent;
            const ele      = eleRaw != null ? parseFloat(eleRaw) : undefined;

            let segDistKm = 0;
            if (prev) { segDistKm = haversineM(prev.lat, prev.lon, lat, lon) / 1000; cumDist += segDistKm; }

            let speedKph: number | undefined;
            if (prev?.timeMs != null && timeMs != null) {
                const dtH = (timeMs - prev.timeMs) / 3_600_000;
                if (dtH > 0) speedKph = segDistKm / dtH;
            }

            let grade: number | undefined;
            if (prev?.ele !== undefined && ele !== undefined && !isNaN(ele) && segDistKm > 0)
                grade = ((ele - prev.ele) / (segDistKm * 1000)) * 100;

            const elapsedMs = firstTimeMs !== null && timeMs !== null ? timeMs - firstTimeMs : undefined;

            points.push({
                lat, lon, distanceKm: cumDist,
                ...(elapsedMs !== undefined        ? { elapsedMs }                                           : {}),
                ...(ele !== undefined && !isNaN(ele) ? { ele } : {}),
                ...(speedKph !== undefined          ? { speedKph } : {}),
                ...(grade    !== undefined          ? { grade }    : {}),
                ...(getExt(pt, "hr")               !== undefined ? { hr:    getExt(pt, "hr") }               : {}),
                ...(getExt(pt, "cad")              !== undefined ? { cad:   getExt(pt, "cad") }              : {}),
                ...(getExt(pt, "atemp")            !== undefined ? { atemp: getExt(pt, "atemp") }            : {}),
                ...(getExt(pt, "watt", "power", "PowerInWatts") !== undefined
                    ? { power: getExt(pt, "watt", "power", "PowerInWatts") } : {}),
            });
            const prevEle = prev?.ele;
            prev = { lat, lon, ele: ele !== undefined && !isNaN(ele) ? ele : prevEle, timeMs };
        }

        smooth(points, "speedKph", 5);
        smooth(points, "grade",    15);
        return points;
    } catch { return []; }
}

export function formatDuration(ms: number): string {
    const s = Math.round(ms / 1000);
    const h = Math.floor(s / 3600), m = Math.floor((s % 3600) / 60), sec = s % 60;
    if (h > 0) return `${h}h ${m}m`;
    if (m > 0) return `${m}m ${sec}s`;
    return `${sec}s`;
}

export function formatPace(minPerKm: number): string {
    if (!isFinite(minPerKm) || minPerKm <= 0 || minPerKm > 60) return "--";
    const min = Math.floor(minPerKm);
    const sec = Math.round((minPerKm - min) * 60);
    return `${min}:${sec.toString().padStart(2, "0")}`;
}

export function computeKmSplits(points: GpxTrackPoint[]): KmSplit[] {
    if (points.length < 2) return [];
    const maxKm = Math.floor(points[points.length - 1].distanceKm);
    const splits: KmSplit[] = [];

    for (let k = 1; k <= maxKm; k++) {
        const seg = points.filter(p => p.distanceKm >= k - 1 && p.distanceKm < k);
        if (seg.length < 2) continue;

        const first = seg[0], last = seg[seg.length - 1];
        const actualDist = last.distanceKm - first.distanceKm;
        if (actualDist <= 0) continue;

        let durationMs: number;
        if (first.elapsedMs !== undefined && last.elapsedMs !== undefined) {
            durationMs = last.elapsedMs - first.elapsedMs;
        } else {
            const speeds = seg.map(p => p.speedKph).filter((v): v is number => v !== undefined && v > 0.5);
            if (speeds.length === 0) continue;
            const avgKph = speeds.reduce((a, b) => a + b, 0) / speeds.length;
            durationMs = (actualDist / avgKph) * 3_600_000;
        }

        if (durationMs <= 0) continue;
        const pace = (durationMs / 60000) / actualDist;
        const hrs = seg.map(p => p.hr).filter((v): v is number => v !== undefined);
        const avgHr = hrs.length ? Math.round(hrs.reduce((a, b) => a + b, 0) / hrs.length) : undefined;

        splits.push({ km: k, durationMs, pace, avgHr });
    }
    return splits;
}
