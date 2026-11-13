import { fetchFromServer, remember, peek } from "../../assets/js/data/apiClient.ts";
import type { ExerciseType } from "../../types.ts";
import type { CustomExerciseBody } from "../../assets/js/data/apiTypes.ts";

export type { CustomExerciseBody };

export interface Exercise {
    id: string;
    name: string;
    description: string;
    type: ExerciseType;
    /** True for exercises the current user created themselves. */
    isCustom?: boolean;
    /** Primary movers: a working set counts fully for each. */
    targetMuscles: string[];
    /** Assisting muscles: a working set counts as half a set for each. */
    secondaryMuscles: string[];
}

export interface ExerciseFilters {
    name?: string;
    targetMuscleGroups?: string[];
    exerciseTypes?: ExerciseType[];
}

let _allExercises: Promise<Exercise[]> | null = null;

export function searchExercises(filters?: ExerciseFilters): Promise<Exercise[]> {
    const hasFilters = filters && (
        filters.name ||
        (filters.targetMuscleGroups?.length ?? 0) > 0 ||
        (filters.exerciseTypes?.length ?? 0) > 0
    );

    if (!hasFilters) {
        if (!_allExercises) {
            _allExercises = remember(fetchFromServer<Exercise[]>("exercises", "GET")
                .catch(e => { _allExercises = null; throw e; }));
        }
        return _allExercises;
    }

    const p = new URLSearchParams();
    if (filters?.name) p.set("Name", filters.name);
    filters?.targetMuscleGroups?.forEach(m => p.append("TargetMuscleGroups", m));
    filters?.exerciseTypes?.forEach(t => p.append("ExerciseTypes", t));
    return fetchFromServer<Exercise[]>(`exercises?${p.toString()}`, "GET");
}

export function peekExercises(): Exercise[] | undefined {
    return peek(_allExercises);
}

/** Drops the cached exercise list (e.g. after an import was undone). */
export function forgetExercises(): void {
    _allExercises = null;
}

export async function getExercise(exerciseId: string): Promise<Exercise> {
    return fetchFromServer<Exercise>(`exercises/${exerciseId}`, "GET");
}

// ── Custom exercises ──────────────────────────────────────────────────────────

export async function createCustomExercise(body: CustomExerciseBody): Promise<Exercise> {
    const { exerciseId } = await fetchFromServer<{ exerciseId: string }>("exercises", "POST", body);
    _allExercises = null;
    return { id: exerciseId, ...body, secondaryMuscles: body.secondaryMuscles ?? [], isCustom: true };
}

export async function updateCustomExercise(exerciseId: string, body: CustomExerciseBody): Promise<Exercise> {
    await fetchFromServer<unknown>(`exercises/${exerciseId}`, "PUT", body);
    _allExercises = null;
    return { id: exerciseId, ...body, secondaryMuscles: body.secondaryMuscles ?? [], isCustom: true };
}

export async function deleteCustomExercise(exerciseId: string): Promise<void> {
    await fetchFromServer<unknown>(`exercises/${exerciseId}`, "DELETE");
    _allExercises = null;
}
