export type WorkoutFilterType = "Cardio" | "Strength" | "Plyometric";

export function dominantType(sets: { type: string }[]): "cardio" | "strength" | "plyometric" | "mix" {
    const cardio   = sets.filter(s => s.type === "Cardio").length;
    const plyo     = sets.filter(s => s.type === "Plyometric").length;
    const strength = sets.length - cardio - plyo;
    if (cardio > 0 && (strength > 0 || plyo > 0)) return "mix";
    if (cardio >= strength && cardio >= plyo) return "cardio";
    if (plyo > strength) return "plyometric";
    return "strength";
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

export function displayDuration(raw: string | null): string {
    if (!raw) return "—";
    const [h, m, s] = raw.split(":").map(Number);
    if (h) return `${h}h ${m}m`;
    if (m) return `${m}m ${s ?? 0}s`;
    return `${s ?? 0}s`;
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

export function formatWorkoutDate(date: Date): string {
    return date.toLocaleDateString("en-GB", {
        weekday: "long", day: "numeric", month: "long", year: "numeric",
    });
}
