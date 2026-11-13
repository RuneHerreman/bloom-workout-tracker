import type { LoggedExercise, LoggedWorkout } from "../../assets/js/data/apiTypes.ts";
import type { Exercise } from "../exercises/api.ts";
import { isWorkingStrengthSet, toKg, toKm, toMinutes } from "../insights/analysis.ts";
import { muscleShares } from "../exercises/muscles.ts";

export interface MuscleSets { muscle: string; sets: number }

/** A body area and the muscles in it, as the exercise library names them. */
export interface MuscleArea { area: string; sets: number; parts: MuscleSets[] }

/**
 * Areas in the order a body reads top to bottom; each lists its muscles. The broad
 * old names at the end ("Back", "Shoulders", "Core", "Rhomboids") are only for custom
 * exercises or imports that still use them.
 */
export const AREAS: { area: string; muscles: string[] }[] = [
    { area: "Chest", muscles: ["Chest"] },
    { area: "Back", muscles: ["Lats", "Upper back", "Traps", "Lower back", "Back", "Rhomboids"] },
    { area: "Shoulders", muscles: ["Front delts", "Side delts", "Rear delts", "Shoulders"] },
    { area: "Arms", muscles: ["Biceps", "Triceps", "Forearms"] },
    { area: "Core", muscles: ["Abs", "Obliques", "Core"] },
    { area: "Legs", muscles: ["Quadriceps", "Hamstrings", "Glutes", "Adductors", "Calves"] },
    { area: "Full body", muscles: ["Full Body"] },
];

/** The area a muscle belongs to; a name we don't know (a custom exercise's) is an area of its own. */
export function areaOf(muscle: string): string {
    return AREAS.find(a => a.muscles.includes(muscle))?.area ?? muscle;
}

export interface SessionRecord { exerciseId: string; name: string; weightKg: number; previousKg: number }

export interface SessionSummary {
    /** Working sets only: warm-ups are prep, not training volume. */
    volumeKg: number;
    workingSets: number;
    strengthExercises: number;
    distanceKm: number;
    minutes: number;
    /** Working sets per muscle, most first: a full set for each primary muscle, half for each secondary one. */
    muscles: MuscleSets[];
    /** The same, per area with its muscles inside, most first. An exercise counts once per area, at its biggest share there. */
    areas: MuscleArea[];
    /** Exercises where this session's heaviest working set beats every earlier session. */
    records: SessionRecord[];
    /** The last earlier session with the same name, for "vs last time". */
    previous: { name: string; loggedAt: string; volumeKg: number } | null;
}

const sameName = (a: string, b: string) => a.trim().toLowerCase() === b.trim().toLowerCase();

function workingVolumeKg(exercises: LoggedExercise[]): number {
    return exercises.reduce((t, ex) =>
        t + ex.sets.filter(isWorkingStrengthSet).reduce((s, set) => s + toKg(set.weight, set.weightUnit) * (set.reps ?? 0), 0), 0);
}

function heaviestKg(ex: LoggedExercise): number {
    return Math.max(0, ...ex.sets.filter(s => isWorkingStrengthSet(s) && (s.reps ?? 0) > 0).map(s => toKg(s.weight, s.weightUnit)));
}

/**
 * What one session added up to. `history` is every other log; only the ones
 * before `loggedAt` count for records and the comparison.
 */
export function summarizeSession(
    name: string,
    loggedAt: string,
    exercises: LoggedExercise[],
    exerciseInfo: Record<string, Exercise>,
    history: LoggedWorkout[],
): SessionSummary {
    const at = new Date(loggedAt).getTime();
    const earlier = history
        .filter(l => new Date(l.loggedAt).getTime() < at)
        .sort((a, b) => new Date(b.loggedAt).getTime() - new Date(a.loggedAt).getTime());

    let workingSets = 0, strengthExercises = 0, distanceKm = 0, minutes = 0;
    const muscles = new Map<string, number>();
    const areaSets = new Map<string, number>();
    const records: SessionRecord[] = [];

    for (const ex of exercises) {
        for (const s of ex.sets) {
            if (s.type !== "Cardio") continue;
            distanceKm += toKm(s.distance, s.distanceUnit);
            minutes += toMinutes(s.duration);
        }

        const working = ex.sets.filter(isWorkingStrengthSet).length;
        if (!working) continue;
        workingSets += working;
        strengthExercises++;
        const shares = muscleShares(exerciseInfo[ex.exerciseId]);
        const perArea = new Map<string, number>();
        for (const [m, share] of shares) {
            muscles.set(m, (muscles.get(m) ?? 0) + working * share);
            const a = areaOf(m);
            perArea.set(a, Math.max(perArea.get(a) ?? 0, share));
        }
        for (const [a, share] of perArea) areaSets.set(a, (areaSets.get(a) ?? 0) + working * share);

        const now = heaviestKg(ex);
        const before = Math.max(0, ...earlier.flatMap(l => l.exercises.filter(e => e.exerciseId === ex.exerciseId).map(heaviestKg)));
        // A first time isn't a record: there's nothing to beat yet.
        if (now > 0 && before > 0 && now > before && !records.some(r => r.exerciseId === ex.exerciseId)) {
            records.push({ exerciseId: ex.exerciseId, name: exerciseInfo[ex.exerciseId]?.name ?? "Exercise", weightKg: now, previousKg: before });
        }
    }

    const muscleList = [...muscles.entries()].map(([muscle, sets]) => ({ muscle, sets })).sort((a, b) => b.sets - a.sets || a.muscle.localeCompare(b.muscle));
    const order = (area: string) => { const i = AREAS.findIndex(a => a.area === area); return i < 0 ? AREAS.length : i; };
    const areas = [...areaSets.entries()]
        .map(([area, sets]) => ({ area, sets, parts: muscleList.filter(m => areaOf(m.muscle) === area) }))
        .sort((a, b) => b.sets - a.sets || order(a.area) - order(b.area));

    const prev = name.trim() ? earlier.find(l => sameName(l.name, name)) : undefined;

    return {
        volumeKg: workingVolumeKg(exercises),
        workingSets,
        strengthExercises,
        distanceKm,
        minutes,
        muscles: muscleList,
        areas,
        records,
        previous: prev ? { name: prev.name, loggedAt: prev.loggedAt, volumeKg: workingVolumeKg(prev.exercises) } : null,
    };
}

/** A tap on the selected muscle clears it; a mouse click keeps what hovering already picked. */
export function tapToggle(e: Event, active: string | null, muscle: string): string | null {
    const isMouse = (e as PointerEvent).pointerType === "mouse";
    return !isMouse && active === muscle ? null : muscle;
}
