import { fetchFromServer } from "../../assets/js/data/apiClient.ts";
import type { User, UpdateUserInfoBody } from "../../assets/js/data/apiTypes.ts";

export type { User };

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
    firstName: string,
    lastName: string,
    activeDays: number
): Promise<string> {
    const response = await fetchFromServer<{ token: string }>("users/register", "POST", {
        email, password, height, weight, username, firstName, lastName, activeDays,
    });
    return response.token;
}

export async function getMe(): Promise<User> {
    return fetchFromServer<User>("users/me", "GET");
}

export async function updateMe(body: UpdateUserInfoBody): Promise<void> {
    await fetchFromServer<unknown>("users/me", "PUT", body);
}

export async function deleteMe(): Promise<void> {
    await fetchFromServer<unknown>("users/me", "DELETE");
}

export async function changePassword(oldPassword: string, newPassword: string): Promise<void> {
    await fetchFromServer<unknown>("users/me/change-password", "POST", { oldPassword, newPassword });
}
