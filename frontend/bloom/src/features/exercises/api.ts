import { fetchFromServer } from "../../assets/js/data/apiClient.ts";
import type { ExerciseType } from "../../types.ts";

export interface Exercise {
    id: string;
    name: string;
    description: string;
    type: ExerciseType;
    targetMuscles: string[];
}

export interface ExerciseFilters {
    name?: string;
    targetMuscleGroups?: string[];
    exerciseTypes?: ExerciseType[];
}

export function searchExercises(filters?: ExerciseFilters): Promise<Exercise[]> {
    const hasFilters = filters && (
        filters.name ||
        (filters.targetMuscleGroups?.length ?? 0) > 0 ||
        (filters.exerciseTypes?.length ?? 0) > 0
    );

    if (!hasFilters) {
        return fetchFromServer<Exercise[]>("exercises", "GET");
    }

    const p = new URLSearchParams();
    if (filters?.name) p.set("Name", filters.name);
    filters?.targetMuscleGroups?.forEach(m => p.append("TargetMuscleGroups", m));
    filters?.exerciseTypes?.forEach(t => p.append("ExerciseTypes", t));
    return fetchFromServer<Exercise[]>(`exercises?${p.toString()}`, "GET");
}

export async function getExercise(exerciseId: string): Promise<Exercise> {
    return fetchFromServer<Exercise>(`exercises/${exerciseId}`, "GET");
}
