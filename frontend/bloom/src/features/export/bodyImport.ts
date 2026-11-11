// Reads body log days out of a file: the "Body" sheet of a Bloom export, the
// body array of a Bloom backup, or a scale / health app's export (any table
// with a date and a weight column and no exercise column).

import { dayOf, detectDateOrder, timeOf, type DateOrder } from "./dates.ts";

export interface ParsedBody {
    /** yyyy-MM-dd */
    date: string;
    weightKg: number;
    bodyFatPct: number | null;
}

export type BodyUnit = "kg" | "lb";

export interface BodyParse {
    days: ParsedBody[];
    warnings: string[];
    /** How weights were read. `ambiguous` when the header has no unit and the values could be either. */
    unit: { used: BodyUnit; ambiguous: boolean; median: number };
    dateOrder: { used: DateOrder; ambiguous: boolean; sample?: string };
    /** Readings dropped because an earlier one on the same day was kept. */
    sameDay: number;
}

export const LB = 0.45359237;
const MIN_KG = 20, MAX_KG = 400, MIN_BF = 3, MAX_BF = 70;
/** With no unit in the header, a median above this is almost surely pounds (no adult median is 140 kg). */
const LIKELY_LB = 140;
/** Below this, a median can't be pounds (90 lb = 41 kg is still possible, but rare enough to ask). */
const SURELY_KG = 90;

type Cell = string | number;

const norm = (h: Cell) => String(h ?? "").trim().toLowerCase();
const OTHER_MASS = /fat|muscle|bone|water|lean|visceral|protein|bmi|bmr|goal|target|change|diff/;

function findColumns(header: string[]) {
    const date = header.findIndex(h => /^(date|day|datum|dag|time|timestamp|date ?\/ ?time|date and time|datetime|measured|measurement date)/.test(h) || /\bdate\b/.test(h));
    const weight = header.findIndex(h =>
        (/weight|gewicht|\bmass\b|^kg$|^lbs?$|^kilograms?$|^pounds?$/.test(h)) && !OTHER_MASS.test(h));
    const fatPct = header.findIndex(h => /fat/.test(h) && !/mass|kg|lb|weight/.test(h) && !/visceral/.test(h));
    const fatMass = header.findIndex(h => /fat/.test(h) && /mass|kg|lb|weight/.test(h) && !/free|visceral/.test(h));
    const exercise = header.findIndex(h => h === "exercise" || h === "exercise name");
    return { date, weight, fatPct, fatMass, exercise };
}

/** A table is a body log when it has a date and a weight column and no exercise column. */
export function isBodyTable(header: Cell[]): boolean {
    const c = findColumns(header.map(norm));
    return c.date >= 0 && c.weight >= 0 && c.exercise < 0;
}

function unitOf(h: string): BodyUnit | null {
    if (/\blbs?\b|pound/.test(h)) return "lb";
    if (/\bkg\b|kilo/.test(h)) return "kg";
    return null;
}

const asNumber = (v: Cell): number | null => {
    if (v === null || v === undefined || v === "") return null;
    const n = typeof v === "number" ? v : Number(String(v).trim().replace(/\s*(kg|lbs?|%)$/i, "").replace(",", "."));
    return Number.isFinite(n) ? n : null;
};

