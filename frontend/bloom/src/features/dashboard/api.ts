import { fetchFromServer } from "../../assets/js/data/apiClient.ts";
import type { ExercisePrResponse, ExerciseVolumeResponse, LoggedWorkout } from "../../assets/js/data/apiTypes.ts";

export async function getLogs(): Promise<LoggedWorkout[]> {
    return fetchFromServer<LoggedWorkout[]>("logs", "GET");
}

export async function getPRs(): Promise<ExercisePrResponse[]> {
    return fetchFromServer<ExercisePrResponse[]>("logs/pr", "GET");
}

export async function getVolume(): Promise<ExerciseVolumeResponse[]> {
    return fetchFromServer<ExerciseVolumeResponse[]>("logs/volume", "GET");
}
