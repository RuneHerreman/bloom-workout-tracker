// Time and value axes shared by the Insights line charts.

const DAY = 86_400_000;

/** ~6 evenly spaced ticks on week or month boundaries. */
export function timeTicks(start: Date, end: Date): number[] {
    const span = end.getTime() - start.getTime();
    const ticks: number[] = [];
    if (span <= 70 * DAY) {
        const step = span <= 35 * DAY ? 7 : 14;
        for (let t = start.getTime(); t <= end.getTime(); t += step * DAY) ticks.push(t);
    } else {
        const months = Math.max(1, Math.round(span / (30.44 * DAY) / 6));
        for (let d = new Date(start.getFullYear(), start.getMonth() + 1, 1); d <= end; d = new Date(d.getFullYear(), d.getMonth() + months, 1)) {
            ticks.push(d.getTime());
        }
    }
    return ticks;
}

/** Month ticks carry the year on the first tick and whenever it changes: "Oct '24  Feb '25  Jun  Oct". */
export function formatTick(t: number, prev: number | undefined, spanDays: number): string {
    const d = new Date(t);
    if (spanDays <= 70) return d.toLocaleDateString("en-GB", { day: "numeric", month: "short" });
    const month = d.toLocaleDateString("en-GB", { month: "short" });
    const showYear = spanDays > 300 && (prev === undefined || new Date(prev).getFullYear() !== d.getFullYear());
    return showYear ? `${month} ’${String(d.getFullYear()).slice(2)}` : month;
}

/** Rounds the value range out to 1/2/2.5/5 × 10ⁿ steps so ticks read 60, 80, 100… */
export function niceAxis(min: number, max: number, count = 4): { domain: [number, number]; ticks: number[] } {
    const raw = Math.max((max - min) / count, 1e-6);
    const mag = 10 ** Math.floor(Math.log10(raw));
    const step = [1, 2, 2.5, 5, 10].map(m => m * mag).find(s => s >= raw) ?? 10 * mag;
    const lo = Math.max(0, Math.floor(min / step) * step);
    const hi = Math.ceil(max / step) * step;
    const ticks: number[] = [];
    for (let v = lo; v <= hi + step / 2; v += step) ticks.push(Math.round(v * 100) / 100);
    return { domain: [lo, hi], ticks };
}
