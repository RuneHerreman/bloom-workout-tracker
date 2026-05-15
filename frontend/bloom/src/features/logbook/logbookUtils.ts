import type { LoggedWorkout } from "./api.ts";
import {
    dominantType as _dominantType,
    formatDate,
    relativeDate,
    toDateInputValue,
    displayDuration,
    type WorkoutFilterType,
} from "../../utils/workoutUtils.ts";

export type LogFilterType = WorkoutFilterType;
export { formatDate, relativeDate, toDateInputValue, displayDuration };

export function dominantTypeFromLog(log: LoggedWorkout): "cardio" | "strength" | "plyometric" | "mix" {
    return _dominantType(log.exercises.flatMap(ex => ex.sets));
}

export function matchesLogFilter(log: LoggedWorkout, filter: LogFilterType): boolean {
    return log.exercises.flatMap(ex => ex.sets).some(s => s.type === filter);
}
