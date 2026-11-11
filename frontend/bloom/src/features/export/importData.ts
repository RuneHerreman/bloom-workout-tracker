// Reads the files Bloom exports (JSON backup, CSV, Excel) back into workouts,
// templates and custom exercises, and plans how they map onto this account.

import type { ExerciseType } from "../../types.ts";
import type {
    Exercise, LoggedExercise, LoggedSet, LoggedWorkout, PlannedSet, SetMarker,
    TemplateExercise, WorkoutTemplate,
} from "../../assets/js/data/apiTypes.ts";
import { MARKERS } from "../logbook/logbookUtils.ts";
import { SET_COLUMNS } from "./exportData.ts";
import { readXlsxSheet } from "./xlsxRead.ts";

/** Example rows in the CSV layout. Exercise names should match ones in your account (or they're created as custom exercises). */
const EXAMPLE_ROWS: string[][] = [
    ["2026-11-02", "18:30", "Push", "Bench Press", "Strength", "Chest, Triceps", "1", "Warm-up", "10", "40", "Kg", "", "", "", "", "", "", "", "", ""],
    ["2026-11-02", "18:30", "Push", "Bench Press", "Strength", "Chest, Triceps", "1", "Working set", "6", "80", "Kg", "2", "", "", "", "", "", "", "Pause on the chest", "Felt strong"],
    ["2026-11-02", "18:30", "Push", "Bench Press", "Strength", "Chest, Triceps", "2", "Working set", "6", "80", "Kg", "1", "", "", "", "", "", "", "", ""],
    ["2026-11-04", "07:15", "Run", "Treadmill Run", "Cardio", "Quadriceps", "1", "Working set", "", "", "", "", "", "5", "Km", "00:27:30", "1650", "", "", ""],
];

/** Header plus example rows, for showing the CSV as a grid. */
export function exampleCsvGrid(): string[][] {
    return [[...SET_COLUMNS], ...EXAMPLE_ROWS];
}

