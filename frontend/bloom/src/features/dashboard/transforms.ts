import type { ExercisePrResponse, ExerciseVolumeResponse, LoggedWorkout } from "../../assets/js/data/apiTypes.ts";
import type { ActivityDay } from "./components/ActivityWidget";
import type { ExerciseSeries } from "./components/VolumeWidget";
import type { FocusSegment } from "./components/TrainingFocusWidget";
import type { LogEntryData } from "./components/LogWidget";
export interface DashboardStats {
    workoutsThisYear: number;
    workoutChange?: number;
    volumeThisMonth: string;
    volumeChange?: number;
    currentStreak: number;
    bestStreak: number;
    totalPRs: number;
}

export function calculateDashboardStats(workouts: LoggedWorkout[], prs: ExercisePrResponse[]): DashboardStats {
    const now = new Date();
    const currentYear = now.getFullYear();
    const currentMonth = now.getMonth();

    // 1. Workouts this year & % change from last year
    const thisYearCount = workouts.filter(w => new Date(w.loggedAt).getFullYear() === currentYear).length;
    const lastYearCount = workouts.filter(w => new Date(w.loggedAt).getFullYear() === currentYear - 1).length;

    // Require a minimum baseline of 5 workouts last year to show a percentage
    const workoutChange = lastYearCount >= 5
        ? Math.round(((thisYearCount - lastYearCount) / lastYearCount) * 100)
        : undefined;

    // 2. Volume this month (Sum of weight * reps) & % change
    const calcVolume = (year: number, month: number) => {
        return workouts.reduce((total, w) => {
            const d = new Date(w.loggedAt);
            if (d.getFullYear() === year && d.getMonth() === month) {
                const workoutVol = w.exercises.reduce((exTotal, ex) => {
                    return exTotal + ex.sets.reduce((setTotal, set) => {
                        return setTotal + ((set.weight || 0) * (set.reps || 0));
                    }, 0);
                }, 0);
                return total + workoutVol;
            }
            return total;
        }, 0);
    };

    const volThisMonth = calcVolume(currentYear, currentMonth);
    const prevMonthYear = currentMonth === 0 ? currentYear - 1 : currentYear;
    const prevMonth = currentMonth === 0 ? 11 : currentMonth - 1;
    const volLastMonth = calcVolume(prevMonthYear, prevMonth);

    const volumeThisMonthStr = volThisMonth >= 1000 ? (volThisMonth / 1000).toFixed(1) + 'k' : volThisMonth.toString();
    const volumeChange = volLastMonth > 0
        ? Math.round(((volThisMonth - volLastMonth) / volLastMonth) * 100)
        : undefined;

    // 3. Active & Best Streak (Consecutive days)
    const dates = [...new Set(workouts.map(w => w.loggedAt.slice(0, 10)))].sort((a, b) => b.localeCompare(a));

    let currentStreak = 0;
    let bestStreak = 0;

    if (dates.length > 0) {
        let tempStreak = 1;
        let maxStreak = 1;

        // Calculate best streak overall
        for (let i = 0; i < dates.length - 1; i++) {
            const d1 = new Date(dates[i]);
            const d2 = new Date(dates[i + 1]);
            const diffDays = Math.round(Math.abs(d1.getTime() - d2.getTime()) / (1000 * 60 * 60 * 24));

            if (diffDays === 1) {
                tempStreak++;
                maxStreak = Math.max(maxStreak, tempStreak);
            } else {
                tempStreak = 1;
            }
        }
        bestStreak = Math.max(maxStreak, tempStreak);

        // Calculate current streak (checks if the latest workout was today or yesterday)
        const getLocalDateStr = (d: Date) => {
            const pad = (n: number) => String(n).padStart(2, '0');
            return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
        };

        const todayStr = getLocalDateStr(now);
        const yesterday = new Date(now);
        yesterday.setDate(now.getDate() - 1);
        const yesterdayStr = getLocalDateStr(yesterday);

        if (dates[0] === todayStr || dates[0] === yesterdayStr) {
            currentStreak = 1;
            for (let i = 0; i < dates.length - 1; i++) {
                const d1 = new Date(dates[i]);
                const d2 = new Date(dates[i + 1]);
                const diffDays = Math.round(Math.abs(d1.getTime() - d2.getTime()) / (1000 * 60 * 60 * 24));

                if (diffDays === 1) {
                    currentStreak++;
                } else {
                    break;
                }
            }
        }
    }

    // 4. PRs
    const totalPRs = prs.length;

    return {
        workoutsThisYear: thisYearCount,
        workoutChange,
        volumeThisMonth: volumeThisMonthStr,
        volumeChange,
        currentStreak,
        bestStreak,
        totalPRs
    };
}

/**
 * Transform ExerciseVolumeResponse[] to ExerciseSeries[] for VolumeWidget
 */
