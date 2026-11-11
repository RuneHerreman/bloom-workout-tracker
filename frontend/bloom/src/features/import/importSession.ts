// Pure logic behind the import page: the decision for each exercise name in the
// file, its status, and the evidence shown next to it.

import type { Exercise, LoggedWorkout } from "../../assets/js/data/apiTypes.ts";
import type { ExerciseType } from "../../types.ts";
import {
    exerciseUsage, rankExercises, workoutProblems,
    type Decision, type ImportPlan, type ParsedWorkout,
} from "../export/importData.ts";

/** Suggestions at least this close start out pre-selected (still marked "to check"). */
export const STRONG_MATCH = 0.8;
/** Below this a candidate isn't worth showing as an alternative. */
const WEAK_MATCH = 0.45;

export type Status = "check" | "matched" | "new" | "skip";

export interface Choice {
    decision: Decision;
    /** Confirmed by the person (or an exact name match). Unchecked choices still import as shown. */
    checked: boolean;
}

export const keyOf = (name: string) => name.trim().toLowerCase();

export function statusOf(choice: Choice | undefined): Status {
    if (!choice || !choice.checked) return "check";
    return choice.decision.kind === "existing" ? "matched" : choice.decision.kind === "skip" ? "skip" : "new";
}

/** Starting point: exact names linked, strong suggestions pre-selected, the rest proposed as new. */
export function initialChoices(plan: ImportPlan, catalog: Exercise[]): Map<string, Choice> {
    const out = new Map<string, Choice>();
    for (const [key, target] of plan.exercises) {
        if ("existing" in target) {
            out.set(key, { decision: { kind: "existing", exerciseId: target.existing.id }, checked: true });
            continue;
        }
        const best = rankExercises(target.create.name, catalog)[0];
        out.set(key, best && best.score >= STRONG_MATCH
            ? { decision: { kind: "existing", exerciseId: best.exercise.id }, checked: false }
            : { decision: { kind: "create", name: target.create.name, type: target.create.type, muscles: target.create.targetMuscles }, checked: false });
    }
    return out;
}

/** Candidates for one name: the best match and a few alternatives worth a click. */
export function candidatesFor(name: string, catalog: Exercise[]) {
    const ranked = rankExercises(name, catalog).filter(r => r.score >= WEAK_MATCH);
    return { best: ranked[0] ?? null, alternatives: ranked.slice(1, 5).map(r => r.exercise) };
}

export interface Evidence {
    name: string;
    sets: number;
    workouts: number;
    templates: number;
    first: Date | null;
    last: Date | null;
    /** "50–97.5 kg", "3.2–10 km" — whatever the sets have. */
    range: string | null;
    /** A few recent sets as they appear in the file. */
    samples: string[];
    type: ExerciseType | null;
    muscles: string[];
}

const fmtDay = (d: Date) => d.toLocaleDateString("en-GB", { day: "numeric", month: "short", year: d.getFullYear() === new Date().getFullYear() ? undefined : "numeric" });
const num = (n: number) => String(Math.round(n * 10) / 10);

