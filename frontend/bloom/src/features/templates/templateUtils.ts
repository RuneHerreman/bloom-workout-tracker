import type { WorkoutTemplate } from "./api.ts";

export type FilterType = "Cardio" | "Strength" | "Plyometric";

export function dominantType(template: WorkoutTemplate): "cardio" | "strength" | "plyometric" | "mix" {
    const sets = template.exercises.flatMap(ex => ex.sets);
    const cardio   = sets.filter(s => s.type === "Cardio").length;
    const plyo     = sets.filter(s => s.type === "Plyometric").length;
    const strength = sets.length - cardio - plyo;
    if (cardio > 0 && (strength > 0 || plyo > 0)) return "mix";
    if (cardio >= strength && cardio >= plyo) return "cardio";
    if (plyo > strength) return "plyometric";
    return "strength";
}

export function matchesFilter(template: WorkoutTemplate, filter: FilterType): boolean {
    return template.exercises.flatMap(ex => ex.sets).some(s => s.type === filter);
}

export function parseDuration(raw: string | null): [number, number, number] {
    if (!raw) return [0, 0, 0];
    const parts = raw.split(":").map(Number);
    if (parts.length === 3) return [parts[0] ?? 0, parts[1] ?? 0, parts[2] ?? 0];
    if (parts.length === 2) return [0, parts[0] ?? 0, parts[1] ?? 0];
    return [0, 0, parts[0] ?? 0];
}

export function formatDuration(h: number, m: number, s: number): string {
    const pad = (n: number) => String(n).padStart(2, "0");
    return `${pad(h)}:${pad(m)}:${pad(s)}`;
}