import { useEffect, useMemo, useState } from "react";
import type { LoggedWorkout } from "../../../assets/js/data/apiTypes.ts";
import { getLogs, peekLogs } from "../../logbook/api.ts";
import { searchExercises, peekExercises, type Exercise } from "../../exercises/api.ts";
import {
    MAX_REPS, RELIABLE_REPS, estimateOneRepMax, repsToFailure, round1, roundDownToStep, weightForReps,
} from "../../../utils/bodyMath.ts";
import { liftHistories, RECENT_DAYS, type BestSet } from "./liftHistory.ts";
import InfoHint from "../../../components/general/InfoHint.tsx";
import "../../../assets/css/insights.css";
import "../../../assets/css/tools.css";

const TABLE_REPS = [1, 2, 3, 4, 5, 6, 8, 10, 12];
const TABLE_RIR = [0, 1, 2, 3];
const STEPS = [2.5, 1.25] as const;
type Step = (typeof STEPS)[number];
const STEP_KEY = "bloom.1rm.step";

const fmtKg = (n: number) => `${round1(n).toLocaleString("en-GB")} kg`;
const fmtStep = (n: number) => n.toLocaleString("en-GB", { maximumFractionDigits: 2 });
const fmtDate = (d: string) => new Date(d).toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" });
const fmtSet = (s: BestSet) => `${round1(s.weightKg)} kg × ${s.reps}${s.rir ? ` @ ${s.rir} RIR` : ""}`;

/** Parses a number field; empty or junk is null so the result can say what's missing. */
const num = (v: string) => (v.trim() === "" || !Number.isFinite(Number(v)) ? null : Number(v));

function readStep(): Step {
    try { return Number(localStorage.getItem(STEP_KEY)) === 1.25 ? 1.25 : 2.5; } catch { return 2.5; }
}

