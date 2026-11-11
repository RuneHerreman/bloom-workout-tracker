import { describe, expect, it, vi } from "vitest";
import { createSessionKeepAlive, KEEPALIVE_INTERVAL_MS } from "./sessionKeepAlive.ts";

function setup() {
    let t = 0;
    const ping = vi.fn(() => Promise.resolve());
    const keepAlive = createSessionKeepAlive(ping, () => t);
    return { ping, keepAlive, advance: (ms: number) => { t += ms; } };
}

describe("createSessionKeepAlive", () => {
    it("does not ping while the server was reached recently", () => {
        const { ping, keepAlive, advance } = setup();
        advance(KEEPALIVE_INTERVAL_MS - 1);
        keepAlive.activity();
        expect(ping).not.toHaveBeenCalled();
    });

    it("pings on activity once an interval passed without server contact", () => {
        const { ping, keepAlive, advance } = setup();
        advance(KEEPALIVE_INTERVAL_MS);
        keepAlive.activity();
        keepAlive.activity();
        expect(ping).toHaveBeenCalledTimes(1);
    });

    it("counts regular API traffic as contact", () => {
        const { ping, keepAlive, advance } = setup();
        advance(KEEPALIVE_INTERVAL_MS - 1000);
        keepAlive.contact();
        advance(5000);
        keepAlive.activity();
        expect(ping).not.toHaveBeenCalled();
    });

    it("never pings without user activity, so an idle session still expires", () => {
        const { ping, advance } = setup();
        advance(10 * KEEPALIVE_INTERVAL_MS);
        expect(ping).not.toHaveBeenCalled();
    });

    it("keeps an active user's gaps between requests under the server's 30-minute renewal window", async () => {
        const { ping, keepAlive, advance } = setup();
        // Activity every minute for three hours, no other API traffic.
        for (let i = 0; i < 180; i++) {
            advance(60_000);
            keepAlive.activity();
            await Promise.resolve();
        }
        expect(ping.mock.calls.length).toBeGreaterThanOrEqual(180 * 60_000 / (KEEPALIVE_INTERVAL_MS + 60_000));
    });
});
