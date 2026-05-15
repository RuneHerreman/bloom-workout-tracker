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

export function formatDuration(ms: number): string {
    const totalSec = Math.round(ms / 1000);
    const h = Math.floor(totalSec / 3600);
    const m = Math.floor((totalSec % 3600) / 60);
    const s = totalSec % 60;
    if (h > 0) return `${h}h ${m}m`;
    if (m > 0) return `${m}m ${s}s`;
    return `${s}s`;
}