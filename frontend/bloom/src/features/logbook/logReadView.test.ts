import { describe, it, expect } from "vitest";
import { setParts } from "./logbookUtils.ts";
import type { LoggedSet } from "../../assets/js/data/apiTypes.ts";

const base: LoggedSet = { type: "Strength", order: 1, reps: 8, weight: 60, weightUnit: "Kg", rir: null, duration: null, distance: null, distanceUnit: null };

describe("setParts", () => {
    it("leads strength sets with the weight", () => {
        expect(setParts(base)).toEqual({ value: "60", unit: "kg", detail: "× 8" });
        expect(setParts({ ...base, weight: 22.5, rir: 2 })).toEqual({ value: "22.5", unit: "kg", detail: "× 8 @2" });
        expect(setParts({ ...base, weight: null })).toEqual({ value: "8", unit: "reps", detail: "" });
    });

    it("leads cardio with the distance, then the time", () => {
        const run: LoggedSet = { ...base, type: "Cardio", reps: null, weight: null, distance: 5.56, distanceUnit: "km", duration: "00:50:00" };
        expect(setParts(run)).toEqual({ value: "5.56", unit: "km", detail: "50m 0s" });
        expect(setParts({ ...run, distance: 400, distanceUnit: "meters", duration: null })).toEqual({ value: "400", unit: "m", detail: "" });
        expect(setParts({ ...run, distance: null })).toEqual({ value: "50m 0s", unit: "", detail: "" });
    });
});