function OneRepMaxPage() {
    const [logs, setLogs] = useState<LoggedWorkout[]>(() => peekLogs() ?? []);
    const [exercises, setExercises] = useState<Record<string, Exercise>>(
        () => Object.fromEntries((peekExercises() ?? []).map(e => [e.id, e])),
    );
    const [exerciseId, setExerciseId] = useState("");
    const [weight, setWeight] = useState("");
    const [reps, setReps] = useState("");
    const [rir, setRir] = useState("");
    const [step, setStep] = useState<Step>(readStep);

    useEffect(() => {
        Promise.all([getLogs(), searchExercises()])
            .then(([l, e]) => { setLogs(l); setExercises(Object.fromEntries(e.map(x => [x.id, x]))); })
            .catch(() => { /* the calculator works without history */ });
    }, []);

    const histories = useMemo(
        () => liftHistories(logs).filter(h => exercises[h.exerciseId]?.type !== "Cardio"),
        [logs, exercises],
    );
    const history = histories.find(h => h.exerciseId === exerciseId) ?? null;

    function pickExercise(id: string) {
        setExerciseId(id);
        const h = histories.find(x => x.exerciseId === id);
        if (!h) return;
        setWeight(String(round1(h.recent.weightKg)));
        setReps(String(h.recent.reps));
        setRir(h.recent.rir ? String(h.recent.rir) : "");
    }

    function changeStep(s: Step) {
        setStep(s);
        try { localStorage.setItem(STEP_KEY, String(s)); } catch { /* per-viewer convenience only */ }
    }

    const w = num(weight), r = num(reps), rr = num(rir) ?? 0;
    const validInput = w !== null && r !== null && w > 0 && r >= 1 && Number.isInteger(r) && rr >= 0;
    const est = validInput ? estimateOneRepMax(w, r, rr) : null;
    const toFailure = validInput ? repsToFailure(r, rr) : null;
    const oneRm = est?.estimate ?? null;

    return (
        <div className="insights-page tool-page">
            <header className="page-header">
                <div className="page-header-titles">
                    <h1>One rep max</h1>
                    <p className="header-subtitle">Estimate your max from a set, and see what to lift for any rep count</p>
                </div>
            </header>

            <div className="tool-body">
                <section className="panel tool-panel" aria-label="One rep max calculator">
                    <div className="tool-inputs">
                        <label className="tool-field tool-field-wide">
                            <span>Exercise <em>optional</em></span>
                            <select aria-label="Exercise" value={exerciseId} onChange={e => pickExercise(e.target.value)}>
                                <option value="">Any lift</option>
                                {histories.map(h => (
                                    <option key={h.exerciseId} value={h.exerciseId}>{exercises[h.exerciseId]?.name ?? "Unknown exercise"}</option>
                                ))}
                            </select>
                        </label>
                        <label className="tool-field">
                            <span>Weight</span>
                            <span className="tool-input-unit">
                                <input type="number" aria-label="Weight in kg" inputMode="decimal" min={0} step={0.5} value={weight} placeholder="0" onChange={e => setWeight(e.target.value)} />
                                <span aria-hidden>kg</span>
                            </span>
                        </label>
                        <label className="tool-field tool-field-narrow">
                            <span>Reps</span>
                            <input type="number" aria-label="Reps" inputMode="numeric" min={1} max={30} step={1} value={reps} placeholder="0" onChange={e => setReps(e.target.value)} />
                        </label>
                        <label className="tool-field tool-field-narrow">
                            <span className="tool-label-hint">
                                RIR
                                <InfoHint title="Reps in reserve" label="What does RIR mean?">
                                    <p>How many more reps you could have done. Leave it empty if you went to failure.</p>
                                </InfoHint>
                            </span>
                            <input type="number" aria-label="Reps in reserve" inputMode="numeric" min={0} max={10} step={1} value={rir} placeholder="0" onChange={e => setRir(e.target.value)} />
                        </label>
                    </div>
                    {history && (
                        <p className="tool-history">
                            Best set in the last {RECENT_DAYS / 7} weeks: <b>{fmtSet(history.recent)}</b> on {fmtDate(history.recent.date)}
                        </p>
                    )}

                    <div className="tool-split">
                        <div className="tool-result" aria-live="polite">
                            <span className="tool-result-label">Estimated 1RM</span>
                            {est ? (
                                <>
                                    <p className="tool-result-value">
                                        {fmtKg(est.estimate)}
                                        {est.reliability !== "exact" && <span className="tool-result-range">± {round1((est.high - est.low) / 2)}</span>}
                                    </p>
                                    {est.reliability !== "exact" && (
                                        <p className="tool-muted">Epley {round1(est.epley)} · Brzycki {round1(est.brzycki)}</p>
                                    )}
                                    {rr > 0 && <p className="tool-muted">Counted as {toFailure} reps to failure ({r} + {rr} in reserve).</p>}
                                    {est.reliability === "exact" && <p className="tool-muted">A single rep to failure is your max.</p>}
                                    {est.reliability === "rough" && (
                                        <p className="tool-note">Above {RELIABLE_REPS} reps to failure the estimate gets rough. Sets of 3–8 reps give the best one.</p>
                                    )}
                                </>
                            ) : (
                                <>
                                    <p className="tool-result-value tool-result-empty">—</p>
                                    <p className="tool-muted">
                                        {!validInput
                                            ? "Enter a weight and the reps you did."
                                            : `More than ${MAX_REPS} reps to failure can't give a useful estimate. Use a heavier set.`}
                                    </p>
                                </>
                            )}
                            {history && (
                                <p className="tool-history-best">
                                    Your logged best: <b>{fmtKg(history.allTime.e1rm)}</b>
                                    <span> from {fmtSet(history.allTime)}, {fmtDate(history.allTime.date)}</span>
                                </p>
                            )}
                        </div>

                        <div className="tool-table-wrap">
                            <div className="tool-table-head">
                                <h2 className="history-title">What to lift</h2>
                                <div className="segmented segmented-sm" role="radiogroup" aria-label="Round to">
                                    {STEPS.map(s => (
                                        <button key={s} role="radio" aria-checked={step === s}
                                            className={`segmented-item${step === s ? " active" : ""}`} onClick={() => changeStep(s)}>
                                            {fmtStep(s)} kg
                                        </button>
                                    ))}
                                </div>
                            </div>
                            <table className="history-table tool-table">
                                <thead>
                                    <tr>
                                        <th>Reps</th>
                                        {TABLE_RIR.map(x => <th key={x} className="num">{x === 0 ? "To failure" : `${x} RIR`}</th>)}
                                    </tr>
                                </thead>
                                <tbody>
                                    {TABLE_REPS.map(reps => (
                                        <tr key={reps}>
                                            <td>{reps}</td>
                                            {TABLE_RIR.map(x => {
                                                const kg = oneRm ? weightForReps(oneRm, reps, x) : 0;
                                                const current = validInput && reps === r && x === rr;
                                                return (
                                                    <td key={x} className={`num${current ? " current" : ""}`}>
                                                        {kg > 0 ? fmtStep(roundDownToStep(kg, step)) : "—"}
                                                    </td>
                                                );
                                            })}
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                            <p className="tool-muted">Rounded down to the nearest {fmtStep(step)} kg.</p>
                        </div>
                    </div>
                </section>
            </div>
        </div>
    );
}

export default OneRepMaxPage;
