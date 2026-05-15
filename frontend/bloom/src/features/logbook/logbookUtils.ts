import type { LoggedWorkout } from "./api.ts";

export type LogFilterType = "Cardio" | "Strength" | "Plyometric";

export function dominantTypeFromLog(log: LoggedWorkout): "cardio" | "strength" | "plyometric" | "mix" {
    const sets = log.exercises.flatMap(ex => ex.sets);
    const cardio   = sets.filter(s => s.type === "Cardio").length;
    const plyo     = sets.filter(s => s.type === "Plyometric").length;
    const strength = sets.length - cardio - plyo;
    if (cardio > 0 && (strength > 0 || plyo > 0)) return "mix";
    if (cardio >= strength && cardio >= plyo) return "cardio";
    if (plyo > strength) return "plyometric";
    return "strength";
}

export function matchesLogFilter(log: LoggedWorkout, filter: LogFilterType): boolean {
    return log.exercises.flatMap(ex => ex.sets).some(s => s.type === filter);
}

export function formatDate(isoString: string): string {
    return new Date(isoString).toLocaleDateString("en-US", {
        month: "short", day: "numeric", year: "numeric",
    });
}

export function relativeDate(isoString: string): string {
    const diffDays = Math.floor((Date.now() - new Date(isoString).getTime()) / 86_400_000);
    if (diffDays === 0) return "Today";
    if (diffDays === 1) return "Yesterday";
    if (diffDays < 7) return `${diffDays}d ago`;
    return formatDate(isoString);
}

export function toDateInputValue(isoString: string): string {
    return isoString.split("T")[0] ?? "";
}

export function displayDuration(raw: string | null): string {
    if (!raw) return "—";
    const [h, m, s] = raw.split(":").map(Number);
    if (h) return `${h}h ${m}m`;
    if (m) return `${m}m ${s ?? 0}s`;
    return `${s ?? 0}s`;
}