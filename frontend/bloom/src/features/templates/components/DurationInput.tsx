import { parseDuration, formatDuration } from "../templateUtils.ts";

function DurationInput({ value, onChange }: { value: string | null; onChange: (v: string | null) => void }) {
    const [h, m, s] = parseDuration(value);

    function handleChange(part: "h" | "m" | "s", raw: string) {
        const n = raw === "" ? 0 : Math.max(0, Number(raw));
        const clamped = part === "h" ? Math.min(n, 99) : Math.min(n, 59);
        const next = part === "h" ? formatDuration(clamped, m, s)
                   : part === "m" ? formatDuration(h, clamped, s)
                   :                formatDuration(h, m, clamped);
        onChange(next === "00:00:00" ? null : next);
    }

    return (
        <div className="duration-input">
            <input type="number" min={0} max={99} value={h === 0 ? "" : h} placeholder="0" onChange={e => handleChange("h", e.target.value)} />
            <span className="duration-sep">:</span>
            <input type="number" min={0} max={59} value={m === 0 ? "" : m} placeholder="00" onChange={e => handleChange("m", e.target.value)} />
            <span className="duration-sep">:</span>
            <input type="number" min={0} max={59} value={s === 0 ? "" : s} placeholder="00" onChange={e => handleChange("s", e.target.value)} />
        </div>
    );
}

export default DurationInput;