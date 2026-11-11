import { fetchFromServer, remember, peek } from "../../assets/js/data/apiClient.ts";
import type { BodyEntry, LogBodyBody } from "../../assets/js/data/apiTypes.ts";
import { invalidateMe } from "../auth/api.ts";

export type { BodyEntry };

let _bodyCache: Promise<BodyEntry[]> | null = null;

/** The whole body log, oldest first. Cached; logging or deleting refreshes it. */
export function getBody(): Promise<BodyEntry[]> {
    if (!_bodyCache) {
        _bodyCache = remember(fetchFromServer<BodyEntry[]>("body", "GET")
            .catch(e => { _bodyCache = null; throw e; }));
    }
    return _bodyCache;
}

export function peekBody(): BodyEntry[] | undefined {
    return peek(_bodyCache);
}

/** Logs (or updates) a day. The latest day is also the profile weight, so that cache goes too. */
export async function logBody(date: string, body: LogBodyBody): Promise<BodyEntry> {
    const entry = await fetchFromServer<BodyEntry>(`body/${date}`, "PUT", body);
    _bodyCache = null;
    invalidateMe();
    return entry;
}

export async function deleteBody(date: string): Promise<void> {
    await fetchFromServer<void>(`body/${date}`, "DELETE");
    _bodyCache = null;
    invalidateMe();
}

/** Today in the viewer's time zone, as yyyy-MM-dd. */
export function localToday(now = new Date()): string {
    const y = now.getFullYear(), m = String(now.getMonth() + 1).padStart(2, "0"), d = String(now.getDate()).padStart(2, "0");
    return `${y}-${m}-${d}`;
}

/** "Mon 9 Nov", or with the year when it isn't this year. */
export function formatEntryDate(date: string, now = new Date()): string {
    const [y, m, d] = date.split("-").map(Number);
    const at = new Date(y, m - 1, d);
    return at.toLocaleDateString("en-GB", {
        weekday: "short", day: "numeric", month: "short", ...(y !== now.getFullYear() ? { year: "numeric" } : {}),
    });
}
