import { describe, expect, it } from "vitest";
import { isBodyTable, parseBodyJson, parseBodyTable } from "./bodyImport.ts";
import { parseCsv } from "./importData.ts";

const csv = (text: string) => parseCsv(text.trim());

describe("isBodyTable", () => {
    it.each([
        [["Date", "Weight (kg)", "Body fat (%)"], true],               // Bloom's own Body sheet
        [["Date", "Weight (kg)", "Fat mass (kg)", "Bone mass (kg)"], true], // Withings
        [["Time of Measurement", "Weight(kg)", "BMI", "Body Fat(%)"], true], // Renpho
        [["Date", "Weight", "BMI", "Fat"], true],                     // Fitbit
        [["Datum", "Gewicht"], true],
        [["Date", "Workout", "Exercise", "Weight"], false],            // Bloom sets: has an exercise column
        [["Date", "Muscle mass (kg)"], false],                         // no body weight column
        [["Name", "Weight"], false],                                   // no date
    ])("%j → %s", (header, expected) => {
        expect(isBodyTable(header)).toBe(expected);
    });
});

describe("parseBodyTable", () => {
    it("reads Bloom's Body sheet", () => {
        const r = parseBodyTable(csv(`
Date,Weight (kg),Body fat (%)
2026-10-01,81.2,19.5
2026-10-02,81,
`));
        expect(r.days).toEqual([
            { date: "2026-10-01", weightKg: 81.2, bodyFatPct: 19.5 },
            { date: "2026-10-02", weightKg: 81, bodyFatPct: null },
        ]);
        expect(r.unit).toMatchObject({ used: "kg", ambiguous: false });
        expect(r.warnings).toEqual([]);
    });

    it("converts pounds from the header, and fat mass to a percentage", () => {
        const r = parseBodyTable(csv(`
Date,Weight (lb),Fat mass (lb)
2026-10-01,180,36
`));
        expect(r.days[0].weightKg).toBeCloseTo(81.65, 2);
        expect(r.days[0].bodyFatPct).toBe(20);
        expect(r.unit.used).toBe("lb");
    });

    it("keeps the earliest reading of a day when the date has a time (Withings)", () => {
        const r = parseBodyTable(csv(`
Date,Weight (kg),Fat mass (kg)
"2026-10-01 21:40:02",82.4,
"2026-10-01 07:12:33",81.1,16.2
"2026-10-02 07:05:10",80.9,
`));
        expect(r.days).toEqual([
            { date: "2026-10-01", weightKg: 81.1, bodyFatPct: 20 },
            { date: "2026-10-02", weightKg: 80.9, bodyFatPct: null },
        ]);
        expect(r.sameDay).toBe(1);
    });

    it("reads body fat written as a fraction, and decimal commas", () => {
        const r = parseBodyTable(csv(`
Date,Weight
2026-10-01,"80,4"
`));
        expect(r.days[0].weightKg).toBe(80.4);
        const f = parseBodyTable(csv(`
Date,Weight,Fat
2026-10-01,80,0.184
`));
        expect(f.days[0].bodyFatPct).toBe(18.4);
    });

    it("guesses pounds from the values when the header has no unit, and asks when unsure", () => {
        const lb = parseBodyTable(csv(`
Date,Weight
2026-10-01,250
2026-10-02,249
`));
        expect(lb.unit).toMatchObject({ used: "lb", ambiguous: false });
        expect(lb.days[0].weightKg).toBeCloseTo(113.4, 1);

        const unsure = parseBodyTable(csv(`
Date,Weight
2026-10-01,165
2026-10-02,164
`));
        expect(unsure.unit).toMatchObject({ used: "lb", ambiguous: true });

        const clear = parseBodyTable(csv(`
Date,Weight
2026-10-01,72
`));
        expect(clear.unit).toMatchObject({ used: "kg", ambiguous: false });

        const chosen = parseBodyTable(csv(`
Date,Weight
2026-10-01,165
`), { unit: "kg" });
        expect(chosen.unit).toMatchObject({ used: "kg", ambiguous: false });
        expect(chosen.days[0].weightKg).toBe(165);
    });

    it("reports rows it can't use and drops only an impossible body fat", () => {
        const r = parseBodyTable(csv(`
Date,Weight (kg),Body fat (%)
2026-10-01,800,
,80,
2026-10-03,80,95
`));
        expect(r.days).toEqual([{ date: "2026-10-03", weightKg: 80, bodyFatPct: null }]);
        expect(r.warnings).toHaveLength(3);
    });

    it("reads day/month dates, and flags them when either order fits", () => {
        const dmy = parseBodyTable(csv(`
Date,Weight (kg)
25/10/2026,80
`));
        expect(dmy.days[0].date).toBe("2026-10-25");
        const unsure = parseBodyTable(csv(`
Date,Weight (kg)
02/11/2026,80
`));
        expect(unsure.dateOrder.ambiguous).toBe(true);
        expect(parseBodyTable(csv(`
Date,Weight (kg)
02/11/2026,80
`), { dateOrder: "mdy" }).days[0].date).toBe("2026-02-11");
    });
});

describe("parseBodyJson", () => {
    it("reads a backup's body array, skipping bad and repeated days", () => {
        const r = parseBodyJson([
            { date: "2026-10-01", weightKg: 81, bodyFatPct: 19 },
            { date: "2026-10-01", weightKg: 99 },
            { date: "2026-10-02", weightKg: 80.5, bodyFatPct: 120 },
            { date: "oops", weightKg: 80 },
        ]);
        expect(r.days).toEqual([
            { date: "2026-10-01", weightKg: 81, bodyFatPct: 19 },
            { date: "2026-10-02", weightKg: 80.5, bodyFatPct: null },
        ]);
        expect(r.warnings).toHaveLength(1);
    });

    it("is empty when there's no body array", () => {
        expect(parseBodyJson(undefined).days).toEqual([]);
    });
});
