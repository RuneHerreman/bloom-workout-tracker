import { describe, it, expect } from "vitest";
import { prefillSetFromGpx } from "./gpxUtils.ts";

const stats = { distanceKm: 17.9634, elevationGainM: 1533, durationMs: (1 * 3600 + 58 * 60 + 7) * 1000 };
const empty = { distance: null, distanceUnit: null, duration: null };

describe("prefillSetFromGpx", () => {
    it("fills empty distance and duration", () => {
        expect(prefillSetFromGpx(empty, stats)).toEqual({ distance: 17.963, distanceUnit: "km", duration: "01:58:07" });
    });

    it("keeps fields the user already filled in", () => {
        const set = { distance: 5, distanceUnit: "km", duration: null };
        expect(prefillSetFromGpx(set, stats)).toEqual({ distance: 5, distanceUnit: "km", duration: "01:58:07" });
    });

    it("leaves fields empty when the track has no data for them", () => {
        expect(prefillSetFromGpx(empty, { distanceKm: 0, elevationGainM: 0, durationMs: 0 })).toEqual(empty);
    });
});