const pad = (n: number) => String(n).padStart(2, "0");
const isoDay = (d: Date) => `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;

function median(values: number[]): number {
    if (!values.length) return 0;
    const s = [...values].sort((a, b) => a - b);
    const m = Math.floor(s.length / 2);
    return s.length % 2 ? s[m] : (s[m - 1] + s[m]) / 2;
}

/** Body fat as a percentage: 0.184 (a fraction) reads as 18.4%. */
const asPercent = (v: number) => (v > 0 && v <= 1 ? v * 100 : v);
const round = (n: number, d: number) => Math.round(n * 10 ** d) / 10 ** d;

/**
 * Body log days from a table. One reading per day: the earliest when the date
 * cell has a time (scales log every weigh-in; the morning one is the consistent
 * one), otherwise the first in the file.
 */
export function parseBodyTable(rows: Cell[][], options: { dateOrder?: DateOrder; unit?: BodyUnit } = {}): BodyParse {
    const header = rows[0].map(norm);
    const col = findColumns(header);
    const body = rows.slice(1);

    const detected = detectDateOrder(body.map(r => r[col.date] ?? ""));
    const order: DateOrder = options.dateOrder ?? detected.order ?? "dmy";

    const raw = body.map(r => asNumber(r[col.weight] ?? "")).filter((n): n is number => n !== null && n > 0);
    const med = median(raw);
    const headerUnit = unitOf(header[col.weight]);
    const guessed: BodyUnit = med > LIKELY_LB ? "lb" : "kg";
    const unit: BodyUnit = options.unit ?? headerUnit ?? guessed;
    const ambiguous = !headerUnit && med >= SURELY_KG && med <= LIKELY_LB * 1.6 && !options.unit;
    const toKg = (v: number) => (unit === "lb" ? v * LB : v);
    const fatMassUnit = col.fatMass >= 0 ? unitOf(header[col.fatMass]) ?? unit : unit;

    const warnings: string[] = [];
    const byDay = new Map<string, { minutes: number; day: ParsedBody }>();
    let sameDay = 0;

    body.forEach((r, n) => {
        const line = n + 2;
        const cell = r[col.date] ?? "";
        const date = dayOf(cell, order);
        const w = asNumber(r[col.weight] ?? "");
        if (!date || w === null) {
            if (String(cell).trim() || w !== null) warnings.push(`Row ${line}: missing a date or weight, skipped.`);
            return;
        }
        const kg = round(toKg(w), 2);
        if (kg < MIN_KG || kg > MAX_KG) { warnings.push(`Row ${line}: ${w} ${unit} is outside ${MIN_KG}–${MAX_KG} kg, skipped.`); return; }

        let bf: number | null = null;
        const pct = col.fatPct >= 0 ? asNumber(r[col.fatPct] ?? "") : null;
        const fatMass = col.fatMass >= 0 ? asNumber(r[col.fatMass] ?? "") : null;
        if (pct !== null && pct > 0) bf = round(asPercent(pct), 1);
        else if (fatMass !== null && fatMass > 0) bf = round(((fatMassUnit === "lb" ? fatMass * LB : fatMass) / kg) * 100, 1);
        if (bf !== null && (bf < MIN_BF || bf > MAX_BF)) {
            warnings.push(`Row ${line}: body fat ${bf}% is outside ${MIN_BF}–${MAX_BF}%, kept the weight only.`);
            bf = null;
        }

        // A time in the date cell (or a time column) decides which reading of the day is kept.
        const hasTime = typeof cell === "number" ? cell % 1 !== 0 : /\d{1,2}:\d{2}/.test(String(cell));
        const [h, m] = hasTime ? timeOf(typeof cell === "number" ? cell % 1 : String(cell)) : [0, 0];
        const minutes = hasTime ? h * 60 + m : Number.POSITIVE_INFINITY;
        const key = isoDay(date);
        const prev = byDay.get(key);
        if (prev) {
            sameDay++;
            if (minutes >= prev.minutes) return;
        }
        byDay.set(key, { minutes, day: { date: key, weightKg: kg, bodyFatPct: bf } });
    });

    return {
        days: [...byDay.values()].map(v => v.day).sort((a, b) => a.date.localeCompare(b.date)),
        warnings,
        unit: { used: unit, ambiguous, median: round(med, 1) },
        dateOrder: { used: order, ambiguous: detected.slashDates && detected.order === null, sample: detected.sample },
        sameDay,
    };
}

/** The `body` array of a Bloom backup (always kg). */
export function parseBodyJson(value: unknown): { days: ParsedBody[]; warnings: string[] } {
    if (!Array.isArray(value)) return { days: [], warnings: [] };
    const warnings: string[] = [];
    const seen = new Set<string>();
    const days: ParsedBody[] = [];
    value.forEach((e: { date?: unknown; weightKg?: unknown; bodyFatPct?: unknown }, i) => {
        const date = typeof e?.date === "string" && /^\d{4}-\d{2}-\d{2}$/.test(e.date) ? e.date : null;
        const kg = typeof e?.weightKg === "number" ? e.weightKg : null;
        if (!date || kg === null || kg < MIN_KG || kg > MAX_KG) { warnings.push(`Body log entry ${i + 1}: missing or invalid date or weight, skipped.`); return; }
        if (seen.has(date)) return;
        seen.add(date);
        const bf = typeof e.bodyFatPct === "number" && e.bodyFatPct >= MIN_BF && e.bodyFatPct <= MAX_BF ? e.bodyFatPct : null;
        days.push({ date, weightKg: kg, bodyFatPct: bf });
    });
    return { days: days.sort((a, b) => a.date.localeCompare(b.date)), warnings };
}
