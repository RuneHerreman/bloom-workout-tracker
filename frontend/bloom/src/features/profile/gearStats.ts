import type { LoggedWorkout } from "../../assets/js/data/apiTypes.ts";

export interface GearUsage {
    sessions: number;
    distanceKm: number;
    lastUsed: string | null;
}

function toKm(distance: number | null, unit: string | null): number {
    if (distance == null) return 0;
    switch ((unit ?? "km").toLowerCase()) {
        case "miles": return distance * 1.609344;
        case "meters": return distance / 1000;
        default: return distance;
    }
}

/** Per-gear usage across all logs: how often it was used and how far it has travelled. */
export function computeGearUsage(logs: LoggedWorkout[]): Map<string, GearUsage> {
    const usage = new Map<string, GearUsage>();
    for (const log of logs) {
        for (const ex of log.exercises) {
            for (const g of ex.gear ?? []) {
                const u = usage.get(g) ?? { sessions: 0, distanceKm: 0, lastUsed: null };
                u.sessions += 1;
                u.distanceKm += ex.sets.reduce((km, s) => km + toKm(s.distance, s.distanceUnit), 0);
                if (!u.lastUsed || log.loggedAt > u.lastUsed) u.lastUsed = log.loggedAt;
                usage.set(g, u);
            }
        }
    }
    return usage;
}
