import { fetchFromServer } from "../../assets/js/data/apiClient.ts";
import type { User, UpdateUserInfoBody, UpdateTechnicalPointsBody } from "../../assets/js/data/apiTypes.ts";

export type { User };

export async function login(email: string, password: string): Promise<void> {
    await fetchFromServer<void>("users/login", "POST", { email, password });
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
): Promise<void> {
    await fetchFromServer<void>("users/register", "POST", {
        email, password, height, weight, username, firstName, lastName, activeDays,
    });
}

export async function logout(): Promise<void> {
    await fetchFromServer<void>("users/logout", "POST");
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

export async function updateTechnicalPoints(technicalPoints: string | null): Promise<void> {
    await fetchFromServer<unknown>("users/me/technical-points", "PUT", { technicalPoints } satisfies UpdateTechnicalPointsBody);
}
