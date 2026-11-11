import { describe, expect, it } from "vitest";
import type { BodyEntry } from "../../assets/js/data/apiTypes.ts";
import { buildSeries, dayIndex, seriesStats } from "./bodySeries.ts";

const entry = (date: string, weightKg: number, bodyFatPct: number | null = null): BodyEntry =>
    ({ date, weightKg, bodyFatPct, source: "Manual" });

/** `n` daily entries from 1 Oct 2026, weight from `start` changing by `perDay`. */
const daily = (n: number, start: number, perDay: number, bf?: (i: number) => number | null): BodyEntry[] =>
    Array.from({ length: n }, (_, i) => {
        const d = new Date(2026, 9, 1 + i);
        const date = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
        return entry(date, Math.round((start + perDay * i) * 100) / 100, bf ? bf(i) : null);
    });

describe("dayIndex", () => {
    it("counts whole days, across month ends and DST changes", () => {
        expect(dayIndex("2026-10-26") - dayIndex("2026-10-24")).toBe(2); // DST ends 25 Oct in Europe
        expect(dayIndex("2026-03-01") - dayIndex("2026-02-28")).toBe(1);
    });
});

describe("buildSeries", () => {
    it("sorts by date and trends over the whole log", () => {
        const s = buildSeries([entry("2026-10-03", 83), entry("2026-10-01", 80), entry("2026-10-02", 80)], "weight");
        expect(s.map(p => p.date)).toEqual(["2026-10-01", "2026-10-02", "2026-10-03"]);
        expect(s.at(-1)!.trend).toBeCloseTo(80.3, 9);
    });

    it("only uses entries with a body fat reading for body fat and lean mass", () => {
        const entries = [entry("2026-10-01", 80, 20), entry("2026-10-02", 81), entry("2026-10-03", 80, 19)];
        expect(buildSeries(entries, "bodyFat").map(p => p.raw)).toEqual([20, 19]);
        expect(buildSeries(entries, "lean").map(p => p.raw)).toEqual([64, 64.8]);
    });
});

describe("seriesStats", () => {
    it("reports the trend, its change over the period, and a weekly rate", () => {
        const all = buildSeries(daily(40, 85, -0.1), "weight");
        const start = new Date(2026, 9, 11, 21, 30), end = new Date(2026, 10, 9, 12);
        const s = seriesStats(all, "weight", start, end)!;

        expect(s.points[0].date).toBe("2026-10-11"); // the period starts at 21:30, but its first day still counts
        expect(s.now).toBe(all.at(-1)!.trend);
        // The baseline is the last reading before the period.
        expect(s.change).toBeCloseTo(all.at(-1)!.trend - all[9].trend, 9);
        expect(s.rate!.per).toBe("week");
        expect(s.rate!.value).toBeCloseTo(-0.7, 6);
        expect(s.low).toBeLessThan(s.high);
    });

    it("ignores a baseline from long before the period", () => {
        // One weigh-in two years back, then a month of steady loss.
        const all = buildSeries([entry("2024-09-02", 80), ...daily(30, 82, -0.05)], "weight");
        const s = seriesStats(all, "weight", new Date(2026, 9, 1), new Date(2026, 10, 9))!;
        expect(s.change).toBeCloseTo(s.points.at(-1)!.trend - s.points[0].trend, 9);
        expect(s.change!).toBeLessThan(0);
    });

    it("has no rate until there are enough weigh-ins, and no change from a single reading", () => {
        const all = buildSeries([entry("2026-11-01", 80)], "weight");
        const s = seriesStats(all, "weight", new Date(2026, 9, 1), new Date(2026, 10, 9))!;
        expect(s.change).toBeNull();
        expect(s.rate).toBeNull();
    });

    it("is null for a period without readings", () => {
        const all = buildSeries([entry("2026-01-01", 80)], "weight");
        expect(seriesStats(all, "weight", new Date(2026, 9, 1), new Date(2026, 10, 9))).toBeNull();
    });

    it("gives body fat a monthly rate once there are 4 readings over 28 days", () => {
        const weekly = daily(36, 80, 0, i => (i % 7 === 0 ? 20 - i * 0.02 : null));
        const s = seriesStats(buildSeries(weekly, "bodyFat"), "bodyFat", new Date(2026, 8, 1), new Date(2026, 10, 9))!;
        expect(s.points).toHaveLength(6);
        expect(s.rate!.per).toBe("month");
        expect(s.rate!.value).toBeCloseTo(-0.02 * 30, 6);
    });
});
