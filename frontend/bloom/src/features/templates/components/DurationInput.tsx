import { parseDuration, formatDuration } from "../templateUtils.ts";

interface DurationInputProps {
    value: string | null;
    onChange: (v: string | null) => void;
    /** Greyed duration shown while the field is empty (e.g. last time's). */
    placeholder?: string | null;
    /** Accessible name for the whole field; each part adds hours/minutes/seconds. */
    label?: string;
}

function DurationInput({ value, onChange, placeholder, label = "Duration" }: DurationInputProps) {
    const [h, m, s] = parseDuration(value);
    // Placeholders only stand in for a duration nobody has typed yet.
    const [ph, pm, ps] = value == null && placeholder ? parseDuration(placeholder) : [0, 0, 0];
    const two = (n: number) => String(n).padStart(2, "0");

    function handleChange(part: "h" | "m" | "s", raw: string) {
        const n = raw === "" ? 0 : Math.max(0, Number(raw));
        const clamped = part === "h" ? Math.min(n, 99) : Math.min(n, 59);
        const updated = { h, m, s, [part]: clamped };
        const next = formatDuration(updated.h, updated.m, updated.s);
        onChange(next === "00:00:00" ? null : next);
    }

    return (
        <div className="duration-input" role="group" aria-label={label}>
            <input type="number" inputMode="numeric" enterKeyHint="next" min={0} max={99} aria-label="Hours" value={h === 0 ? "" : h} placeholder={String(ph)} onFocus={e => e.target.select()} onChange={e => handleChange("h", e.target.value)} />
            <span className="duration-sep">:</span>
            <input type="number" inputMode="numeric" enterKeyHint="next" min={0} max={59} aria-label="Minutes" value={m === 0 ? "" : m} placeholder={two(pm)} onFocus={e => e.target.select()} onChange={e => handleChange("m", e.target.value)} />
            <span className="duration-sep">:</span>
            <input type="number" inputMode="numeric" enterKeyHint="next" min={0} max={59} aria-label="Seconds" value={s === 0 ? "" : s} placeholder={two(ps)} onFocus={e => e.target.select()} onChange={e => handleChange("s", e.target.value)} />
        </div>
    );
}

export default DurationInput;
