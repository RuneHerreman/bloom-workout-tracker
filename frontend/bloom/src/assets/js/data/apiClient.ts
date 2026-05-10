export const API_BASE_URL: string = "http://localhost:8080/api/";

export interface ApiError {
    failure: true;
    message?: string;
    error?: string;
}

function constructOptions(httpVerb: string, requestBody?: unknown): RequestInit {
    const token = localStorage.getItem("jwt");
    const options: RequestInit = {
        method: httpVerb,
        headers: {
            "Content-Type": "application/json",
            ...(token ? { Authorization: `Bearer ${token}` } : {}),
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
    const json = await response.json();

    if (!response.ok || json.failure) {
        throw json as ApiError;
    }

    return json as T;
}
