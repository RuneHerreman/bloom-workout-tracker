/**
 * TypeScript type definitions derived from OpenAPI spec (bloommain--v1.yaml)
 * These ensure type safety when working with API responses
 */

// ==================== SHARED TYPES ====================

export interface MonthlyVolumeResponse {
    year: number;
    month: number;
    maxWeight: number;
    weightUnit: string;
}

// ==================== EXERCISE TYPES ====================

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

// ==================== LOGGED SETS & EXERCISES ====================

export interface LoggedSet {
    type: string;
    order: number;
    duration: string | null;
    distance: number | null;
    distanceUnit: string | null;
    reps: number | null;
    weight: number | null;
    weightUnit: string | null;
    rir: number | null;
}

export interface LoggedSetBody {
    type: string;
    order: number;
    duration: string | null;
    distance: number | null;
    distanceUnit: string | null;
    reps: number | null;
    weight: number | null;
    weightUnit: string | null;
    rir: number | null;
}

export interface LoggedExercise {
    exerciseId: string;
    order: number;
    sets: LoggedSet[];
}

export interface LoggedExerciseBody {
    exerciseId: string;
    order: number;
    sets: LoggedSetBody[];
}

// ==================== WORKOUT LOG TYPES ====================

export interface LoggedWorkout {
    id: string;
    userId: string;
    loggedAt: string;
    exercises: LoggedExercise[];
}

export interface CreateLoggedWorkoutBody {
    exercises: LoggedExerciseBody[];
    loggedAt?: string | null;
}

export interface CreateLoggedWorkoutResponse {
    loggedWorkoutId: string;
}

export interface UpdateLoggedWorkoutBody {
    loggedAt: string;
    exercises: LoggedExerciseBody[];
}

export interface UpdateLoggedWorkoutResponse {
    loggedWorkoutId: string;
}

// ==================== PLANNED SETS & EXERCISES ====================

export interface PlannedSet {
    type: string;
    order: number;
    reps: number | null;
    duration: string | null;
    distance: number | null;
    distanceUnit: string | null;
}

export interface PlannedSetBody {
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

export interface TemplateExerciseBody {
    exerciseId: string;
    order: number;
    sets: PlannedSetBody[];
}

// ==================== WORKOUT TEMPLATE TYPES ====================

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

// ==================== USER TYPES ====================

export interface User {
    id: string;
    email: string;
    username: string;
    weight: number;
    height: number;
    activeDays: number;
}

export interface RegisterUserBody {
    email: string;
    username: string;
    password: string;
    weight: number;
    height: number;
    activeDays: number;
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
    weight: number;
    height: number;
    activeDays: number;
}

export interface UpdateUserInfoResponse {
    userId: string;
}

export interface ChangeUserPasswordBody {
    oldPassword: string;
    newPassword: string;
}