export function transformVolumeDataToSeries(volumeData: ExerciseVolumeResponse[]): { series: ExerciseSeries[]; labels: string[] } {
    // Find all unique year/month pairs
    const uniqueMonthsMap = new Map<string, { year: number; month: number }>();
    volumeData.forEach(ex => {
        ex.monthlyVolume.forEach(m => {
            uniqueMonthsMap.set(`${m.year}-${m.month}`, { year: m.year, month: m.month });
        });
    });

    // Sort chronologically
    const sortedMonths = Array.from(uniqueMonthsMap.values()).sort((a, b) => {
        if (a.year !== b.year) return a.year - b.year;
        return a.month - b.month;
    });

    const monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    const labels = sortedMonths.map(m => `${monthNames[m.month - 1]} ${m.year}`);

    const series = volumeData.map((exercise) => {
        const data = sortedMonths.map(sm => {
            const match = exercise.monthlyVolume.find(m => m.year === sm.year && m.month === sm.month);
            if (!match) return 0;
            return typeof match.maxWeight === "string" ? parseFloat(match.maxWeight) : match.maxWeight;
        });

        return {
            name: exercise.exerciseName,
            data,
        };
    });

    return { series, labels };
}

/**
 * Transform LoggedWorkout[] to LogEntryData[] for LogWidget
 */
export function transformWorkoutLogsToEntries(workouts: LoggedWorkout[]): LogEntryData[] {
    return workouts.map((workout) => {
        // Map each exercise to its type
        const exerciseTypes = workout.exercises.map(ex => {
            // Check the first set to determine if the exercise is cardio or strength
            const firstSetType = ex.sets[0]?.type?.toLowerCase() || "strength";
            return firstSetType;
        });

        return {
            id: workout.id,
            name: inferWorkoutName(workout),
            date: formatWorkoutDate(new Date(workout.loggedAt)),
            exerciseCount: workout.exercises.length,
            exerciseTypes: exerciseTypes,
        };
    });
}

/**
 * Transform ExercisePrResponse[] to FocusSegment[] for TrainingFocusWidget
 */
export function transformPrDataToFocus(prData: ExercisePrResponse[]): FocusSegment[] {
    const typeMap = new Map<string, { count: number; color: string }>();
    const colors: { [key: string]: string } = {
        "strength": "#003E1F",
        "cardio": "#E9762B",
        "plyometric": "#2D8055",
        "flexibility": "#7B1616",
        "recovery": "#595959",
    };

    prData.forEach(pr => {
        const type = pr.exerciseType?.toLowerCase() ?? "strength";
        const existing = typeMap.get(type) || { count: 0, color: colors[type] || "#595959" };
        typeMap.set(type, { count: existing.count + 1, color: existing.color });
    });

    return Array.from(typeMap.entries()).map(([label, { count, color }]) => ({
        label: label.charAt(0).toUpperCase() + label.slice(1),
        value: count,
        color,
    }));
}

/**
 * Transform LoggedWorkout[] to ActivityDay[] for ActivityWidget
 * Colors by workout type: Level 1 = cardio, Level 2 = strength, Level 3 = mix
 */
export function transformLogsToActivityData(workouts: LoggedWorkout[]): ActivityDay[] {
    const today = new Date();
    const days: ActivityDay[] = [];

    for (let w = 51; w >= 0; w--) {
        for (let d = 6; d >= 0; d--) {
            const date = new Date(today);
            date.setDate(today.getDate() - w * 7 - d);
            const dateStr = date.toISOString().slice(0, 10);

            const workout = workouts.find(w => w.loggedAt.slice(0, 10) === dateStr);

            let level: 0 | 1 | 2 | 3 = 0;
            if (workout) {
                const allSets = workout.exercises.flatMap(ex => ex.sets);
                const cardioCount = allSets.filter(s => s.type?.toLowerCase() === "cardio").length;
                const strengthCount = allSets.filter(s => s.type?.toLowerCase() === "strength").length;
                const plyoCount = allSets.filter(s => s.type?.toLowerCase() === "plyometric").length;

                const totalCardio = cardioCount;
                const totalStrength = strengthCount + plyoCount;

                if (totalCardio > 0 && totalStrength > 0) {
                    level = 3;
                } else if (totalCardio > 0) {
                    level = 1;
                } else if (totalStrength > 0) {
                    level = 2;
                }
            }

            days.push({ date: dateStr, level });
        }
    }

    return days;
}

function inferWorkoutName(workout: LoggedWorkout): string {
    const exerciseIdMap: { [key: string]: string } = {
        "550e8400-e29b-41d4-a716-446655440001": "Bench Press",
        "550e8400-e29b-41d4-a716-446655440002": "Squat",
        "550e8400-e29b-41d4-a716-446655440003": "Deadlift",
        "550e8400-e29b-41d4-a716-446655440004": "Overhead Press",
        "550e8400-e29b-41d4-a716-446655440005": "Barbell Row",
    };

    const exerciseNames = workout.exercises
        .map(ex => exerciseIdMap[ex.exerciseId] || "")
        .join(" ")
        .toLowerCase();

    if (exerciseNames.includes("bench") || exerciseNames.includes("press")) return "Push";
    if (exerciseNames.includes("row") || exerciseNames.includes("pull")) return "Pull";
    if (exerciseNames.includes("squat") || exerciseNames.includes("deadlift")) return "Legs";
    if (exerciseNames.includes("cardio") || exerciseNames.includes("run")) return "Cardio";

    return "Workout";
}

function formatWorkoutDate(date: Date): string {
    return date.toLocaleDateString("en-GB", {
        weekday: "long",
        day: "numeric",
        month: "long",
        year: "numeric",
    });
}
