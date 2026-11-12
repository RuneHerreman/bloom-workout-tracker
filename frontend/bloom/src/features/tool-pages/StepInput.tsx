import { Minus, Plus } from "lucide-react";

interface Props {
    value: string;
    onChange: (v: string) => void;
    step: number;
    min?: number;
    max?: number;
    unit?: string;
    label: string;
    placeholder?: string;
    decimal?: boolean;
}

const fmt = (n: number) => String(Math.round(n * 100) / 100);

/**
 * A number field with − and + either side, for values you nudge rather than type:
 * the buttons snap to the step (80 → 82.5, not 80 → 82.5 → 85 from 81).
 */
function StepInput({ value, onChange, step, min = 0, max = Infinity, unit, label, placeholder = "0", decimal }: Props) {
    const n = Number(value.replace(",", "."));
    const has = value.trim() !== "" && Number.isFinite(n);
    function nudge(dir: 1 | -1) {
        const base = has ? n : min;
        const snapped = dir > 0 ? Math.floor(base / step + 1e-9) * step + step : Math.ceil(base / step - 1e-9) * step - step;
        onChange(fmt(Math.min(max, Math.max(min, snapped))));
    }
    return (
        <span className="step-input">
            <button type="button" tabIndex={-1} aria-label={`Less ${label.toLowerCase()}`} onClick={() => nudge(-1)} disabled={has && n <= min}>
                <Minus size={14} />
            </button>
            <span className="step-input-field">
                <input inputMode={decimal ? "decimal" : "numeric"} aria-label={label} value={value} placeholder={placeholder}
                    onChange={e => onChange(e.target.value)}
                    onKeyDown={e => {
                        if (e.key === "ArrowUp") { e.preventDefault(); nudge(1); }
                        if (e.key === "ArrowDown") { e.preventDefault(); nudge(-1); }
                    }} />
                {unit && <span aria-hidden>{unit}</span>}
            </span>
            <button type="button" tabIndex={-1} aria-label={`More ${label.toLowerCase()}`} onClick={() => nudge(1)} disabled={has && n >= max}>
                <Plus size={14} />
            </button>
        </span>
    );
}

export default StepInput;
