import { useState } from "react";

const KEY = "bloom:recent-exercises";
const MAX = 6;

function readIds(): string[] {
    try {
        const raw = localStorage.getItem(KEY);
        if (!raw) return [];
        const parsed = JSON.parse(raw);
        return Array.isArray(parsed) ? parsed.filter(x => typeof x === "string") : [];
    } catch {
        return [];
    }
}

function writeIds(ids: string[]) {
    try {
        localStorage.setItem(KEY, JSON.stringify(ids));
    } catch {
        // quota exceeded or private browsing — silently ignore
    }
}

export function useRecentExercises() {
    const [recentIds, setRecentIds] = useState<string[]>(readIds);

    function addRecent(id: string) {
        const updated = [id, ...recentIds.filter(x => x !== id)].slice(0, MAX);
        setRecentIds(updated);
        writeIds(updated);
    }

    return { recentIds, addRecent };
}