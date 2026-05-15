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

export async function fetchFromServer<T>(
    path: string,
    httpVerb: string,
    requestBody?: unknown
): Promise<T> {
    const options = constructOptions(httpVerb, requestBody);
    const response = await fetch(`${API_BASE_URL}${path}`, options);

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
