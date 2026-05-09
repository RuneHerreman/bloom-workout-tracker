import { type ApiError, fetchFromServer } from "../../lib/apiClient.ts";

interface LoginResponse {
    token: string;
}

export async function login(email: string, password: string): Promise<string> {
    const response = await fetchFromServer<LoginResponse>("auth/login", "POST", {
        email,
        password,
    });

    if (!("token" in response)) {
        throw response as ApiError;
    }

    return response.token;
}

export async function register(
    email: string,
    password: string,
    height: number,
    weight: number,
    name: string,
    activeDays: number
): Promise<string> {
    const response = await fetchFromServer<LoginResponse>("auth/register", "POST", {
        email,
        password,
        height,
        weight,
        name,
        activeDays,
    });

    if (!("token" in response)) {
        throw response as ApiError;
    }

    return response.token;
}
