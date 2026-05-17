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
    activeDays: number,
    birthDate: string
): Promise<void> {
    await fetchFromServer<void>("users/register", "POST", {
        email, password, height, weight, username, firstName, lastName, activeDays, birthDate,
    });
}

export async function logout(): Promise<void> {
    await fetchFromServer<void>("users/logout", "POST");
}

let _meCache: Promise<User> | null = null;

export function getMe(): Promise<User> {
    if (!_meCache) {
        _meCache = fetchFromServer<User>("users/me", "GET")
            .catch(e => { _meCache = null; throw e; });
    }
    return _meCache;
}

export async function updateMe(body: UpdateUserInfoBody): Promise<void> {
    await fetchFromServer<unknown>("users/me", "PUT", body);
    _meCache = null;
}

export async function deleteMe(): Promise<void> {
    await fetchFromServer<unknown>("users/me", "DELETE");
    _meCache = null;
}

export async function changePassword(oldPassword: string, newPassword: string): Promise<void> {
    await fetchFromServer<unknown>("users/me/change-password", "POST", { oldPassword, newPassword });
}

export async function updateTechnicalPoints(technicalPoints: string | null): Promise<void> {
    await fetchFromServer<unknown>("users/me/technical-points", "PUT", { technicalPoints } satisfies UpdateTechnicalPointsBody);
}
