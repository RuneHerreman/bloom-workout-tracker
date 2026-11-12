import { describe, expect, it } from "vitest";
import { daysAgo } from "./workoutUtils.ts";

/** A local date-time as an ISO string, the way logs store `loggedAt`. */
const at = (y: number, m: number, d: number, h: number, min = 0) => new Date(y, m - 1, d, h, min).toISOString();

describe("daysAgo", () => {
    const morning = new Date(2026, 10, 12, 8, 0);

    it("counts last night's workout as yesterday, though it was under 24 hours ago", () => {
        expect(daysAgo(at(2026, 11, 11, 20, 30), morning)).toBe(1);
    });

    it("counts earlier the same day as today", () => {
        expect(daysAgo(at(2026, 11, 12, 6, 15), morning)).toBe(0);
    });

    it("counts calendar days further back", () => {
        expect(daysAgo(at(2026, 11, 5, 23, 59), morning)).toBe(7);
    });

    it("is negative for a future date", () => {
        expect(daysAgo(at(2026, 11, 13, 9), morning)).toBe(-1);
    });

    it("isn't thrown off by a daylight-saving change", () => {
        // Clocks go back on 25 Oct 2026 in Europe: that day is 25 hours long.
        expect(daysAgo(at(2026, 10, 24, 12), new Date(2026, 9, 26, 12))).toBe(2);
    });
});
