import type { ExerciseVolumeResponse, LoggedWorkout } from "../../assets/js/data/apiTypes.ts";
import type { Exercise } from "../exercises/api.ts";
import type { ExerciseType } from "../../types.ts";

const MONTH_NAMES = ["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"];

export function getDateRange(period: string): { from: Date; to: Date } {
    const to = new Date();
    const from = new Date();
    if (period === "max") return { from: new Date(2000, 0, 1), to };
    if (period === "1m") { from.setMonth(from.getMonth() - 1); return { from, to }; }
    if (period === "3m") { from.setMonth(from.getMonth() - 3); return { from, to }; }
    if (period === "6m") { from.setMonth(from.getMonth() - 6); return { from, to }; }
    if (period === "1y") { from.setFullYear(from.getFullYear() - 1); return { from, to }; }
    const year = parseInt(period);
    if (!isNaN(year)) return { from: new Date(year, 0, 1), to: new Date(year, 11, 31, 23, 59) };
    return { from: new Date(0), to };
}

export function filterVolumeByPeriod(data: ExerciseVolumeResponse[], period: string): ExerciseVolumeResponse[] {
    const { from, to } = getDateRange(period);
    return data
        .map(ex => ({
            ...ex,
            monthlyVolume: ex.monthlyVolume.filter(m => {
                const d = new Date(m.year, m.month - 1, 1);
                return d >= from && d <= to;
            }),
        }))
        .filter(ex => ex.monthlyVolume.length > 0);
}

export function filterLogsByPeriod(logs: LoggedWorkout[], period: string): LoggedWorkout[] {
    const { from, to } = getDateRange(period);
    return logs.filter(l => {
        const d = new Date(l.loggedAt);
        return d >= from && d <= to;
    });
}

export interface DerivedPR {
    exerciseId: string;
    exerciseName: string;
    exerciseType: ExerciseType;
    targetMuscles: string[];
    weight: number;
    weightUnit: string;
}

export function derivePRsFromLogs(
    logs: LoggedWorkout[],
    exerciseMap: Map<string, Exercise>
): DerivedPR[] {
    const maxMap = new Map<string, DerivedPR>();

    logs.forEach(log => {
        log.exercises.forEach(ex => {
            ex.sets
                .filter(s => s.weight != null && s.weight > 0)
                .forEach(s => {
                    const current = maxMap.get(ex.exerciseId);
                    if (!current || s.weight! > current.weight) {
                        const info = exerciseMap.get(ex.exerciseId);
                        maxMap.set(ex.exerciseId, {
                            exerciseId: ex.exerciseId,
                            exerciseName: info?.name ?? ex.exerciseId,
                            exerciseType: (info?.type ?? "Strength") as ExerciseType,
                            targetMuscles: info?.targetMuscles ?? [],
                            weight: s.weight!,
                            weightUnit: s.weightUnit ?? "kg",
                        });
                    }
                });
        });
    });

    return Array.from(maxMap.values());
}

export interface CardioMonth {
    label: string;
    year: number;
    month: number;
    sessions: number;
    totalDistanceKm: number;
    totalDurationMin: number;
}

export function extractCardioMonthly(logs: LoggedWorkout[]): CardioMonth[] {
    const monthMap = new Map<string, CardioMonth>();

    logs.forEach(log => {
        const hasCardio = log.exercises.some(ex =>
            ex.sets.some(s => s.type?.toLowerCase() === "cardio")
        );
        if (!hasCardio) return;

        const d = new Date(log.loggedAt);
        const year = d.getFullYear();
        const month = d.getMonth() + 1;
        const key = `${year}-${month}`;

        if (!monthMap.has(key)) {
            monthMap.set(key, {
                label: `${MONTH_NAMES[month - 1]} ${year}`,
                year, month,
                sessions: 0,
                totalDistanceKm: 0,
                totalDurationMin: 0,
            });
        }

        const entry = monthMap.get(key)!;
        entry.sessions += 1;

        log.exercises.forEach(ex => {
            ex.sets
                .filter(s => s.type?.toLowerCase() === "cardio")
                .forEach(s => {
                    if (s.duration) {
                        const parts = s.duration.split(":").map(Number);
                        entry.totalDurationMin += (parts[0] ?? 0) * 60 + (parts[1] ?? 0) + (parts[2] ?? 0) / 60;
                    }
                    if (s.distance != null && s.distanceUnit) {
                        let km = s.distance;
                        if (s.distanceUnit === "miles") km *= 1.60934;
                        else if (s.distanceUnit === "meters") km /= 1000;
                        entry.totalDistanceKm += km;
                    }
                });
        });
    });

    return Array.from(monthMap.values()).sort((a, b) =>
        a.year !== b.year ? a.year - b.year : a.month - b.month
    );
}

export function getAvailableYears(
    volumeData: ExerciseVolumeResponse[],
    logs: LoggedWorkout[]
): number[] {
    const years = new Set<number>();
    volumeData.forEach(ex => ex.monthlyVolume.forEach(m => years.add(m.year)));
    logs.forEach(l => years.add(new Date(l.loggedAt).getFullYear()));
    return Array.from(years).sort((a, b) => b - a);
}

export function buildVolumeChartSeries(data: ExerciseVolumeResponse[], limit = 5): {
    series: { name: string; data: (number | null)[] }[];
    labels: string[];
} {
    if (data.length === 0) return { series: [], labels: [] };

    const withPrDate = data.map(ex => {
        const sorted = [...ex.monthlyVolume].sort((a, b) =>
            a.year !== b.year ? a.year - b.year : a.month - b.month
        );
        let historicalMax = 0;
        let lastPrScore = 0;
        sorted.forEach(m => {
            const w = typeof m.maxWeight === "string" ? parseFloat(m.maxWeight) : m.maxWeight;
            if (w > historicalMax) { historicalMax = w; lastPrScore = m.year * 12 + m.month; }
        });
        return { ...ex, lastPrScore };
    });

    const sorted = withPrDate.sort((a, b) => b.lastPrScore - a.lastPrScore).slice(0, limit);

    const monthsMap = new Map<string, { year: number; month: number }>();
    sorted.forEach(ex => ex.monthlyVolume.forEach(m =>
        monthsMap.set(`${m.year}-${m.month}`, { year: m.year, month: m.month })
    ));

    const sortedMonths = Array.from(monthsMap.values()).sort((a, b) =>
        a.year !== b.year ? a.year - b.year : a.month - b.month
    );

    const labels = sortedMonths.map(m => `${MONTH_NAMES[m.month - 1]} ${m.year}`);

    const series = sorted.map(ex => {
        let lastKnown: number | null = null;
        return {
            name: ex.exerciseName,
            data: sortedMonths.map(sm => {
                const match = ex.monthlyVolume.find(m => m.year === sm.year && m.month === sm.month);
                if (match) {
                    const w = typeof match.maxWeight === "string" ? parseFloat(match.maxWeight) : match.maxWeight;
                    if (w > 0) lastKnown = w;
                }
                return lastKnown;
            }),
        };
    });

    return { series, labels };
}
