// Minimal, dependency-free XLSX writer.
//
// An .xlsx file is a ZIP of a handful of XML parts. We only need plain sheets
// with text, numbers and dates plus a bold header row, so the archive is
// written with the STORE method (no compression) and inline strings — small
// enough to keep in-house instead of shipping a ~1 MB spreadsheet library.

export type CellValue = string | number | Date | null | undefined;

export interface Sheet {
    name: string;
    /** First row is rendered as a bold, frozen header. */
    rows: CellValue[][];
    /** Optional column widths in characters. */
    widths?: number[];
}

const MAIN_NS = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
const REL_NS = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
const PKG_REL_NS = "http://schemas.openxmlformats.org/package/2006/relationships";

// Style indices into cellXfs below.
const STYLE_HEADER = 1;
const STYLE_DATE = 2;

function escapeXml(s: string): string {
    return s
        // Strip characters that are illegal in XML 1.0.
        // eslint-disable-next-line no-control-regex
        .replace(/[\u0000-\u0008\u000B\u000C\u000E-\u001F]/g, "")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;");
}

function columnName(index: number): string {
    let n = index + 1;
    let name = "";
    while (n > 0) {
        const rem = (n - 1) % 26;
        name = String.fromCharCode(65 + rem) + name;
        n = Math.floor((n - 1) / 26);
    }
    return name;
}

/** Excel stores dates as days since 1899-12-30 (in local time, no zone). */
function toExcelSerial(d: Date): number {
    const localMs = d.getTime() - d.getTimezoneOffset() * 60_000;
    return localMs / 86_400_000 + 25569;
}

function cellXml(value: CellValue, ref: string, header: boolean): string {
    if (value === null || value === undefined || value === "") return "";
    if (header) {
        return `<c r="${ref}" t="inlineStr" s="${STYLE_HEADER}"><is><t>${escapeXml(String(value))}</t></is></c>`;
    }
    if (value instanceof Date) {
        return `<c r="${ref}" s="${STYLE_DATE}"><v>${toExcelSerial(value)}</v></c>`;
    }
    if (typeof value === "number") {
        return Number.isFinite(value) ? `<c r="${ref}"><v>${value}</v></c>` : "";
    }
    return `<c r="${ref}" t="inlineStr"><is><t xml:space="preserve">${escapeXml(value)}</t></is></c>`;
}

function sheetXml(sheet: Sheet): string {
    const cols = sheet.widths?.length
        ? `<cols>${sheet.widths.map((w, i) => `<col min="${i + 1}" max="${i + 1}" width="${w}" customWidth="1"/>`).join("")}</cols>`
        : "";
    const rows = sheet.rows.map((row, r) =>
        `<row r="${r + 1}">${row.map((v, c) => cellXml(v, `${columnName(c)}${r + 1}`, r === 0)).join("")}</row>`
    ).join("");
    return `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<worksheet xmlns="${MAIN_NS}"><sheetViews><sheetView workbookViewId="0"><pane ySplit="1" topLeftCell="A2" activePane="bottomLeft" state="frozen"/></sheetView></sheetViews>${cols}<sheetData>${rows}</sheetData></worksheet>`;
}

function safeSheetName(name: string, used: Set<string>): string {
    let base = name.replace(/[\\/?*[\]:]/g, " ").trim().slice(0, 31) || "Sheet";
    let candidate = base;
    let i = 2;
    while (used.has(candidate.toLowerCase())) {
        const suffix = ` ${i++}`;
        base = base.slice(0, 31 - suffix.length);
        candidate = base + suffix;
    }
    used.add(candidate.toLowerCase());
    return candidate;
}

function workbookParts(sheets: Sheet[]): Record<string, string> {
    const used = new Set<string>();
    const names = sheets.map(s => safeSheetName(s.name, used));
    const parts: Record<string, string> = {};

    parts["[Content_Types].xml"] = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/><Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>${
        sheets.map((_, i) => `<Override PartName="/xl/worksheets/sheet${i + 1}.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>`).join("")
    }</Types>`;

    parts["_rels/.rels"] = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="${PKG_REL_NS}"><Relationship Id="rId1" Type="${REL_NS}/officeDocument" Target="xl/workbook.xml"/></Relationships>`;

    parts["xl/workbook.xml"] = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<workbook xmlns="${MAIN_NS}" xmlns:r="${REL_NS}"><sheets>${
        names.map((n, i) => `<sheet name="${escapeXml(n)}" sheetId="${i + 1}" r:id="rId${i + 1}"/>`).join("")
    }</sheets></workbook>`;

    parts["xl/_rels/workbook.xml.rels"] = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="${PKG_REL_NS}">${
        sheets.map((_, i) => `<Relationship Id="rId${i + 1}" Type="${REL_NS}/worksheet" Target="worksheets/sheet${i + 1}.xml"/>`).join("")
    }<Relationship Id="rId${sheets.length + 1}" Type="${REL_NS}/styles" Target="styles.xml"/></Relationships>`;

    parts["xl/styles.xml"] = `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<styleSheet xmlns="${MAIN_NS}"><fonts count="2"><font><sz val="11"/><name val="Calibri"/></font><font><b/><sz val="11"/><name val="Calibri"/></font></fonts><fills count="2"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="gray125"/></fill></fills><borders count="1"><border><left/><right/><top/><bottom/><diagonal/></border></borders><cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs><cellXfs count="3"><xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/><xf numFmtId="0" fontId="1" fillId="0" borderId="0" xfId="0" applyFont="1"/><xf numFmtId="14" fontId="0" fillId="0" borderId="0" xfId="0" applyNumberFormat="1"/></cellXfs><cellStyles count="1"><cellStyle name="Normal" xfId="0" builtinId="0"/></cellStyles></styleSheet>`;

    sheets.forEach((s, i) => { parts[`xl/worksheets/sheet${i + 1}.xml`] = sheetXml(s); });
    return parts;
}