/** What the file contains for each exercise name, for the detail panel. */
export function evidenceByKey(workouts: ParsedWorkout[], templates: { exercises: { ref: { name: string } }[] }[]): Map<string, Evidence> {
    const usage = exerciseUsage(workouts, templates as never);
    const out = new Map<string, Evidence>();
    for (const [key, u] of usage) {
        const sessions = workouts
            .flatMap(w => w.exercises.filter(e => keyOf(e.ref.name) === key).map(e => ({ at: w.loggedAt, sets: e.sets })))
            .sort((a, b) => a.at.getTime() - b.at.getTime());
        const weights = sessions.flatMap(s => s.sets.map(x => x.weight).filter((w): w is number => w != null));
        const units = sessions.flatMap(s => s.sets.map(x => x.weightUnit).filter(Boolean));
        const dists = sessions.flatMap(s => s.sets.map(x => x.distance).filter((d): d is number => d != null && d > 0));
        const distUnit = sessions.flatMap(s => s.sets.map(x => x.distanceUnit).filter(Boolean))[0] ?? "Km";
        const wUnit = (units[0] ?? "Kg").toLowerCase();
        const range = weights.length
            ? `${num(Math.min(...weights))}${Math.min(...weights) !== Math.max(...weights) ? `–${num(Math.max(...weights))}` : ""} ${wUnit}`
            : dists.length
                ? `${num(Math.min(...dists))}${Math.min(...dists) !== Math.max(...dists) ? `–${num(Math.max(...dists))}` : ""} ${String(distUnit).toLowerCase()}`
                : null;
        const samples = sessions.slice(-3).reverse().map(s => {
            const top = [...s.sets].sort((a, b) => (b.weight ?? b.distance ?? 0) - (a.weight ?? a.distance ?? 0))[0];
            const what = !top ? "" : top.type === "Cardio"
                ? `${top.distance ?? "?"} ${String(top.distanceUnit ?? "km").toLowerCase()} in ${(top.duration ?? "").replace(/^00:/, "")}`
                : `${top.weight ?? "?"} ${(top.weightUnit ?? "kg").toLowerCase()} × ${top.reps ?? "?"}`;
            return `${fmtDay(s.at)} · ${what}`;
        });
        out.set(key, {
            name: u.name, sets: u.sets, workouts: u.workouts, templates: u.templates,
            first: sessions[0]?.at ?? null, last: sessions.at(-1)?.at ?? null,
            range, samples, type: u.type, muscles: u.muscles,
        });
    }
    return out;
}

/** How often you've logged each exercise, to show "you've logged it 118 times". */
export function timesLogged(logs: LoggedWorkout[]): Map<string, number> {
    const counts = new Map<string, number>();
    for (const l of logs) for (const e of l.exercises) counts.set(e.exerciseId, (counts.get(e.exerciseId) ?? 0) + 1);
    return counts;
}

/** Why a choice can't be imported as-is, or null. */
export function choiceProblem(choice: Choice | undefined, catalogNames: Set<string>, otherNewNames: Set<string>): string | null {
    if (!choice) return "Choose what to do with this exercise.";
    const d = choice.decision;
    if (d.kind === "skip") return null;
    if (d.kind === "existing") return d.exerciseId ? null : "Pick an exercise.";
    const name = d.name.trim().toLowerCase();
    if (!name) return "Give the new exercise a name.";
    if (name.length > 100) return "Use 100 characters at most.";
    if (catalogNames.has(name)) return "You already have an exercise with this name. Match it instead.";
    if (otherNewNames.has(name)) return "Another new exercise already has this name.";
    return null;
}

export interface Fixes {
    /** Strength sets with a weight but no reps get your usual reps for that exercise. */
    fillReps: boolean;
    /** Strength sets with no weight are logged as bodyweight (0 kg). */
    zeroWeight: boolean;
}

export interface Prepared {
    ok: ParsedWorkout[];
    rejected: { workout: ParsedWorkout; problems: string[] }[];
    /** Workouts left with no exercises after skipping. */
    emptied: number;
    /** Sets that got assumed reps / 0 kg, and in how many workouts. */
    filledReps: { sets: number; workouts: number };
    zeroWeights: { sets: number; workouts: number };
}

const DEFAULT_REPS = 8;

/** Most common reps for each exercise: from the file first, then your history, else 8. */
function usualReps(workouts: ParsedWorkout[], logs: LoggedWorkout[], idFor: (key: string) => string | null): (key: string) => number {
    const fromFile = new Map<string, Map<number, number>>();
    for (const w of workouts) for (const e of w.exercises) for (const set of e.sets) {
        if (!set.reps || set.reps < 1) continue;
        const counts = fromFile.get(keyOf(e.ref.name)) ?? new Map<number, number>();
        counts.set(set.reps, (counts.get(set.reps) ?? 0) + 1);
        fromFile.set(keyOf(e.ref.name), counts);
    }
    const fromHistory = new Map<string, Map<number, number>>();
    for (const l of logs) for (const e of l.exercises) for (const set of e.sets) {
        if (!set.reps || set.reps < 1 || set.marker === "WarmUp") continue;
        const counts = fromHistory.get(e.exerciseId) ?? new Map<number, number>();
        counts.set(set.reps, (counts.get(set.reps) ?? 0) + 1);
        fromHistory.set(e.exerciseId, counts);
    }
    const mode = (counts: Map<number, number> | undefined) =>
        counts && counts.size ? [...counts].sort((a, b) => b[1] - a[1] || b[0] - a[0])[0][0] : null;
    return key => mode(fromFile.get(key)) ?? mode(fromHistory.get(idFor(key) ?? "")) ?? DEFAULT_REPS;
}

