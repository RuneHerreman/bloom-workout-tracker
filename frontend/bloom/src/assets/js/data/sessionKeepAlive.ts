// The API slides the session: any authenticated request made once the token is
// past half its lifetime gets a fresh one. Someone working in the UI without
// hitting the server (a long form, parsing an import file) would still time
// out, so while the user is active we ping at most once per interval.

export const KEEPALIVE_INTERVAL_MS = 10 * 60_000;

export interface SessionKeepAlive {
    /** Any response from the server: the session was just used, so no ping is needed for a while. */
    contact(): void;
    /** The user did something in the UI. Pings when the server hasn't been reached for an interval. */
    activity(): void;
}

export function createSessionKeepAlive(
    ping: () => Promise<unknown>,
    now: () => number = Date.now,
    intervalMs: number = KEEPALIVE_INTERVAL_MS,
): SessionKeepAlive {
    let lastContact = now();
    let pinging = false;

    return {
        contact() {
            lastContact = now();
        },
        activity() {
            if (pinging || now() - lastContact < intervalMs) return;
            pinging = true;
            lastContact = now();
            ping().catch(() => {}).finally(() => { pinging = false; });
        },
    };
}
