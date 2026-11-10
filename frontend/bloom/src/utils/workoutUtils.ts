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

// Compares calendar days in the user's local time zone rather than elapsed
// milliseconds: a log stamped "now" by the server can sit slightly in the future
// relative to the client clock, which used to floor to "-1d ago".
export function relativeDate(isoString: string): string {
    const then = new Date(isoString);
    const now = new Date();
    const startOfDay = (d: Date) => new Date(d.getFullYear(), d.getMonth(), d.getDate()).getTime();
    const diffDays = Math.round((startOfDay(now) - startOfDay(then)) / 86_400_000);
    if (diffDays < 0) return formatDate(isoString);
    if (diffDays === 0) return "Today";
    if (diffDays === 1) return "Yesterday";
    if (diffDays < 7) return `${diffDays}d ago`;
    return formatDate(isoString);
}

/** Local-time YYYY-MM-DD for an <input type="date">. */
export function toDateInputValue(isoString: string): string {
    const d = new Date(isoString);
    if (Number.isNaN(d.getTime())) return isoString.split("T")[0] ?? "";
    const pad = (n: number) => String(n).padStart(2, "0");
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
}

/** Moves `isoString` to the local calendar day `dateInput` (YYYY-MM-DD), keeping its time of day. */
export function withDate(isoString: string, dateInput: string): string {
    const [y, m, d] = dateInput.split("-").map(Number);
    const result = new Date(isoString);
    if (!y || !m || !d || Number.isNaN(result.getTime())) return new Date(dateInput).toISOString();
    result.setFullYear(y, m - 1, d);
    return result.toISOString();
}

export function formatWorkoutDate(date: Date): string {
    return date.toLocaleDateString("en-GB", {
        weekday: "long", day: "numeric", month: "long", year: "numeric",
    });
}
