import { fetchFromServer } from "../../assets/js/data/apiClient.ts";

export interface StravaStatus {
    connected: boolean;
    athleteName: string | null;
    connectedAt: string | null;
}

export async function getStravaStatus(): Promise<StravaStatus> {
    return fetchFromServer<StravaStatus>("strava/status", "GET");
}

export async function getStravaAuthUrl(): Promise<string> {
    const res = await fetchFromServer<{ url: string }>("strava/auth-url", "GET");
    return res.url;
}

export async function disconnectStrava(): Promise<void> {
    await fetchFromServer<void>("strava/disconnect", "DELETE");
}

export async function importStravaHistory(): Promise<{ imported: number }> {
    return fetchFromServer<{ imported: number }>("strava/import", "POST");
}

export async function syncStrava(): Promise<{ imported: number }> {
    return fetchFromServer<{ imported: number }>("strava/sync", "POST");
}
