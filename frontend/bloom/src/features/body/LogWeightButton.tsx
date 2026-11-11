import { useEffect, useId, useRef, useState, type FormEvent } from "react";
import { Scale } from "lucide-react";
import { getBody, logBody, localToday, formatEntryDate, type BodyEntry } from "./api.ts";
import "../../assets/css/body.css";

const MIN_KG = 20, MAX_KG = 400, MIN_BF = 3, MAX_BF = 70;

/** Accepts "78.4" and "78,4"; empty is null, junk is NaN. */
function parse(v: string): number | null {
    const t = v.trim().replace(",", ".");
    if (t === "") return null;
    return /^\d+(\.\d+)?$/.test(t) ? Number(t) : NaN;
}

const fmt = (n: number) => n.toLocaleString("en-GB", { maximumFractionDigits: 2 });

interface LogWeightButtonProps {
    /** Called after a successful save, with the saved day. */
    onLogged?: (entry: BodyEntry) => void;
    /** Extra classes for the trigger, e.g. "hide-on-phone". */
    className?: string;
}

/**
 * "Log weight" with a small popover under it: weight (prefilled with the last one),
 * optional body fat, and the day. Enter saves.
 */
function LogWeightButton({ onLogged, className = "" }: LogWeightButtonProps) {
    const [open, setOpen] = useState(false);
    const [entries, setEntries] = useState<BodyEntry[] | null>(null);
    const [weight, setWeight] = useState("");
    const [bodyFat, setBodyFat] = useState("");
    const [date, setDate] = useState(localToday);
    const [error, setError] = useState<string | null>(null);
    const [state, setState] = useState<"idle" | "saving" | "saved">("idle");
    const wrapRef = useRef<HTMLDivElement>(null);
    const weightRef = useRef<HTMLInputElement>(null);
    const titleId = useId();

    const last = entries?.at(-1) ?? null;
    const sameDay = entries?.find(e => e.date === date) ?? null;
    const lastBodyFat = [...(entries ?? [])].reverse().find(e => e.bodyFatPct != null)?.bodyFatPct ?? null;

    function openPopover() {
        setError(null);
        setState("idle");
        setDate(localToday());
        setBodyFat("");
        setOpen(true);
        getBody()
            .then(list => {
                setEntries(list);
                setWeight(prev => prev || (list.at(-1) ? fmt(list.at(-1)!.weightKg) : ""));
            })
            .catch(() => setEntries([]));
    }

    useEffect(() => {
        if (!open) return;
        weightRef.current?.focus();
        weightRef.current?.select();
        function onDown(e: PointerEvent) {
            if (!wrapRef.current?.contains(e.target as Node)) setOpen(false);
        }
        function onKey(e: KeyboardEvent) { if (e.key === "Escape") setOpen(false); }
        document.addEventListener("pointerdown", onDown);
        document.addEventListener("keydown", onKey);
        return () => {
            document.removeEventListener("pointerdown", onDown);
            document.removeEventListener("keydown", onKey);
        };
    }, [open]);

    // The prefilled weight arrives with the log; select it so typing replaces it.
    useEffect(() => { if (open && entries) weightRef.current?.select(); }, [open, entries]);

    async function submit(e: FormEvent) {
        e.preventDefault();
        if (state === "saving") return;
        const kg = parse(weight), bf = parse(bodyFat);
        if (kg === null || Number.isNaN(kg) || kg < MIN_KG || kg > MAX_KG) { setError(`Enter a weight between ${MIN_KG} and ${MAX_KG} kg.`); return; }
        if (bf !== null && (Number.isNaN(bf) || bf < MIN_BF || bf > MAX_BF)) { setError(`Body fat must be between ${MIN_BF} and ${MAX_BF}%.`); return; }
        if (!date || date > localToday()) { setError("Pick today or an earlier day."); return; }

        setError(null);
        setState("saving");
        try {
            const entry = await logBody(date, { weightKg: kg, ...(bf !== null ? { bodyFatPct: bf } : {}) });
            setState("saved");
            onLogged?.(entry);
            window.setTimeout(() => setOpen(false), 900);
        } catch (err) {
            setState("idle");
            setError((err as { error?: string })?.error ?? "Couldn't save, try again.");
        }
    }

    return (
        <div className={`log-weight ${className}`} ref={wrapRef}>
            <button
                type="button"
                className="button-component modern log-weight-trigger"
                aria-haspopup="dialog"
                aria-expanded={open}
                onClick={() => (open ? setOpen(false) : openPopover())}
            >
                <Scale size={15} /> Log weight
            </button>

            {open && (
                <form className="log-weight-popover" role="dialog" aria-labelledby={titleId} onSubmit={submit} noValidate>
                    <div className="log-weight-head">
                        <p id={titleId} className="log-weight-title">Log weight</p>
                        {last && <p className="log-weight-last">Last: {fmt(last.weightKg)} kg · {formatEntryDate(last.date)}</p>}
                    </div>

                    <div className="log-weight-fields">
                        <label className="tool-field">
                            <span>Weight</span>
                            <span className="tool-input-unit">
                                <input ref={weightRef} type="text" inputMode="decimal" autoComplete="off" aria-label="Weight in kg"
                                    value={weight} onChange={e => setWeight(e.target.value)} placeholder="0" />
                                <span aria-hidden>kg</span>
                            </span>
                        </label>
                        <label className="tool-field">
                            <span>Body fat <em>optional</em></span>
                            <span className="tool-input-unit">
                                <input type="text" inputMode="decimal" autoComplete="off" aria-label="Body fat in percent"
                                    value={bodyFat} onChange={e => setBodyFat(e.target.value)}
                                    placeholder={sameDay?.bodyFatPct != null ? fmt(sameDay.bodyFatPct) : lastBodyFat != null ? fmt(lastBodyFat) : "—"} />
                                <span aria-hidden>%</span>
                            </span>
                        </label>
                    </div>

                    <label className="tool-field">
                        <span>Day</span>
                        <input type="date" value={date} max={localToday()} onChange={e => setDate(e.target.value)} aria-label="Day" />
                    </label>

                    {sameDay && (
                        <p className="log-weight-note">
                            Replaces {fmt(sameDay.weightKg)} kg on this day{sameDay.bodyFatPct != null ? `; body fat stays ${fmt(sameDay.bodyFatPct)}% unless you enter one` : ""}.
                        </p>
                    )}
                    {error && <p className="log-weight-error" role="alert">{error}</p>}

                    <div className="log-weight-actions">
                        <button type="button" className="log-weight-cancel" onClick={() => setOpen(false)}>Cancel</button>
                        <button type="submit" className="button-component green" disabled={state !== "idle"}>
                            {state === "saved" ? "Saved" : state === "saving" ? "Saving…" : "Save"}
                        </button>
                    </div>
                </form>
            )}
        </div>
    );
}

export default LogWeightButton;
