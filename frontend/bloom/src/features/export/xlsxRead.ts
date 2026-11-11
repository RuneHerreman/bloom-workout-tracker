// Minimal .xlsx reader: enough to read back the "Sets" sheet Bloom exports, also
// after the file was re-saved by Excel (deflate-compressed, shared strings).

const decoder = new TextDecoder();

interface ZipEntry { method: number; compressedSize: number; dataStart: number }

/** Reads the central directory, so entries with data descriptors (written by Excel) work. */
function readZipEntries(buf: ArrayBuffer): Map<string, ZipEntry> {
    const v = new DataView(buf);
    let eocd = -1;
    for (let i = buf.byteLength - 22; i >= Math.max(0, buf.byteLength - 65_557); i--) {
        if (v.getUint32(i, true) === 0x06054b50) { eocd = i; break; }
    }
    if (eocd < 0) throw new Error("Not a valid Excel file.");
    const count = v.getUint16(eocd + 10, true);
    let p = v.getUint32(eocd + 16, true);
    const entries = new Map<string, ZipEntry>();
    for (let i = 0; i < count; i++) {
        if (v.getUint32(p, true) !== 0x02014b50) break;
        const method = v.getUint16(p + 10, true);
        const compressedSize = v.getUint32(p + 20, true);
        const nameLen = v.getUint16(p + 28, true);
        const extraLen = v.getUint16(p + 30, true);
        const commentLen = v.getUint16(p + 32, true);
        const localOffset = v.getUint32(p + 42, true);
        const name = decoder.decode(new Uint8Array(buf, p + 46, nameLen));
        const localNameLen = v.getUint16(localOffset + 26, true);
        const localExtraLen = v.getUint16(localOffset + 28, true);
        entries.set(name, { method, compressedSize, dataStart: localOffset + 30 + localNameLen + localExtraLen });
        p += 46 + nameLen + extraLen + commentLen;
    }
    return entries;
}

async function readEntry(buf: ArrayBuffer, entry: ZipEntry): Promise<string> {
    const raw = new Uint8Array(buf, entry.dataStart, entry.compressedSize);
    if (entry.method === 0) return decoder.decode(raw);
    if (entry.method !== 8) throw new Error("Unsupported compression in this Excel file.");
    const stream = new Blob([raw]).stream().pipeThrough(new DecompressionStream("deflate-raw"));
    return decoder.decode(await new Response(stream).arrayBuffer());
}

const xmlText = (node: Element | null) =>
    node ? [...node.getElementsByTagName("t")].map(t => t.textContent ?? "").join("") : "";

function columnIndex(ref: string): number {
    let n = 0;
    for (const c of ref.replace(/\d+/g, "")) n = n * 26 + (c.charCodeAt(0) - 64);
    return n - 1;
}

/**
 * Rows of one sheet as strings and numbers (dates come back as Excel serial
 * numbers). Falls back to the first sheet when the named one doesn't exist.
 */
export async function readXlsxSheet(buf: ArrayBuffer, sheetName: string): Promise<(string | number)[][]> {
    const entries = readZipEntries(buf);
    const read = async (path: string) => {
        const e = entries.get(path);
        return e ? new DOMParser().parseFromString(await readEntry(buf, e), "application/xml") : null;
    };

    const workbook = await read("xl/workbook.xml");
    const rels = await read("xl/_rels/workbook.xml.rels");
    if (!workbook || !rels) throw new Error("Not a valid Excel file.");
    const sheets = [...workbook.getElementsByTagName("sheet")];
    const sheet = sheets.find(s => s.getAttribute("name") === sheetName) ?? sheets[0];
    if (!sheet) throw new Error("This Excel file has no sheets.");
    const relId = sheet.getAttribute("r:id") ?? sheet.getAttributeNS("http://schemas.openxmlformats.org/officeDocument/2006/relationships", "id");
    const rel = [...rels.getElementsByTagName("Relationship")].find(r => r.getAttribute("Id") === relId);
    const target = (rel?.getAttribute("Target") ?? "worksheets/sheet1.xml").replace(/^\//, "");
    const doc = await read(target.startsWith("xl/") ? target : `xl/${target}`);
    if (!doc) throw new Error("Couldn't read the sheet.");

    const shared = (await read("xl/sharedStrings.xml")) ? [...(await read("xl/sharedStrings.xml"))!.getElementsByTagName("si")].map(xmlText) : [];

    return [...doc.getElementsByTagName("row")].map(row => {
        const out: (string | number)[] = [];
        for (const c of row.getElementsByTagName("c")) {
            const i = columnIndex(c.getAttribute("r") ?? "A1");
            const type = c.getAttribute("t");
            const v = c.getElementsByTagName("v")[0]?.textContent ?? "";
            let value: string | number = "";
            if (type === "inlineStr") value = xmlText(c.getElementsByTagName("is")[0] ?? null);
            else if (type === "s") value = shared[Number(v)] ?? "";
            else if (type === "str" || type === "b") value = v;
            else if (v !== "") value = Number(v);
            out[i] = value;
        }
        return Array.from(out, x => x ?? "");
    });
}
