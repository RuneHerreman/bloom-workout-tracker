// Reading dates and times out of spreadsheet cells: ISO, day/month or
// month/day with slashes, and Excel's serial numbers.

/** Day-first (02/11 = 2 November) or month-first (02/11 = 11 February). */
export type DateOrder = "dmy" | "mdy";

export const SLASH_DATE = /^(\d{1,2})[/.-](\d{1,2})[/.-](\d{4})/;

/** Date cell → local calendar day: "2026-11-02", "02/11/2026" (in `order`), or an Excel serial number. */
export function dayOf(v: string | number, order: DateOrder): Date | null {
    if (typeof v === "number" || /^\d+(\.\d+)?$/.test(String(v).trim())) {
        const serial = Number(v);
        const utc = new Date(Math.round((serial - 25569) * 86_400_000));
        return new Date(utc.getUTCFullYear(), utc.getUTCMonth(), utc.getUTCDate());
    }
    const iso = String(v).trim().match(/^(\d{4})-(\d{1,2})-(\d{1,2})/);
    if (iso) return valid(Number(iso[1]), Number(iso[2]), Number(iso[3]));
    const m = String(v).trim().match(SLASH_DATE);
    if (!m) return null;
    const [a, b] = [Number(m[1]), Number(m[2])];
    return order === "dmy" ? valid(Number(m[3]), b, a) : valid(Number(m[3]), a, b);
}

/** A real calendar date, or null (so 31/02 or month 13 is reported instead of rolling over). */
export function valid(y: number, month: number, d: number): Date | null {
    const date = new Date(y, month - 1, d);
    return date.getMonth() === month - 1 && date.getDate() === d ? date : null;
}

/** Decides day/month order from the file: any first part above 12 means day-first, any second part above 12 month-first. */
export function detectDateOrder(cells: (string | number)[]): { order: DateOrder | null; slashDates: boolean; sample?: string } {
    let dayFirst = false, monthFirst = false, slashDates = false;
    let sample: string | undefined;
    for (const c of cells) {
        const m = typeof c === "string" ? c.trim().match(SLASH_DATE) : null;
        if (!m) continue;
        slashDates = true;
        sample ??= m[0];
        if (Number(m[1]) > 12) dayFirst = true;
        if (Number(m[2]) > 12) monthFirst = true;
    }
    return { order: dayFirst === monthFirst ? null : dayFirst ? "dmy" : "mdy", slashDates, sample };
}

/** Time cell → [hours, minutes]: "18:30" text, or an Excel day fraction. */
export function timeOf(v: string | number): [number, number] {
    if (typeof v === "number" || /^0?\.\d+$/.test(String(v).trim())) {
        const minutes = Math.round(Number(v) * 24 * 60);
        return [Math.floor(minutes / 60) % 24, minutes % 60];
    }
    const m = String(v).match(/(\d{1,2}):(\d{2})/);
    return m ? [Number(m[1]), Number(m[2])] : [12, 0];
}
