// Whether saved logs open in the compact read view (then you click Edit) instead of the editor.
// A per-device display choice, like the log summary's muscle view.
const KEY = "bloom.logbook.openReadOnly";

export function readOpenReadOnly(): boolean {
    try { return localStorage.getItem(KEY) === "1"; } catch { return false; }
}

export function writeOpenReadOnly(on: boolean): void {
    try { localStorage.setItem(KEY, on ? "1" : "0"); } catch { /* private mode: just don't remember */ }
}
