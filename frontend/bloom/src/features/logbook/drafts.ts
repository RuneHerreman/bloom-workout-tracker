import type { LoggedExercise, LoggedWorkout } from "../../assets/js/data/apiTypes.ts";

// A new log lives only in this browser until the user saves it, so abandoned
// sessions never create "Untitled Log" rows on the server. It is kept in
// localStorage so a refresh or a detour to another page doesn't lose it.

export const DRAFT_ID = "draft";
const STORAGE_KEY = "bloom.logDraft";

export function isDraftId(id: string | null | undefined): boolean {
    return id === DRAFT_ID;
}

export function createDraft(name: string, exercises: LoggedExercise[]): LoggedWorkout {
    return {
        id: DRAFT_ID,
        userId: "",
        loggedAt: new Date().toISOString(),
        name,
        note: null,
        exercises,
    };
}

export function loadDraft(): LoggedWorkout | null {
    try {
        const raw = localStorage.getItem(STORAGE_KEY);
        if (!raw) return null;
        const parsed = JSON.parse(raw) as LoggedWorkout;
        return parsed && Array.isArray(parsed.exercises) ? { ...parsed, id: DRAFT_ID } : null;
    } catch {
        return null;
    }
}

export function saveDraft(draft: LoggedWorkout | null): void {
    try {
        if (draft) localStorage.setItem(STORAGE_KEY, JSON.stringify(draft));
        else localStorage.removeItem(STORAGE_KEY);
    } catch {
        // Storage full or blocked (private mode): the draft still lives in memory.
    }
}