// ── ZIP (STORE) ───────────────────────────────────────────────────────────────

const CRC_TABLE = (() => {
    const table = new Uint32Array(256);
    for (let n = 0; n < 256; n++) {
        let c = n;
        for (let k = 0; k < 8; k++) c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1;
        table[n] = c >>> 0;
    }
    return table;
})();

export function crc32(data: Uint8Array): number {
    let crc = 0xffffffff;
    for (let i = 0; i < data.length; i++) crc = CRC_TABLE[(crc ^ data[i]) & 0xff] ^ (crc >>> 8);
    return (crc ^ 0xffffffff) >>> 0;
}

function dosDateTime(d: Date): { time: number; date: number } {
    return {
        time: (d.getHours() << 11) | (d.getMinutes() << 5) | Math.floor(d.getSeconds() / 2),
        date: ((d.getFullYear() - 1980) << 9) | ((d.getMonth() + 1) << 5) | d.getDate(),
    };
}

export function zipStore(files: Record<string, string | Uint8Array>): Uint8Array {
    const enc = new TextEncoder();
    const { time, date } = dosDateTime(new Date());
    const locals: Uint8Array[] = [];
    const centrals: Uint8Array[] = [];
    let offset = 0;

    for (const [name, content] of Object.entries(files)) {
        const nameBytes = enc.encode(name);
        const data = typeof content === "string" ? enc.encode(content) : content;
        const crc = crc32(data);

        const local = new Uint8Array(30 + nameBytes.length + data.length);
        const lv = new DataView(local.buffer);
        lv.setUint32(0, 0x04034b50, true);
        lv.setUint16(4, 20, true);
        lv.setUint16(6, 0x0800, true); // UTF-8 file names
        lv.setUint16(8, 0, true);      // STORE
        lv.setUint16(10, time, true);
        lv.setUint16(12, date, true);
        lv.setUint32(14, crc, true);
        lv.setUint32(18, data.length, true);
        lv.setUint32(22, data.length, true);
        lv.setUint16(26, nameBytes.length, true);
        lv.setUint16(28, 0, true);
        local.set(nameBytes, 30);
        local.set(data, 30 + nameBytes.length);
        locals.push(local);

        const central = new Uint8Array(46 + nameBytes.length);
        const cv = new DataView(central.buffer);
        cv.setUint32(0, 0x02014b50, true);
        cv.setUint16(4, 20, true);
        cv.setUint16(6, 20, true);
        cv.setUint16(8, 0x0800, true);
        cv.setUint16(10, 0, true);
        cv.setUint16(12, time, true);
        cv.setUint16(14, date, true);
        cv.setUint32(16, crc, true);
        cv.setUint32(20, data.length, true);
        cv.setUint32(24, data.length, true);
        cv.setUint16(28, nameBytes.length, true);
        cv.setUint32(42, offset, true);
        central.set(nameBytes, 46);
        centrals.push(central);

        offset += local.length;
    }

    const centralSize = centrals.reduce((n, c) => n + c.length, 0);
    const end = new Uint8Array(22);
    const ev = new DataView(end.buffer);
    ev.setUint32(0, 0x06054b50, true);
    ev.setUint16(8, centrals.length, true);
    ev.setUint16(10, centrals.length, true);
    ev.setUint32(12, centralSize, true);
    ev.setUint32(16, offset, true);

    const out = new Uint8Array(offset + centralSize + end.length);
    let pos = 0;
    for (const part of [...locals, ...centrals, end]) { out.set(part, pos); pos += part.length; }
    return out;
}

export function buildXlsx(sheets: Sheet[]): Blob {
    const bytes = zipStore(workbookParts(sheets));
    return new Blob([bytes.buffer as ArrayBuffer], {
        type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    });
}