/** The example file as text, exactly as it is in the downloaded file. */
export function exampleCsvText(): string {
    const cell = (v: string) => (/[",\n]/.test(v) ? `"${v.replace(/"/g, '""')}"` : v);
    return [[...SET_COLUMNS], ...EXAMPLE_ROWS].map(r => r.map(cell).join(",")).join("\n");
}

/** A minimal JSON file the importer accepts; ids are optional, exercises are matched by name. */
export function exampleJsonText(): string {
    return JSON.stringify({
        app: "Bloom",
        schemaVersion: 1,
        workouts: [{
            loggedAt: "2026-11-02T18:30:00",
            name: "Push",
            note: "Felt strong",
            exercises: [{
                exerciseName: "Bench Press",
                exerciseType: "Strength",
                note: "Pause on the chest",
                gear: [],
                sets: [
                    { type: "Strength", marker: "WarmUp", reps: 10, weight: 40, weightUnit: "Kg" },
                    { type: "Strength", reps: 6, weight: 80, weightUnit: "Kg", rir: 2 },
                ],
            }],
        }, {
            loggedAt: "2026-11-04T07:15:00",
            name: "Run",
            exercises: [{
                exerciseName: "Treadmill Run",
                exerciseType: "Cardio",
                sets: [{ type: "Cardio", distance: 5, distanceUnit: "Km", duration: "00:27:30" }],
            }],
        }],
    }, null, 2);
}

export function exampleBlob(kind: "csv" | "json"): Blob {
    // BOM so Excel opens the CSV as UTF-8.
    return kind === "csv"
        ? new Blob(["﻿" + exampleCsvText().replace(/\n/g, "\r\n")], { type: "text/csv;charset=utf-8" })
        : new Blob([exampleJsonText()], { type: "application/json" });
}

export type ImportFormat = "json" | "csv" | "xlsx";

export interface ParsedExerciseRef {
    /** Exercise id in the source account, when the file has one (Bloom JSON). */
    sourceId: string | null;
    name: string;
    type: ExerciseType | null;
    muscles: string[];
}

export interface ParsedWorkout {
    sourceId: string | null;
    name: string;
    loggedAt: Date;
    note: string | null;
    exercises: { ref: ParsedExerciseRef; note: string | null; gear: string[]; gpx: string | null; sets: LoggedSet[] }[];
}

export interface ParsedTemplate {
    name: string;
    exercises: { ref: ParsedExerciseRef; note: string | null; gear: string[]; sets: PlannedSet[] }[];
}

export interface ParsedFile {
    format: ImportFormat;
    fileName: string;
    workouts: ParsedWorkout[];
    templates: ParsedTemplate[];
    /** Custom exercise definitions shipped with the file (Bloom JSON). */
    customExercises: { sourceId: string | null; name: string; description: string; type: ExerciseType; targetMuscles: string[] }[];
    /** Rows or entries that couldn't be read, with the reason. */
    warnings: string[];
    /**
     * How dates like 02/11/2026 were read. `ambiguous` when nothing in the file
     * decides it (no day above 12), so the person should be asked.
     */
    dateOrder: { used: DateOrder; ambiguous: boolean; sample?: string };
}

/** Day-first (02/11 = 2 November) or month-first (02/11 = 11 February). */
export type DateOrder = "dmy" | "mdy";

export interface ParseOptions { dateOrder?: DateOrder }

export class ImportFileError extends Error {}

const TYPES: ExerciseType[] = ["Strength", "Cardio", "Plyometric"];
const asType = (v: unknown): ExerciseType | null =>
    TYPES.find(t => t.toLowerCase() === String(v ?? "").trim().toLowerCase()) ?? null;

const asNumber = (v: unknown): number | null => {
    if (v === null || v === undefined || v === "") return null;
    const n = typeof v === "number" ? v : Number(String(v).replace(",", "."));
    return Number.isFinite(n) ? n : null;
};

const pad = (n: number) => String(n).padStart(2, "0");
const hms = (seconds: number) => `${pad(Math.floor(seconds / 3600))}:${pad(Math.floor((seconds % 3600) / 60))}:${pad(Math.round(seconds % 60))}`;

function asWeightUnit(v: unknown): "Kg" | "Lbs" {
    return String(v ?? "").toLowerCase().startsWith("lb") ? "Lbs" : "Kg";
}

function asDistanceUnit(v: unknown): "Km" | "Miles" {
    return String(v ?? "").toLowerCase().startsWith("mi") ? "Miles" : "Km";
}

function asMarker(setType: unknown, setNumber: unknown): SetMarker | null {
    const label = String(setType ?? "").trim().toLowerCase();
    const found = MARKERS.find(m => m.value && (m.label.toLowerCase() === label || m.short.toLowerCase() === label || m.value.toLowerCase() === label));
    if (found?.value) return found.value;
    return String(setNumber ?? "").trim().toUpperCase() === "W" ? "WarmUp" : null;
}

// ── Entry point ───────────────────────────────────────────────────────────────

export async function parseImportFile(file: File, options: ParseOptions = {}): Promise<ParsedFile> {
    const ext = file.name.toLowerCase().split(".").pop();
    if (ext === "json") return parseJson(await file.text(), file.name);
    if (ext === "csv") return parseTable(parseCsv(await file.text()), "csv", file.name, options);
    if (ext === "xlsx") return parseTable(await readXlsxSheet(await file.arrayBuffer(), "Sets"), "xlsx", file.name, options);
    throw new ImportFileError("Choose a .json, .csv or .xlsx file exported from Bloom.");
}

// ── JSON (Bloom backup) ───────────────────────────────────────────────────────

interface JsonSet {
    order?: number; type?: string; marker?: SetMarker | null; reps?: number | null; weight?: number | null;
    weightUnit?: string | null; rir?: number | null; distance?: number | null; distanceUnit?: string | null; duration?: string | null;
}

function parseJson(text: string, fileName: string): ParsedFile {
    let data: {
        app?: string;
        workouts?: {
            id?: string; loggedAt?: string; name?: string; note?: string | null;
            exercises?: { exerciseId?: string; exerciseName?: string | null; exerciseType?: string | null; note?: string | null; gear?: string[]; gpx?: string; sets?: JsonSet[] }[];
        }[];
        templates?: { name?: string; exercises?: { exerciseId?: string; exerciseName?: string | null; note?: string | null; gear?: string[]; sets?: JsonSet[] }[] }[];
        customExercises?: { id?: string; name?: string; description?: string; type?: string; targetMuscles?: string[] }[];
    };
    try {
        data = JSON.parse(text);
    } catch {
        throw new ImportFileError("This file isn't valid JSON.");
    }
    if (!data || typeof data !== "object" || (!Array.isArray(data.workouts) && !Array.isArray(data.templates))) {
        throw new ImportFileError("This doesn't look like a Bloom backup: no workouts or templates in it.");
    }

    const warnings: string[] = [];
    const customs = (data.customExercises ?? []).flatMap(c => {
        const type = asType(c.type);
        if (!c.name || !type) return [];
        return [{ sourceId: c.id ?? null, name: c.name, description: c.description || "Imported custom exercise", type, targetMuscles: c.targetMuscles ?? [] }];
    });
    const customById = new Map(customs.map(c => [c.sourceId, c]));
    const ref = (id: string | undefined, name: string | null | undefined, type: string | null | undefined): ParsedExerciseRef => {
        const custom = id ? customById.get(id) : undefined;
        return {
            sourceId: id ?? null,
            name: name ?? custom?.name ?? "Unknown exercise",
            type: asType(type) ?? custom?.type ?? null,
            muscles: custom?.targetMuscles ?? [],
        };
    };
    const toSet = (s: JsonSet, i: number, fallback: ExerciseType | null): LoggedSet => ({
        type: asType(s.type) ?? fallback ?? "Strength",
        order: s.order ?? i,
        reps: s.reps ?? null,
        weight: s.weight ?? null,
        weightUnit: s.weight != null ? asWeightUnit(s.weightUnit) : null,
        rir: s.rir ?? null,
        distance: s.distance ?? null,
        distanceUnit: s.distance != null ? asDistanceUnit(s.distanceUnit) : null,
        duration: s.duration ?? null,
        marker: s.marker ?? null,
    } as LoggedSet);

    const workouts: ParsedWorkout[] = [];
    (data.workouts ?? []).forEach((w, i) => {
        const at = w.loggedAt ? new Date(w.loggedAt) : null;
        if (!at || Number.isNaN(at.getTime()) || !w.name) {
            warnings.push(`Workout ${i + 1}: missing a name or date, skipped.`);
            return;
        }
        workouts.push({
            sourceId: w.id ?? null,
            name: w.name,
            loggedAt: at,
            note: w.note ?? null,
            exercises: (w.exercises ?? []).map(e => {
                const r = ref(e.exerciseId, e.exerciseName, e.exerciseType);
                return { ref: r, note: e.note ?? null, gear: e.gear ?? [], gpx: e.gpx ?? null, sets: (e.sets ?? []).map((s, j) => toSet(s, j, r.type)) };
            }),
        });
    });

    const templates: ParsedTemplate[] = (data.templates ?? []).filter(t => t.name).map(t => ({
        name: t.name!,
        exercises: (t.exercises ?? []).map(e => {
            const r = ref(e.exerciseId, e.exerciseName, null);
            return {
                ref: { ...r, type: r.type ?? asType(e.sets?.[0]?.type) },
                note: e.note ?? null,
                gear: e.gear ?? [],
                sets: (e.sets ?? []).map((s, j) => ({
                    type: asType(s.type) ?? "Strength", order: s.order ?? j, reps: s.reps ?? null,
                    duration: s.duration ?? null, distance: s.distance ?? null, distanceUnit: s.distance != null ? asDistanceUnit(s.distanceUnit) : null,
                } as PlannedSet)),
            };
        }),
    }));

    return { format: "json", fileName, workouts, templates, customExercises: customs, warnings, dateOrder: { used: "dmy", ambiguous: false } };
}

// ── CSV / Excel (one row per set) ─────────────────────────────────────────────

/** RFC 4180 CSV: quoted fields, doubled quotes, CRLF, BOM. */
export function parseCsv(text: string): string[][] {
    const rows: string[][] = [];
    let row: string[] = [], field = "", quoted = false;
    const src = text.replace(new RegExp("^\uFEFF"), "");
    for (let i = 0; i < src.length; i++) {
        const c = src[i];
        if (quoted) {
            if (c === '"' && src[i + 1] === '"') { field += '"'; i++; }
            else if (c === '"') quoted = false;
            else field += c;
        } else if (c === '"') quoted = true;
        else if (c === ",") { row.push(field); field = ""; }
        else if (c === "\n" || c === "\r") {
            if (c === "\r" && src[i + 1] === "\n") i++;
            row.push(field); rows.push(row); row = []; field = "";
        } else field += c;
    }
    if (field !== "" || row.length) { row.push(field); rows.push(row); }
    return rows.filter(r => r.some(v => v.trim() !== ""));
}

/** The export prefixes text starting with = + - @ with a quote so spreadsheets don't run it; undo that. */
const unguard = (v: string) => (/^'[=+\-@\t\r]/.test(v) ? v.slice(1) : v);

const SLASH_DATE = /^(\d{1,2})[/.-](\d{1,2})[/.-](\d{4})/;

/** Date cell → local calendar day: "2026-11-02", "02/11/2026" (in `order`), or an Excel serial number. */
function dayOf(v: string | number, order: DateOrder): Date | null {
    if (typeof v === "number" || /^\d+(\.\d+)?$/.test(String(v).trim())) {
        const serial = Number(v);
        const utc = new Date(Math.round((serial - 25569) * 86_400_000));
        return new Date(utc.getUTCFullYear(), utc.getUTCMonth(), utc.getUTCDate());
    }
    const iso = String(v).trim().match(/^(\d{4})-(\d{1,2})-(\d{1,2})/);
    if (iso) return valid(Number(iso[1]), Number(iso[2]), Number(iso[3]));
    const m = String(v).trim().match(SLASH_DATE);
    if (!m) return null;
    const [a, b] = [Number(m[1]), Number(m[2])];
    return order === "dmy" ? valid(Number(m[3]), b, a) : valid(Number(m[3]), a, b);
}

/** A real calendar date, or null (so 31/02 or month 13 is reported instead of rolling over). */
function valid(y: number, month: number, d: number): Date | null {
    const date = new Date(y, month - 1, d);
    return date.getMonth() === month - 1 && date.getDate() === d ? date : null;
}

/** Decides day/month order from the file: any first part above 12 means day-first, any second part above 12 month-first. */
function detectDateOrder(cells: (string | number)[]): { order: DateOrder | null; slashDates: boolean; sample?: string } {
    let dayFirst = false, monthFirst = false, slashDates = false;
    let sample: string | undefined;
    for (const c of cells) {
        const m = typeof c === "string" ? c.trim().match(SLASH_DATE) : null;
        if (!m) continue;
        slashDates = true;
        sample ??= m[0];
        if (Number(m[1]) > 12) dayFirst = true;
        if (Number(m[2]) > 12) monthFirst = true;
    }
    return { order: dayFirst === monthFirst ? null : dayFirst ? "dmy" : "mdy", slashDates, sample };
}

/**
 * Sets the server would reject, per workout, so the review step can list them
 * before importing: Strength and Plyometric need reps (1+) and a weight (0+),
 * Cardio needs a distance and a duration.
 */
export function workoutProblems(w: ParsedWorkout): string[] {
    const problems = new Set<string>();
    if (w.exercises.length === 0) problems.add("has no exercises");
    for (const e of w.exercises) {
        if (e.sets.length === 0) problems.add(`${e.ref.name} has no sets`);
        for (const s of e.sets) {
            if (s.type === "Cardio") {
                const [h, m, sec] = (s.duration ?? "").split(":").map(Number);
                if (!(s.distance && s.distance > 0)) problems.add(`${e.ref.name}: a cardio set has no distance`);
                if (!((h || 0) * 3600 + (m || 0) * 60 + (sec || 0) > 0)) problems.add(`${e.ref.name}: a cardio set has no duration`);
            } else {
                if (!(s.reps && s.reps >= 1)) problems.add(`${e.ref.name}: a set has no reps`);
                if (s.weight == null || s.weight < 0) problems.add(`${e.ref.name}: a set has no weight (use 0 for bodyweight)`);
            }
        }
    }
    return [...problems];
}

/** Time cell → [hours, minutes]: "18:30" text, or an Excel day fraction. */
function timeOf(v: string | number): [number, number] {
    if (typeof v === "number" || /^0?\.\d+$/.test(String(v).trim())) {
        const minutes = Math.round(Number(v) * 24 * 60);
        return [Math.floor(minutes / 60) % 24, minutes % 60];
    }
    const m = String(v).match(/(\d{1,2}):(\d{2})/);
    return m ? [Number(m[1]), Number(m[2])] : [12, 0];
}

function parseTable(rows: (string | number)[][], format: "csv" | "xlsx", fileName: string, options: ParseOptions = {}): ParsedFile {
    if (rows.length === 0) throw new ImportFileError("The file is empty.");
    const header = rows[0].map(h => String(h).trim().toLowerCase());
    const col = (name: (typeof SET_COLUMNS)[number]) => header.indexOf(name.toLowerCase());
    const idx = {
        date: col("Date"), time: col("Time"), workout: col("Workout"), exercise: col("Exercise"),
        type: col("Exercise type"), muscles: col("Target muscles"), set: col("Set"), setType: col("Set type"),
        reps: col("Reps"), weight: col("Weight"), weightUnit: col("Weight unit"), rir: col("RIR"),
        distance: col("Distance"), distanceUnit: col("Distance unit"), duration: col("Duration"), durationS: col("Duration (s)"),
        gear: col("Gear"), exNote: col("Exercise note"), workoutNote: col("Workout note"),
    };
    if (idx.date < 0 || idx.workout < 0 || idx.exercise < 0) {
        throw new ImportFileError(`This doesn't look like a Bloom export: the first row needs at least the columns Date, Workout and Exercise${format === "xlsx" ? " (on the “Sets” sheet)" : ""}.`);
    }

    const warnings: string[] = [];
    const workouts: ParsedWorkout[] = [];
    const byKey = new Map<string, ParsedWorkout>();
    const get = (r: (string | number)[], i: number) => (i >= 0 ? r[i] ?? "" : "");
    const text = (r: (string | number)[], i: number) => unguard(String(get(r, i)).trim());

    const detected = detectDateOrder(rows.slice(1).map(r => get(r, idx.date)));
    const order: DateOrder = options.dateOrder ?? detected.order ?? "dmy";
    const dateOrder = { used: order, ambiguous: detected.slashDates && detected.order === null, sample: detected.sample };

    rows.slice(1).forEach((r, n) => {
        const line = n + 2;
        const day = dayOf(get(r, idx.date), order);
        const workoutName = text(r, idx.workout);
        const exerciseName = text(r, idx.exercise);
        if (!day || !workoutName || !exerciseName) {
            warnings.push(`Row ${line}: missing a date, workout or exercise, skipped.`);
            return;
        }
        const [h, m] = timeOf(get(r, idx.time));
        const loggedAt = new Date(day.getFullYear(), day.getMonth(), day.getDate(), h, m);
        const key = `${loggedAt.getTime()}|${workoutName}`;
        let workout = byKey.get(key);
        if (!workout) {
            workout = { sourceId: null, name: workoutName, loggedAt, note: text(r, idx.workoutNote) || null, exercises: [] };
            byKey.set(key, workout);
            workouts.push(workout);
        }

        // Consecutive rows of the same exercise are one exercise entry.
        let entry = workout.exercises.at(-1);
        if (!entry || entry.ref.name !== exerciseName) {
            entry = {
                ref: {
                    sourceId: null,
                    name: exerciseName,
                    type: asType(text(r, idx.type)),
                    muscles: text(r, idx.muscles).split(",").map(s => s.trim()).filter(Boolean),
                },
                note: text(r, idx.exNote) || null,
                gear: text(r, idx.gear).split(",").map(s => s.trim()).filter(Boolean),
                gpx: null,
                sets: [],
            };
            workout.exercises.push(entry);
        }

        const distance = asNumber(get(r, idx.distance));
        const seconds = asNumber(get(r, idx.durationS));
        const rawDuration = text(r, idx.duration);
        const duration = /^\d+:\d{2}(:\d{2})?$/.test(rawDuration)
            ? (rawDuration.split(":").length === 2 ? `00:${rawDuration}` : rawDuration).padStart(8, "0")
            : seconds != null ? hms(seconds) : null;
        const weight = asNumber(get(r, idx.weight));
        const type = entry.ref.type ?? (distance != null || duration ? "Cardio" : "Strength");
        entry.sets.push({
            type,
            order: entry.sets.length,
            reps: asNumber(get(r, idx.reps)),
            weight,
            weightUnit: weight != null ? asWeightUnit(get(r, idx.weightUnit)) : null,
            rir: asNumber(get(r, idx.rir)),
            distance,
            distanceUnit: distance != null ? asDistanceUnit(get(r, idx.distanceUnit)) : null,
            duration,
            marker: asMarker(get(r, idx.setType), get(r, idx.set)),
        } as LoggedSet);
    });

    return { format, fileName, workouts, templates: [], customExercises: [], warnings, dateOrder };
}

// ── Plan: match against this account ──────────────────────────────────────────

export interface ImportPlan {
    /** Source exercise (by lower-cased name) → existing exercise, or a custom one to create. */
    exercises: Map<string, { existing: Exercise } | { create: { name: string; description: string; type: ExerciseType; targetMuscles: string[] } }>;
    newWorkouts: ParsedWorkout[];
    duplicateWorkouts: number;
    newTemplates: ParsedTemplate[];
    existingTemplates: number;
    range: { from: Date; to: Date } | null;
}

const exerciseKey = (ref: ParsedExerciseRef) => ref.name.trim().toLowerCase();
const workoutKey = (name: string, at: Date) => `${Math.floor(at.getTime() / 60_000)}|${name.trim().toLowerCase()}`;

/**
 * Works out what an import would do: which exercises match ones you have (by id,
 * then by name), which must be created as custom exercises, and which workouts
 * and templates you already have. The server makes the final duplicate check.
 */
export function planImport(file: ParsedFile, catalog: Exercise[], logs: LoggedWorkout[], templates: WorkoutTemplate[]): ImportPlan {
    const byId = new Map(catalog.map(e => [e.id, e]));
    const byName = new Map(catalog.map(e => [e.name.trim().toLowerCase(), e]));
    const customByName = new Map(file.customExercises.map(c => [c.name.trim().toLowerCase(), c]));

    const exercises: ImportPlan["exercises"] = new Map();
    const refs = [
        ...file.workouts.flatMap(w => w.exercises.map(e => ({ ref: e.ref, setType: e.sets[0]?.type ?? null }))),
        ...file.templates.flatMap(t => t.exercises.map(e => ({ ref: e.ref, setType: e.sets[0]?.type ?? null }))),
    ];
    for (const { ref, setType } of refs) {
        const key = exerciseKey(ref);
        if (exercises.has(key)) continue;
        const existing = (ref.sourceId ? byId.get(ref.sourceId) : undefined) ?? byName.get(key);
        if (existing) { exercises.set(key, { existing }); continue; }
        const def = customByName.get(key);
        exercises.set(key, {
            create: {
                name: ref.name.trim(),
                description: def?.description || `Imported from ${file.fileName}`,
                type: def?.type ?? ref.type ?? (setType as ExerciseType | null) ?? "Strength",
                targetMuscles: def?.targetMuscles ?? ref.muscles,
            },
        });
    }

    const have = new Set(logs.map(l => workoutKey(l.name, new Date(l.loggedAt))));
    const haveIds = new Set(logs.map(l => l.id));
    const newWorkouts: ParsedWorkout[] = [];
    let duplicateWorkouts = 0;
    for (const w of file.workouts) {
        const key = workoutKey(w.name, w.loggedAt);
        if (have.has(key) || (w.sourceId && haveIds.has(w.sourceId))) { duplicateWorkouts++; continue; }
        newWorkouts.push(w);
    }

    const templateNames = new Set(templates.map(t => t.name.trim().toLowerCase()));
    const newTemplates = file.templates.filter(t => !templateNames.has(t.name.trim().toLowerCase()));

    const times = file.workouts.map(w => w.loggedAt.getTime());
    return {
        exercises,
        newWorkouts,
        duplicateWorkouts,
        newTemplates,
        existingTemplates: file.templates.length - newTemplates.length,
        range: times.length ? { from: new Date(Math.min(...times)), to: new Date(Math.max(...times)) } : null,
    };
}

/** Turns planned workouts into API bodies once every exercise has an id. */
export function toImportBodies(workouts: ParsedWorkout[], idFor: (ref: ParsedExerciseRef) => string) {
    return workouts.map(w => ({
        name: w.name,
        loggedAt: w.loggedAt.toISOString(),
        note: w.note,
        sourceId: w.sourceId,
        exercises: w.exercises.map((e, i): LoggedExercise => ({
            exerciseId: idFor(e.ref),
            order: i,
            note: e.note,
            gear: e.gear,
            gpxData: e.gpx,
            sets: e.sets,
        })),
    }));
}

export function toTemplateExercises(t: ParsedTemplate, idFor: (ref: ParsedExerciseRef) => string): TemplateExercise[] {
    return t.exercises.map((e, i) => ({ exerciseId: idFor(e.ref), order: i, note: e.note, gear: e.gear, sets: e.sets }));
}

// ── Exercise matching helpers (names only; ids are never needed) ──────────────

/** What to do with one exercise name from the file. */
export type Decision =
    | { kind: "existing"; exerciseId: string | null }
    | { kind: "create"; name: string; type: ExerciseType; muscles: string[] };

const ALIASES: Record<string, string> = {
    db: "dumbbell", dbs: "dumbbell", dumbell: "dumbbell", dumbbells: "dumbbell", dumbel: "dumbbell",
    bb: "barbell", barbells: "barbell", kb: "kettlebell", kettlebells: "kettlebell",
    ez: "ez", bw: "bodyweight", cable: "cable", machine: "machine",
    flye: "fly", flyes: "fly", flys: "fly", flies: "fly", pulldown: "pulldown", pullup: "pull up", chinup: "chin up",
};

function tokens(name: string): string[] {
    return name.toLowerCase().replace(/[^a-z0-9]+/g, " ").trim().split(" ").filter(Boolean)
        .map(t => ALIASES[t] ?? (t.length > 3 && t.endsWith("s") ? t.slice(0, -1) : t));
}

function editDistance(a: string, b: string): number {
    const prev = Array.from({ length: b.length + 1 }, (_, i) => i);
    for (let i = 1; i <= a.length; i++) {
        let diag = prev[0];
        prev[0] = i;
        for (let j = 1; j <= b.length; j++) {
            const up = prev[j];
            prev[j] = Math.min(prev[j] + 1, prev[j - 1] + 1, diag + (a[i - 1] === b[j - 1] ? 0 : 1));
            diag = up;
        }
    }
    return prev[b.length];
}

/** 0–1: how alike two exercise names are (typos, word order, "DB" for dumbbell, extra words). */
export function nameSimilarity(a: string, b: string): number {
    const ta = tokens(a), tb = tokens(b);
    if (!ta.length || !tb.length) return 0;
    const ca = ta.join(""), cb = tb.join("");
    if (ca === cb) return 1;
    const sa = new Set(ta), sb = new Set(tb);
    const shared = [...sa].filter(t => sb.has(t)).length;
    const jaccard = shared / new Set([...sa, ...sb]).size;
    const ratio = 1 - editDistance(ca, cb) / Math.max(ca.length, cb.length);
    // One name contained in the other ("cable fly" in "cable reverse fly") is worth suggesting, not pre-selecting.
    const subset = Math.min(sa.size, sb.size) >= 2 && (shared === sa.size || shared === sb.size) ? 0.72 : 0;
    return Math.max(ratio, jaccard * 0.95, subset);
}

/** Your exercises, most similar to `query` first (all of them, so the picker can list the rest). */
export function rankExercises(query: string, catalog: Exercise[]): { exercise: Exercise; score: number }[] {
    return catalog.map(exercise => ({ exercise, score: nameSimilarity(query, exercise.name) })).sort((x, y) => y.score - x.score || x.exercise.name.localeCompare(y.exercise.name));
}

/** The closest existing exercise, if it's close enough to be worth suggesting. */
export function suggestExercise(name: string, catalog: Exercise[]): Exercise | null {
    const best = rankExercises(name, catalog)[0];
    return best && best.score >= 0.62 ? best.exercise : null;
}

/** How often each exercise name is used in what would be imported. */
export function exerciseUsage(workouts: ParsedWorkout[], templates: ParsedTemplate[]): Map<string, { name: string; sets: number; workouts: number; templates: number; type: ExerciseType | null; muscles: string[] }> {
    const usage = new Map<string, { name: string; sets: number; workouts: number; templates: number; type: ExerciseType | null; muscles: string[] }>();
    const touch = (ref: ParsedExerciseRef, sets: number, kind: "workouts" | "templates") => {
        const key = ref.name.trim().toLowerCase();
        const u = usage.get(key) ?? { name: ref.name.trim(), sets: 0, workouts: 0, templates: 0, type: ref.type, muscles: ref.muscles };
        u.sets += sets;
        u[kind] += 1;
        usage.set(key, u);
    };
    for (const w of workouts) for (const e of w.exercises) touch(e.ref, e.sets.length, "workouts");
    for (const t of templates) for (const e of t.exercises) touch(e.ref, e.sets.length, "templates");
    return usage;
}

/** Why a decision can't be used yet, or null. */
export function decisionProblem(d: Decision | undefined, catalogNames: Set<string>, otherNewNames: Set<string>): string | null {
    if (!d) return "Choose what to do with this exercise.";
    if (d.kind === "existing") return d.exerciseId ? null : "Pick an exercise.";
    const name = d.name.trim().toLowerCase();
    if (!name) return "Give the new exercise a name.";
    if (name.length > 100) return "Name is too long (100 characters at most).";
    if (catalogNames.has(name)) return "You already have an exercise with this name; match it instead.";
    if (otherNewNames.has(name)) return "Another new exercise has this name.";
    return null;
}
