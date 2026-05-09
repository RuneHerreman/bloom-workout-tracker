import { fetchFromServer } from "../../assets/js/data/apiClient.ts";

export interface Exercise {
    id: string;
    name: string;
    description: string;
    type: string;
    targetMuscles: string[];
}

export interface ExerciseFilters {
    name?: string;
    targetMuscleGroups?: string[];
    exerciseTypes?: string[];
}

export async function searchExercises(filters?: ExerciseFilters): Promise<Exercise[]> {
    const p = new URLSearchParams();
    if (filters?.name) p.set("Name", filters.name);
    filters?.targetMuscleGroups?.forEach(m => p.append("TargetMuscleGroups", m));
    filters?.exerciseTypes?.forEach(t => p.append("ExerciseTypes", t));
    const query = p.toString() ? `?${p.toString()}` : "";
    return fetchFromServer<Exercise[]>(`exercises${query}`, "GET");
}

export async function getExercise(exerciseId: string): Promise<Exercise> {
    return fetchFromServer<Exercise>(`exercises/${exerciseId}`, "GET");
}
