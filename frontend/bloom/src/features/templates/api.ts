import { fetchFromServer } from "../../assets/js/data/apiClient.ts";

export interface PlannedSet {
    type: string;
    order: number;
    reps: number | null;
    duration: string | null;
    distance: number | null;
    distanceUnit: string | null;
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
