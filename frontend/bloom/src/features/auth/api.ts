import { fetchFromServer, remember, peek } from "../../assets/js/data/apiClient.ts";
import type { Sex, User, UpdateUserInfoBody, UpdateTechnicalPointsBody } from "../../assets/js/data/apiTypes.ts";

export type { User };

export async function login(email: string, password: string, rememberMe = false): Promise<void> {
    await fetchFromServer<void>("users/login", "POST", { email, password, rememberMe });
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
        _meCache = remember(fetchFromServer<User>("users/me", "GET")
            .catch(e => { _meCache = null; throw e; }));
    }
    return _meCache;
}

export function peekMe(): User | undefined {
    return peek(_meCache);
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

export async function updateGear(gear: string[]): Promise<void> {
    await fetchFromServer<unknown>("users/me/gear", "PUT", { gear });
    _meCache = null;
}

export async function updateSex(sex: Sex | null): Promise<void> {
    await fetchFromServer<unknown>("users/me/sex", "PUT", { sex });
    _meCache = null;
}

/** Forget the cached profile, e.g. after the body log changed the current weight. */
export function invalidateMe(): void {
    _meCache = null;
}

export async function updateLeaderboardVisibility(showOnLeaderboard: boolean): Promise<void> {
    await fetchFromServer<unknown>("users/me/leaderboard-visibility", "PUT", { showOnLeaderboard });
    _meCache = null;
}
