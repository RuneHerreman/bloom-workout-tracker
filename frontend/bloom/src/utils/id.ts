/**
 * A random id for client-side list keys (sets, rows).
 *
 * `crypto.randomUUID()` only exists on secure pages (HTTPS or localhost); when the
 * app is served over plain http on a LAN or tailnet host it's undefined and
 * throws. `crypto.getRandomValues()` works everywhere, so fall back to building a
 * version-4 UUID from it.
 */
export function newId(): string {
    if (typeof crypto.randomUUID === "function") return crypto.randomUUID();
    const b = crypto.getRandomValues(new Uint8Array(16));
    b[6] = (b[6] & 0x0f) | 0x40; // version 4
    b[8] = (b[8] & 0x3f) | 0x80; // RFC 4122 variant
    const hex = Array.from(b, x => x.toString(16).padStart(2, "0")).join("");
    return `${hex.slice(0, 8)}-${hex.slice(8, 12)}-${hex.slice(12, 16)}-${hex.slice(16, 20)}-${hex.slice(20)}`;
}