/**
 * Applies the exercise decisions and fixes to the file's workouts, giving what
 * would actually be imported:
 * - skipped exercises are left out (and workouts left empty are dropped);
 * - each set takes the type of the exercise it's matched to or created as, so
 *   "Crunches" typed as cardio in the file become strength sets on Crunch;
 * - missing reps / weights are filled in when the fix is on, with a note;
 * - whatever the server would still reject is listed with the reason.
 */
export function prepareWorkouts(
    workouts: ParsedWorkout[], choices: Map<string, Choice>, catalog: Exercise[], logs: LoggedWorkout[], fixes: Fixes,
): Prepared {
    const byId = new Map(catalog.map(e => [e.id, e]));
    const typeFor = (key: string): ExerciseType | null => {
        const d = choices.get(key)?.decision;
        if (!d || d.kind === "skip") return null;
        return d.kind === "create" ? d.type : (byId.get(d.exerciseId ?? "")?.type as ExerciseType | undefined) ?? null;
    };
    const idFor = (key: string) => { const d = choices.get(key)?.decision; return d?.kind === "existing" ? d.exerciseId : null; };
    const reps = usualReps(workouts, logs, idFor);

    const ok: ParsedWorkout[] = [];
    const rejected: Prepared["rejected"] = [];
    let emptied = 0;
    const filled = { sets: 0, workouts: 0 }, zeroed = { sets: 0, workouts: 0 };

    for (const w of workouts) {
        let filledHere = false, zeroedHere = false;
        const exercises = w.exercises.flatMap(e => {
            const key = keyOf(e.ref.name);
            if (choices.get(key)?.decision.kind === "skip") return [];
            const type = typeFor(key) ?? e.sets[0]?.type ?? "Strength";
            const notes: string[] = [];
            const sets = e.sets.map(set => {
                if (type === "Cardio") {
                    return { ...set, type, reps: null, weight: null, weightUnit: null, rir: null };
                }
                let { reps: r, weight, weightUnit } = set;
                if ((!r || r < 1) && fixes.fillReps) {
                    r = reps(key);
                    filled.sets++; filledHere = true;
                    if (!notes.includes("reps")) notes.push("reps");
                }
                if ((weight == null || weight < 0) && fixes.zeroWeight) {
                    weight = 0; weightUnit = "Kg";
                    zeroed.sets++; zeroedHere = true;
                    if (!notes.includes("weight")) notes.push("weight");
                }
                return { ...set, type, reps: r, weight, weightUnit: weight != null ? weightUnit ?? "Kg" : null, distance: null, distanceUnit: null, duration: null };
            });
            const added = [
                notes.includes("reps") ? `Reps not recorded, assumed ${reps(key)}` : null,
                notes.includes("weight") ? "Weight not recorded, logged as bodyweight" : null,
            ].filter(Boolean).join(". ");
            return [{ ...e, sets, note: [e.note, added].filter(Boolean).join(" · ") || null }];
        });
        if (exercises.length === 0) { emptied++; continue; }
        if (filledHere) filled.workouts++;
        if (zeroedHere) zeroed.workouts++;
        const prepared = { ...w, exercises };
        const problems = workoutProblems(prepared);
        if (problems.length) rejected.push({ workout: prepared, problems }); else ok.push(prepared);
    }
    return { ok, rejected, emptied, filledReps: filled, zeroWeights: zeroed };
}
