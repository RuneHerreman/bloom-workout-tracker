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

// Pages where a 401 is expected (not logged in yet) and must not trigger a redirect.
const PUBLIC_PATHS = ["/", "/login", "/signup"];

export async function fetchFromServer<T>(
    path: string,
    httpVerb: string,
    requestBody?: unknown
): Promise<T> {
    const options = constructOptions(httpVerb, requestBody);
    const response = await fetch(`${API_BASE_URL}${path}`, options);

    if (response.status === 401 && !PUBLIC_PATHS.includes(window.location.pathname)) {
        // Session expired mid-use: send the user back to login instead of
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
