import type { LoggedWorkout, WorkoutTemplate, Exercise } from "../../assets/js/data/apiTypes.ts";
import { parseDuration } from "../../utils/workoutUtils.ts";
import { markerLabel, workingSetNumbers } from "../logbook/logbookUtils.ts";
import { buildXlsx, type CellValue } from "./xlsx.ts";

export type ExportFormat = "csv" | "xlsx" | "json";
export type ExportRange = "all" | "year" | "90d" | "30d";

export const RANGE_LABELS: Record<ExportRange, string> = {
    all: "All time",
    year: "This year",
    "90d": "Last 90 days",
    "30d": "Last 30 days",
};

export function filterByRange(logs: LoggedWorkout[], range: ExportRange, now = new Date()): LoggedWorkout[] {
    if (range === "all") return logs;
    const from = range === "year"
        ? new Date(now.getFullYear(), 0, 1)
        : new Date(now.getTime() - (range === "90d" ? 90 : 30) * 86_400_000);
    return logs.filter(l => new Date(l.loggedAt) >= from);
}

// ── Shared helpers ────────────────────────────────────────────────────────────

const pad = (n: number) => String(n).padStart(2, "0");
const localDate = (d: Date) => `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
const localTime = (d: Date) => `${pad(d.getHours())}:${pad(d.getMinutes())}`;

function durationSeconds(raw: string | null): number | null {
    if (!raw) return null;
    const [h, m, s] = parseDuration(raw);
    return h * 3600 + m * 60 + s;
}

function toKg(weight: number | null, unit: string | null): number {
    if (weight == null) return 0;
    return (unit ?? "kg").toLowerCase() === "lbs" ? weight * 0.45359237 : weight;
}

function toKm(distance: number | null, unit: string | null): number {
    if (distance == null) return 0;
    const u = (unit ?? "km").toLowerCase();
    return u === "miles" ? distance * 1.609344 : u === "meters" ? distance / 1000 : distance;
}

const round = (n: number, digits = 2) => Math.round(n * 10 ** digits) / 10 ** digits;

const sortedLogs = (logs: LoggedWorkout[]) =>
    [...logs].sort((a, b) => new Date(a.loggedAt).getTime() - new Date(b.loggedAt).getTime());

// ── Tabular (one row per set) ─────────────────────────────────────────────────

export const SET_COLUMNS = [
    "Date", "Time", "Workout", "Exercise", "Exercise type", "Target muscles",
    "Set", "Set type", "Reps", "Weight", "Weight unit", "RIR", "Volume (kg)",
    "Distance", "Distance unit", "Duration", "Duration (s)", "Gear", "Exercise note", "Workout note",
] as const;

export function setRows(logs: LoggedWorkout[], exercises: Record<string, Exercise>): CellValue[][] {
    const rows: CellValue[][] = [];
    for (const log of sortedLogs(logs)) {
        const when = new Date(log.loggedAt);
        for (const ex of [...log.exercises].sort((a, b) => a.order - b.order)) {
            const info = exercises[ex.exerciseId];
            const sets = [...ex.sets].sort((a, b) => a.order - b.order);
            const numbers = workingSetNumbers(sets);
            sets.forEach((s, i) => {
                const volume = s.reps != null && s.weight != null ? round(s.reps * toKg(s.weight, s.weightUnit)) : null;
                rows.push([
                    when, localTime(when), log.name,
                    info?.name ?? "Unknown exercise", info?.type ?? s.type, info?.targetMuscles.join(", ") ?? "",
                    s.marker === "WarmUp" ? "W" : numbers[i], markerLabel(s.marker),
                    s.reps, s.weight, s.weight != null ? s.weightUnit : null, s.rir, volume,
                    s.distance, s.distance != null ? s.distanceUnit : null,
                    s.duration, durationSeconds(s.duration),
                    (ex.gear ?? []).join(", "), ex.note ?? "", log.note ?? "",
                ]);
            });
        }
    }
    return rows;
}

export const WORKOUT_COLUMNS = [
    "Date", "Workout", "Exercises", "Sets", "Working sets", "Volume (kg)", "Distance (km)", "Duration (min)", "Note",
] as const;

export function workoutRows(logs: LoggedWorkout[]): CellValue[][] {
    return sortedLogs(logs).map(log => {
        const sets = log.exercises.flatMap(e => e.sets);
        const volume = sets.reduce((n, s) => n + (s.reps ?? 0) * toKg(s.weight, s.weightUnit) * (s.marker === "WarmUp" ? 0 : 1), 0);
        const km = sets.reduce((n, s) => n + toKm(s.distance, s.distanceUnit), 0);
        const seconds = sets.reduce((n, s) => n + (durationSeconds(s.duration) ?? 0), 0);
        return [
            new Date(log.loggedAt), log.name, log.exercises.length, sets.length,
            sets.filter(s => s.marker !== "WarmUp").length,
            volume ? round(volume, 1) : null, km ? round(km) : null, seconds ? round(seconds / 60, 1) : null,
            log.note ?? "",
        ];
    });
}

// ── CSV ───────────────────────────────────────────────────────────────────────

function csvCell(v: CellValue): string {
    if (v === null || v === undefined) return "";
    if (v instanceof Date) return localDate(v);
    if (typeof v === "number") return String(v);
    // Neutralise spreadsheet formula injection in free-text fields.
    const text = /^[=+\-@\t\r]/.test(v) ? `'${v}` : v;
    return /[",\n\r]/.test(text) ? `"${text.replace(/"/g, '""')}"` : text;
}

export function buildCsv(logs: LoggedWorkout[], exercises: Record<string, Exercise>): Blob {
    const lines = [SET_COLUMNS as readonly CellValue[], ...setRows(logs, exercises)].map(r => r.map(csvCell).join(","));
    // BOM so Excel opens UTF-8 (accents, emoji in notes) correctly.
    return new Blob(["﻿" + lines.join("\r\n")], { type: "text/csv;charset=utf-8" });
}

// ── Excel ─────────────────────────────────────────────────────────────────────

export function buildWorkbook(logs: LoggedWorkout[], exercises: Record<string, Exercise>): Blob {
    return buildXlsx([
        {
            name: "Sets",
            rows: [[...SET_COLUMNS], ...setRows(logs, exercises)],
            widths: [11, 7, 24, 26, 12, 24, 5, 11, 6, 8, 7, 5, 11, 9, 9, 10, 11, 18, 30, 30],
        },
        {
            name: "Workouts",
            rows: [[...WORKOUT_COLUMNS], ...workoutRows(logs)],
            widths: [11, 26, 10, 6, 12, 12, 13, 14, 40],
        },
    ]);
}

// ── JSON (full backup) ────────────────────────────────────────────────────────

export function buildJson(
    logs: LoggedWorkout[],
    templates: WorkoutTemplate[],
    exercises: Record<string, Exercise>,
    options: { includeGps: boolean; range: ExportRange }
): Blob {
    const exerciseName = (id: string) => exercises[id]?.name ?? null;
    const payload = {
        app: "Bloom",
        schemaVersion: 1,
        exportedAt: new Date().toISOString(),
        range: options.range,
        workouts: sortedLogs(logs).map(l => ({
            id: l.id,
            loggedAt: l.loggedAt,
            name: l.name,
            note: l.note,
            exercises: [...l.exercises].sort((a, b) => a.order - b.order).map(e => ({
                exerciseId: e.exerciseId,
                exerciseName: exerciseName(e.exerciseId),
                exerciseType: exercises[e.exerciseId]?.type ?? null,
                note: e.note ?? null,
                gear: e.gear ?? [],
                sets: [...e.sets].sort((a, b) => a.order - b.order).map(s => ({
                    order: s.order, type: s.type, marker: s.marker ?? null,
                    reps: s.reps, weight: s.weight, weightUnit: s.weightUnit, rir: s.rir,
                    distance: s.distance, distanceUnit: s.distanceUnit, duration: s.duration,
                })),
                ...(options.includeGps && e.gpxData ? { gpx: e.gpxData } : {}),
            })),
        })),
        templates: templates.map(t => ({
            id: t.id,
            name: t.name,
            exercises: [...t.exercises].sort((a, b) => a.order - b.order).map(e => ({
                exerciseId: e.exerciseId,
                exerciseName: exerciseName(e.exerciseId),
                note: e.note ?? null,
                gear: e.gear ?? [],
                sets: e.sets,
            })),
        })),
        customExercises: Object.values(exercises).filter(e => e.isCustom),
    };
    return new Blob([JSON.stringify(payload, null, 2)], { type: "application/json" });
}

// ── Download ──────────────────────────────────────────────────────────────────

export function downloadBlob(blob: Blob, filename: string) {
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    a.remove();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
}

export function exportFilename(format: ExportFormat, now = new Date()): string {
    return `bloom-${format === "json" ? "backup" : "logs"}-${localDate(now)}.${format}`;
}
