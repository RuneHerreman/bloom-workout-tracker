export const API_BASE_URL: string =
    import.meta.env.VITE_API_BASE_URL ?? "http://localhost:8080/api/";

export interface ApiError {
    failure: true;
    message?: string;
    error?: string;
}

function constructOptions(httpVerb: string, requestBody?: unknown): RequestInit {
    const options: RequestInit = {
        method: httpVerb,
        credentials: "include",
        headers: {
            "Content-Type": "application/json",
        },
    };

    if (requestBody !== undefined) {
        options.body = JSON.stringify(requestBody);
    }

    return options;
}

const serverContactListeners = new Set<() => void>();

/** Called after every authenticated-or-public response that isn't a 401. Returns an unsubscribe. */
export function onServerContact(listener: () => void): () => void {
    serverContactListeners.add(listener);
    return () => { serverContactListeners.delete(listener); };
}

// Pages where a 401 is expected (not logged in yet) and must not trigger a redirect.
const PUBLIC_PATHS = ["/", "/login", "/signup"];

export async function fetchFromServer<T>(
    path: string,
    httpVerb: string,
    requestBody?: unknown
): Promise<T> {
    const options = constructOptions(httpVerb, requestBody);
    let response: Response;
    try {
        response = await fetch(`${API_BASE_URL}${path}`, options);
    } catch {
        // No response at all (server down, offline, blocked): say so, rather than
        // letting callers fall back to "Login failed", which reads as a wrong password.
        const unreachable: ApiError = { failure: true, error: "Can't reach Bloom right now. Check your connection and try again." };
        throw unreachable;
    }

    if (response.status !== 401) {
        serverContactListeners.forEach(listener => listener());
    }

    if (response.status === 401 && !PUBLIC_PATHS.includes(window.location.pathname)) {
        // The server renews the session cookie on use, so no refresh-and-retry
        // is needed here: a 401 means the session really ended (idle timeout,
        // logout or a password change). Send the user back to login instead of
        // letting every widget fail with its own error.
        window.location.href = "/login";
        const sessionExpired: ApiError = { failure: true, error: "Session expired" };
        throw sessionExpired;
    }

    if (response.status === 204) {
        return undefined as T;
    }

    const json = await response.json();

    if (!response.ok || json.failure) {
        // Normalize ProblemDetails (detail/title) and legacy ApiError (error/message) into one shape
        const error: ApiError = {
            ...json,
            failure: true,
            error: json.error ?? json.detail ?? json.title ?? "An unexpected error occurred.",
            message: json.message ?? json.detail,
        };
        throw error;
    }

    return json as T;
}

// ── Synchronous cache reads ─────────────────────────────────────────────────
// Module-level promise caches (getLogs, getTemplates…) resolve asynchronously,
// so a page that mounts after the data is already loaded would still flash its
// skeleton for a frame. `remember` records each cached promise's value so pages
// can `peek` it and render real content on the very first frame.

const settled = new WeakMap<Promise<unknown>, unknown>();

export function remember<T>(promise: Promise<T>): Promise<T> {
    promise.then(value => { settled.set(promise, value); }, () => {});
    return promise;
}

export function peek<T>(promise: Promise<T> | null): T | undefined {
    return promise ? (settled.get(promise) as T | undefined) : undefined;
}
