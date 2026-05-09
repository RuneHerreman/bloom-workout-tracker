import { fetchFromServer } from "../../assets/js/data/apiClient.ts";

export interface User {
    id: string;
    email: string;
    username: string;
    weight: number;
    height: number;
    activeDays: number;
}

export interface UpdateUserBody {
    email: string;
    username: string;
    weight: number;
    height: number;
    activeDays: number;
}

export async function login(email: string, password: string): Promise<string> {
    const response = await fetchFromServer<{ token: string }>("users/login", "POST", { email, password });
    return response.token;
}

export async function register(
    email: string,
    password: string,
    height: number,
    weight: number,
    username: string,
    activeDays: number
): Promise<string> {
    const response = await fetchFromServer<{ token: string }>("users/register", "POST", {
        email,
        password,
        height,
        weight,
        username,
        activeDays,
    });
    return response.token;
}

export async function getMe(): Promise<User> {
    return fetchFromServer<User>("users/me", "GET");
}

export async function updateMe(body: UpdateUserBody): Promise<void> {
    await fetchFromServer<unknown>("users/me", "PUT", body);
}

export async function deleteMe(): Promise<void> {
    await fetchFromServer<unknown>("users/me", "DELETE");
}

export async function changePassword(oldPassword: string, newPassword: string): Promise<void> {
    await fetchFromServer<unknown>("users/me/change-password", "POST", { oldPassword, newPassword });
}
