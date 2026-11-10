import type { LoggedWorkout, LoggedSet } from "./api.ts";
import type { SetMarker } from "../../assets/js/data/apiTypes.ts";
import {
    dominantType as _dominantType,
    formatDate,
    relativeDate,
    toDateInputValue,
    withDate,
    displayDuration,
    type WorkoutFilterType,
} from "../../utils/workoutUtils.ts";

export type LogFilterType = WorkoutFilterType;
export { formatDate, relativeDate, toDateInputValue, withDate, displayDuration };

export function dominantTypeFromLog(log: LoggedWorkout): "cardio" | "strength" | "plyometric" | "mix" {
    return _dominantType(log.exercises.flatMap(ex => ex.sets));
}

export function matchesLogFilter(log: LoggedWorkout, filter: LogFilterType): boolean {
    return log.exercises.flatMap(ex => ex.sets).some(s => s.type === filter);
}

// ── Set markers ───────────────────────────────────────────────────────────────

export const MARKERS: { value: SetMarker | null; short: string; label: string; hint: string }[] = [
    { value: null,      short: "#", label: "Working set", hint: "A regular set — counts towards numbering" },
    { value: "WarmUp",  short: "W", label: "Warm-up",     hint: "Light prep set, not counted as a working set" },
    { value: "DropSet", short: "D", label: "Drop set",    hint: "Weight reduced right after the previous set" },
    { value: "Failure", short: "F", label: "Failure",     hint: "Taken to (or past) muscular failure" },
];

export function markerShort(marker: SetMarker | null | undefined): string | null {
    if (!marker) return null;
    return MARKERS.find(m => m.value === marker)?.short ?? null;
}

export function markerLabel(marker: SetMarker | null | undefined): string {
    return MARKERS.find(m => m.value === (marker ?? null))?.label ?? "Working set";
}

/** Working-set numbers per row: warm-ups don't advance the count. */
export function workingSetNumbers(sets: LoggedSet[]): number[] {
    let n = 0;
    return sets.map(s => (s.marker === "WarmUp" ? 0 : ++n));
}

// ── Month grouping ────────────────────────────────────────────────────────────

export interface MonthGroup<T> {
    key: string;      // "2026-11"
    label: string;    // "November 2026" (year omitted for the current year)
    items: T[];
}

/** Groups items (already sorted newest-first) into calendar months in local time. */
export function groupByMonth<T>(items: T[], dateOf: (item: T) => string): MonthGroup<T>[] {
    const groups: MonthGroup<T>[] = [];
    const thisYear = new Date().getFullYear();
    for (const item of items) {
        const d = new Date(dateOf(item));
        const key = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}`;
        let group = groups.at(-1);
        if (!group || group.key !== key) {
            const label = d.toLocaleDateString("en-US", {
                month: "long",
                ...(d.getFullYear() !== thisYear ? { year: "numeric" } : {}),
            });
            group = { key, label, items: [] };
            groups.push(group);
        }
        group.items.push(item);
    }
    return groups;
}
