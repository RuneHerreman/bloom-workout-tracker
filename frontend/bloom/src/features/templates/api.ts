import { fetchFromServer } from "../../assets/js/data/apiClient.ts";
import type { ExerciseType, DistanceUnit } from "../../types.ts";

// ── Response types ────────────────────────────────────────────────────────────

export interface PlannedSet {
    type: ExerciseType;
    order: number;
    reps: number | null;
    duration: string | null;       // TimeSpan string: "HH:mm:ss"
    distance: number | null;
    distanceUnit: DistanceUnit | null;
}

export interface TemplateExercise {
    exerciseId: string;
    order: number;
    sets: PlannedSet[];
}

export interface WorkoutTemplate {
    id: string;
    userId: string;
    name: string;
    exercises: TemplateExercise[];
}

// ── Set factory functions ─────────────────────────────────────────────────────

// Cardio planned set — requires target duration and distance
export function createCardioPlannedSet(
    order: number,
    duration: string,       // "HH:mm:ss"
    distance: number,
    distanceUnit: DistanceUnit
): PlannedSet {
    return {
        type: "Cardio",
        order,
        duration,
        distance,
        distanceUnit,
        reps: null,
    };
}

// Strength planned set — only target reps (no weight/RIR planned in templates)
export function createStrengthPlannedSet(order: number, reps: number): PlannedSet {
    return {
        type: "Strength",
        order,
        reps,
        duration: null,
        distance: null,
        distanceUnit: null,
    };
}

// Plyometric planned set — only target reps
export function createPlyometricPlannedSet(order: number, reps: number): PlannedSet {
    return {
        type: "Plyometric",
        order,
        reps,
        duration: null,
        distance: null,
        distanceUnit: null,
    };
}

// ── API functions ─────────────────────────────────────────────────────────────

export async function getTemplates(name?: string): Promise<WorkoutTemplate[]> {
    const params = name ? `?Name=${encodeURIComponent(name)}` : "";
    return fetchFromServer<WorkoutTemplate[]>(`templates${params}`, "GET");
}

export async function getTemplate(templateId: string): Promise<WorkoutTemplate> {
    return fetchFromServer<WorkoutTemplate>(`templates/${templateId}`, "GET");
}

export async function createTemplate(name: string, exercises: TemplateExercise[]): Promise<string> {
    const response = await fetchFromServer<{ workoutTemplateId: string }>("templates", "POST", { name, exercises });
    return response.workoutTemplateId;
}

export async function updateTemplate(templateId: string, name: string, exercises: TemplateExercise[]): Promise<string> {
    const response = await fetchFromServer<{ workoutTemplateId: string }>(`templates/${templateId}`, "PUT", { name, exercises });
    return response.workoutTemplateId;
}

export async function deleteTemplate(templateId: string): Promise<void> {
    await fetchFromServer<unknown>(`templates/${templateId}`, "DELETE");
}
