import { fetchFromServer } from "../../assets/js/data/apiClient.ts";

export interface LeaderboardEntry {
    username: string;
    isCurrentUser: boolean;
    totalVolumeKg: number;
    newPrs: number;
    activeDays: number;
    workouts: number;
}

export interface Leaderboard {
    year: number;
    month: number;
    entries: LeaderboardEntry[];
}

export type LeaderboardMetric = "volume" | "prs" | "days";

export const METRICS: { value: LeaderboardMetric; label: string; unit: string; describe: string }[] = [
    { value: "volume", label: "Volume",      unit: "kg",   describe: "Reps × weight over working sets" },
    { value: "prs",    label: "New PRs",     unit: "PRs",  describe: "Exercises where you beat your previous best" },
    { value: "days",   label: "Active days", unit: "days", describe: "Days with at least one workout" },
];

export function metricValue(e: LeaderboardEntry, m: LeaderboardMetric): number {
    return m === "volume" ? e.totalVolumeKg : m === "prs" ? e.newPrs : e.activeDays;
}

const cache = new Map<string, Promise<Leaderboard>>();

/** Past months are immutable enough to cache for the session; the current month is always fetched fresh. */
export function getLeaderboard(year: number, month: number): Promise<Leaderboard> {
    const path = `leaderboard?year=${year}&month=${month}`;
    const now = new Date();
    if (year === now.getUTCFullYear() && month === now.getUTCMonth() + 1) {
        return fetchFromServer<Leaderboard>(path, "GET");
    }
    const key = `${year}-${month}`;
    let p = cache.get(key);
    if (!p) {
        p = fetchFromServer<Leaderboard>(path, "GET")
            .catch(e => { cache.delete(key); throw e; });
        cache.set(key, p);
    }
    return p;
}
