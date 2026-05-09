import { fetchFromServer } from "../../assets/js/data/apiClient.ts";
import type { ExerciseType, WeightUnit, DistanceUnit } from "../../types.ts";

// ── Response types ────────────────────────────────────────────────────────────

export interface LoggedSet {
    type: ExerciseType;
    order: number;
    duration: string | null;       // TimeSpan string: "HH:mm:ss"
    distance: number | null;
    distanceUnit: DistanceUnit | null;
    reps: number | null;
    weight: number | null;
    weightUnit: WeightUnit | null;
    rir: number | null;
}

export interface LoggedExercise {
    exerciseId: string;
    order: number;
    sets: LoggedSet[];
}

export interface LoggedWorkout {
    id: string;
    userId: string;
    loggedAt: string;
    exercises: LoggedExercise[];
}

export interface ExercisePr {
    exerciseId: string;
    exerciseName: string;
    exerciseType: ExerciseType;
    targetMuscles: string[];
    weight: number;
    weightUnit: WeightUnit;
}

export interface MonthlyVolume {
    year: number;
    month: number;
    maxWeight: number;
    weightUnit: WeightUnit;
}

export interface ExerciseVolume {
    exerciseId: string;
    exerciseName: string;
    exerciseType: ExerciseType;
    targetMuscles: string[];
    monthlyVolume: MonthlyVolume[];
}

// ── Set factory functions ─────────────────────────────────────────────────────

export function createCardioSet(
    order: number,
    duration: string,       // "HH:mm:ss"
    distance: number,
    distanceUnit: DistanceUnit
): LoggedSet {
    return {
        type: "Cardio",
        order,
        duration,
        distance,
        distanceUnit,
        reps: null,
        weight: null,
        weightUnit: null,
        rir: null,
    };
}

export function createStrengthSet(
    order: number,
    reps: number,
    weight: number,
    weightUnit: WeightUnit,
    rir: number
): LoggedSet {
    return {
        type: "Strength",
        order,
        reps,
        weight,
        weightUnit,
        rir,
        duration: null,
        distance: null,
        distanceUnit: null,
    };
}

export function createPlyometricSet(
    order: number,
    reps: number,
    weight: number,
    weightUnit: WeightUnit,
    rir: number
): LoggedSet {
    return {
        type: "Plyometric",
        order,
        reps,
        weight,
        weightUnit,
        rir,
        duration: null,
        distance: null,
        distanceUnit: null,
    };
}

// ── Filter types ──────────────────────────────────────────────────────────────

export interface ExerciseFilters {
    name?: string;
    targetMuscleGroups?: string[];
    exerciseTypes?: ExerciseType[];
}

export interface VolumeFilters extends ExerciseFilters {
    fromYear?: number;
    fromMonth?: number;
    toYear?: number;
    toMonth?: number;
}

// ── API functions ─────────────────────────────────────────────────────────────

export async function getLogs(): Promise<LoggedWorkout[]> {
    return fetchFromServer<LoggedWorkout[]>("logs", "GET");
}

export async function getLog(logId: string): Promise<LoggedWorkout> {
    return fetchFromServer<LoggedWorkout>(`logs/${logId}`, "GET");
}

export async function createLog(exercises: LoggedExercise[], loggedAt?: string): Promise<string> {
    const response = await fetchFromServer<{ loggedWorkoutId: string }>("logs", "POST", {
        exercises,
        ...(loggedAt ? { loggedAt } : {}),
    });
    return response.loggedWorkoutId;
}

export async function updateLog(logId: string, loggedAt: string, exercises: LoggedExercise[]): Promise<string> {
    const response = await fetchFromServer<{ loggedWorkoutId: string }>(`logs/${logId}`, "PUT", { loggedAt, exercises });
    return response.loggedWorkoutId;
}

export async function deleteLog(logId: string): Promise<void> {
    await fetchFromServer<unknown>(`logs/${logId}`, "DELETE");
}

export async function getPRs(filters?: ExerciseFilters): Promise<ExercisePr[]> {
    return fetchFromServer<ExercisePr[]>(`logs/pr${buildExerciseParams(filters)}`, "GET");
}

export async function getVolume(filters?: VolumeFilters): Promise<ExerciseVolume[]> {
    return fetchFromServer<ExerciseVolume[]>(`logs/volume${buildVolumeParams(filters)}`, "GET");
}

// ── Query param helpers ───────────────────────────────────────────────────────

function buildExerciseParams(filters?: ExerciseFilters): string {
    if (!filters) return "";
    const p = new URLSearchParams();
    if (filters.name) p.set("Name", filters.name);
    filters.targetMuscleGroups?.forEach(m => p.append("TargetMuscleGroups", m));
    filters.exerciseTypes?.forEach(t => p.append("ExerciseTypes", t));
    const s = p.toString();
    return s ? `?${s}` : "";
}

function buildVolumeParams(filters?: VolumeFilters): string {
    if (!filters) return "";
    const p = new URLSearchParams();
    if (filters.name) p.set("Name", filters.name);
    filters.targetMuscleGroups?.forEach(m => p.append("TargetMuscleGroups", m));
    filters.exerciseTypes?.forEach(t => p.append("ExerciseTypes", t));
    if (filters.fromYear !== undefined) p.set("FromYear", String(filters.fromYear));
    if (filters.fromMonth !== undefined) p.set("FromMonth", String(filters.fromMonth));
    if (filters.toYear !== undefined) p.set("ToYear", String(filters.toYear));
    if (filters.toMonth !== undefined) p.set("ToMonth", String(filters.toMonth));
    const s = p.toString();
    return s ? `?${s}` : "";
}
