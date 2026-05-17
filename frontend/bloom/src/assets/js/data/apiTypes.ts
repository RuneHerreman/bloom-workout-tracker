// Single source of truth for all API types — derived from OpenAPI spec bloommain--v1.yaml

import type { ExerciseType, DistanceUnit } from "../../../types.ts";

// ── Shared ────────────────────────────────────────────────────────────────────

export interface MonthlyVolumeResponse {
    year: number;
    month: number;
    maxWeight: number;
    weightUnit: string;
}

// ── Exercises ─────────────────────────────────────────────────────────────────

export interface Exercise {
    id: string;
    name: string;
    description: string;
    type: string;
    targetMuscles: string[];
}

export interface ExercisePrResponse {
    exerciseId: string;
    exerciseName: string;
    exerciseType: string;
    targetMuscles: string[];
    weight: number;
    weightUnit: string;
}

export interface ExerciseVolumeResponse {
    exerciseId: string;
    exerciseName: string;
    exerciseType: string;
    targetMuscles: string[];
    monthlyVolume: MonthlyVolumeResponse[];
}

// ── Logged sets & exercises ───────────────────────────────────────────────────

export interface LoggedSet {
    type: string;
    order: number;
    duration: string | null;
    distance: number | null;
    distanceUnit: DistanceUnit | null;
    reps: number | null;
    weight: number | null;
    weightUnit: string | null;
    rir: number | null;
}

export type LoggedSetBody = LoggedSet;

export interface LoggedExercise {
    exerciseId: string;
    order: number;
    gpxData: string | null;
    sets: LoggedSet[];
}

export interface LoggedExerciseBody {
    exerciseId: string;
    order: number;
    sets: LoggedSetBody[];
    gpxData?: string | null;
}

// ── Logged workouts ───────────────────────────────────────────────────────────

export interface LoggedWorkout {
    id: string;
    userId: string;
    loggedAt: string;
    name: string;
    note: string | null;
    exercises: LoggedExercise[];
}

export interface CreateLoggedWorkoutBody {
    name: string;
    exercises: LoggedExerciseBody[];
    note?: string | null;
    loggedAt?: string | null;
}

export interface CreateLoggedWorkoutResponse {
    loggedWorkoutId: string;
}

export interface UpdateLoggedWorkoutBody {
    name: string;
    loggedAt: string;
    exercises: LoggedExerciseBody[];
    note?: string | null;
}

export interface UpdateLoggedWorkoutResponse {
    loggedWorkoutId: string;
}

// ── Planned sets & exercises ──────────────────────────────────────────────────

export interface PlannedSet {
    type: ExerciseType;
    order: number;
    reps: number | null;
    duration: string | null;
    distance: number | null;
    distanceUnit: DistanceUnit | null;
}

export type PlannedSetBody = PlannedSet;

export interface TemplateExercise {
    exerciseId: string;
    order: number;
    sets: PlannedSet[];
}

export type TemplateExerciseBody = TemplateExercise;

// ── Workout templates ─────────────────────────────────────────────────────────

export interface WorkoutTemplate {
    id: string;
    userId: string;
    name: string;
    exercises: TemplateExercise[];
}

export interface CreateWorkoutTemplateBody {
    name: string;
    exercises: TemplateExerciseBody[];
}

export interface CreateWorkoutTemplateResponse {
    workoutTemplateId: string;
}

export interface UpdateWorkoutTemplateBody {
    name: string;
    exercises: TemplateExerciseBody[];
}

export interface UpdateWorkoutTemplateResponse {
    workoutTemplateId: string;
}

// ── Users ─────────────────────────────────────────────────────────────────────

export interface User {
    id: string;
    email: string;
    username: string;
    firstName: string;
    lastName: string;
    weight: number;
    height: number;
    activeDays: number;
    birthDate: string;
    technicalPoints: string | null;
}

export interface RegisterUserBody {
    email: string;
    username: string;
    password: string;
    firstName: string;
    lastName: string;
    weight: number;
    height: number;
    activeDays: number;
    birthDate: string;
}

export interface RegisterUserResponse {
    token: string;
}

export interface LoginUserBody {
    email: string;
    password: string;
}

export interface LoginUserResponse {
    token: string;
}

export interface UpdateUserInfoBody {
    email: string;
    username: string;
    firstName: string;
    lastName: string;
    weight: number;
    height: number;
    activeDays: number;
    birthDate: string;
}

export interface UpdateUserInfoResponse {
    userId: string;
}

export interface ChangeUserPasswordBody {
    oldPassword: string;
    newPassword: string;
}

export interface UpdateTechnicalPointsBody {
    technicalPoints: string | null;
}
