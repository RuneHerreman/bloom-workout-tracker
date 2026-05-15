export interface GpxTrackPoint {
    lat: number;
    lon: number;
    ele?: number;
    hr?: number;
    cad?: number;
    speedKph?: number;
    distanceKm: number;
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

        let distanceM = 0;
        let elevationGainM = 0;
        let prevLat: number | null = null;
        let prevLon: number | null = null;
        let prevEle: number | null = null;

        for (const pt of points) {
            const lat = parseFloat(pt.getAttribute("lat") ?? "");
            const lon = parseFloat(pt.getAttribute("lon") ?? "");
            const eleText = pt.querySelector("ele")?.textContent;
            const ele = eleText != null ? parseFloat(eleText) : NaN;

            if (prevLat !== null && prevLon !== null && !isNaN(lat) && !isNaN(lon)) {
                distanceM += haversineM(prevLat, prevLon, lat, lon);
            }
            if (prevEle !== null && !isNaN(ele)) {
                const diff = ele - prevEle;
                if (diff > 0) elevationGainM += diff;
            }

            if (!isNaN(lat)) prevLat = lat;
            if (!isNaN(lon)) prevLon = lon;
            if (!isNaN(ele)) prevEle = ele;
        }

        const firstTime = points[0].querySelector("time")?.textContent;
        const lastTime = points[points.length - 1].querySelector("time")?.textContent;
        const durationMs =
            firstTime && lastTime
                ? new Date(lastTime).getTime() - new Date(firstTime).getTime()
                : 0;

        return { distanceKm: distanceM / 1000, elevationGainM, durationMs };
    } catch {
        return null;
    }
}

function getExtField(pt: Element, name: string): number | undefined {
    const el = pt.getElementsByTagNameNS("*", name)[0];
    if (!el) return undefined;
    const v = parseFloat(el.textContent ?? "");
    return isNaN(v) ? undefined : v;
}

function smoothSpeed(points: GpxTrackPoint[], window = 5): void {
    const speeds = points.map(p => p.speedKph);
    for (let i = 0; i < points.length; i++) {
        const lo = Math.max(0, i - window);
        const hi = Math.min(points.length - 1, i + window);
        const vals = speeds.slice(lo, hi + 1).filter((v): v is number => v !== undefined);
        if (vals.length > 0) points[i].speedKph = vals.reduce((a, b) => a + b, 0) / vals.length;
    }
}

export function parseGpxTrackPoints(xml: string): GpxTrackPoint[] {
    try {
        const doc = new DOMParser().parseFromString(xml, "application/xml");
        if (doc.querySelector("parsererror")) return [];
        const rawPts = Array.from(doc.querySelectorAll("trkpt"));
        const points: GpxTrackPoint[] = [];
        let cumDist = 0;
        let prev: { lat: number; lon: number; timeMs: number | null } | null = null;

        for (const pt of rawPts) {
            const lat = parseFloat(pt.getAttribute("lat") ?? "");
            const lon = parseFloat(pt.getAttribute("lon") ?? "");
            if (isNaN(lat) || isNaN(lon)) continue;

            const timeText = pt.querySelector("time")?.textContent;
            const timeMs = timeText ? new Date(timeText).getTime() : null;

            let segDistKm = 0;
            if (prev) {
                segDistKm = haversineM(prev.lat, prev.lon, lat, lon) / 1000;
                cumDist += segDistKm;
            }

            const eleText = pt.querySelector("ele")?.textContent;
            const ele = eleText != null ? parseFloat(eleText) : undefined;

            let speedKph: number | undefined;
            if (prev?.timeMs != null && timeMs != null) {
                const dtH = (timeMs - prev.timeMs) / 3_600_000;
                if (dtH > 0) speedKph = segDistKm / dtH;
            }

            points.push({
                lat, lon,
                distanceKm: cumDist,
                ...(ele !== undefined && !isNaN(ele) ? { ele } : {}),
                ...(getExtField(pt, "hr")  !== undefined ? { hr:  getExtField(pt, "hr")  } : {}),
                ...(getExtField(pt, "cad") !== undefined ? { cad: getExtField(pt, "cad") } : {}),
                ...(speedKph !== undefined ? { speedKph } : {}),
            });
            prev = { lat, lon, timeMs };
        }

        smoothSpeed(points);
        return points;
    } catch {
        return [];
    }
}

export function formatDuration(ms: number): string {
    const totalSec = Math.round(ms / 1000);
    const h = Math.floor(totalSec / 3600);
    const m = Math.floor((totalSec % 3600) / 60);
    const s = totalSec % 60;
    if (h > 0) return `${h}h ${m}m`;
    if (m > 0) return `${m}m ${s}s`;
    return `${s}s`;
}