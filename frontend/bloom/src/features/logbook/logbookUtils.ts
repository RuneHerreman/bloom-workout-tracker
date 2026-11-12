import type { LoggedWorkout, LoggedSet, LoggedExercise } from "./api.ts";
import type { SetMarker, SetHint } from "../../assets/js/data/apiTypes.ts";
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

// ── Previous performance ──────────────────────────────────────────────────────

/**
 * An empty set: nothing is made up for an exercise you've never logged. For one
 * you have, the values from last time ride along as a hint (see withHint).
 */
export function defaultSet(type: string, order = 1): LoggedSet {
    return type === "Cardio"
        ? { type: "Cardio", order, reps: null, weight: null, weightUnit: null, rir: null, marker: null, duration: null, distance: null, distanceUnit: "km" }
        : { type, order, reps: null, weight: null, weightUnit: "kg", rir: null, marker: null, duration: null, distance: null, distanceUnit: null };
}

export type PreviousSetsLookup = (exerciseId: string, beforeIso?: string) => LoggedSet[] | undefined;

/**
 * Builds a lookup for "what did I do last time" per exercise: the sets of the most
 * recent log containing it, optionally only from logs before a given date (so
 * editing an old log prefills from the session before *that* one).
 */
export function buildPreviousSetsLookup(logs: LoggedWorkout[], excludeLogId?: string): PreviousSetsLookup {
    const sorted = logs
        .filter(l => l.id !== excludeLogId)
        .sort((a, b) => new Date(b.loggedAt).getTime() - new Date(a.loggedAt).getTime());
    return (exerciseId, beforeIso) => {
        const before = beforeIso ? new Date(beforeIso).getTime() : Infinity;
        for (const log of sorted) {
            if (new Date(log.loggedAt).getTime() >= before) continue;
            const ex = log.exercises.find(e => e.exerciseId === exerciseId && e.sets.length > 0);
            if (ex) return [...ex.sets].sort((a, b) => a.order - b.order);
        }
        return undefined;
    };
}

/** What a set shows (typed value, else its own hint): the hint for the set that follows it. */
export function hintFrom(set: LoggedSet): SetHint {
    return {
        reps: set.reps ?? set.hint?.reps ?? null,
        weight: set.weight ?? set.hint?.weight ?? null,
        duration: set.duration ?? set.hint?.duration ?? null,
        distance: set.distance ?? set.hint?.distance ?? null,
    };
}

/** An empty set of the given type whose placeholders show `reference` (last time's set, or the row above). */
export function hintedSet(type: string, order: number, reference: LoggedSet | undefined): LoggedSet {
    const empty = defaultSet(type, order);
    if (!reference || reference.type !== type) return empty;
    return {
        ...empty,
        weightUnit: reference.weightUnit ?? empty.weightUnit,
        distanceUnit: reference.distanceUnit ?? empty.distanceUnit,
        hint: hintFrom(reference),
    };
}

/** First set for a newly added exercise: empty, with last time's first set as placeholders. */
export function prefilledFirstSet(type: string, previous: LoggedSet[] | undefined): LoggedSet {
    return hintedSet(type, 1, previous?.[0]);
}

/** "Add set": an empty working set showing the row above as placeholders. */
export function duplicateSet(last: LoggedSet, order: number): LoggedSet {
    return hintedSet(last.type, order, last);
}

/**
 * Fills each empty field with the placeholder it showed and drops the client-only
 * hint, giving the sets that get saved. Strength sets with no weight are bodyweight (0).
 */
export function resolveHints(exercises: LoggedExercise[]): LoggedExercise[] {
    return exercises.map(ex => ({
        ...ex,
        sets: ex.sets.map(({ hint, ...set }) => set.type === "Cardio"
            ? { ...set, duration: set.duration ?? hint?.duration ?? null, distance: set.distance ?? hint?.distance ?? null }
            : { ...set, reps: set.reps ?? hint?.reps ?? null, weight: set.weight ?? hint?.weight ?? 0 }),
    }));
}

/** The first strength set left without reps (nothing typed, nothing from last time), if any. */
export function findMissingReps(exercises: LoggedExercise[]): { exercise: LoggedExercise; setIndex: number } | null {
    for (const exercise of exercises) {
        const setIndex = exercise.sets.findIndex(s => s.type !== "Cardio" && s.reps == null && s.hint?.reps == null);
        if (setIndex >= 0) return { exercise, setIndex };
    }
    return null;
}
