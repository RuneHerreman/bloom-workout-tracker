import type { WorkoutTemplate } from "./api.ts";
import { dominantType as _dominantType, parseDuration, formatDuration, type WorkoutFilterType } from "../../utils/workoutUtils.ts";

export type FilterType = WorkoutFilterType;
export { parseDuration, formatDuration };

export function dominantType(template: WorkoutTemplate): "cardio" | "strength" | "plyometric" | "mix" {
    return _dominantType(template.exercises.flatMap(ex => ex.sets));
}

export function matchesFilter(template: WorkoutTemplate, filter: FilterType): boolean {
    return template.exercises.flatMap(ex => ex.sets).some(s => s.type === filter);
}
