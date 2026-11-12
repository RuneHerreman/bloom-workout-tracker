import { useEffect, useMemo, useState } from "react";
import type { LoggedWorkout } from "../../../assets/js/data/apiTypes.ts";
import { getLogs, peekLogs } from "../../logbook/api.ts";
import { searchExercises, peekExercises, type Exercise } from "../../exercises/api.ts";
import {
    MAX_REPS, RELIABLE_REPS, estimateOneRepMax, repsToFailure, round1, roundDownToStep, weightForReps,
} from "../../../utils/bodyMath.ts";
import { liftHistories, RECENT_DAYS, type BestSet } from "./liftHistory.ts";
import LiftPicker, { type LiftOption } from "./LiftPicker.tsx";
import StepInput from "../StepInput.tsx";
import TrendChart, { type TrendPoint } from "../../insights/components/TrendChart.tsx";
import { VizTooltip } from "../../insights/components/ChartParts.tsx";
import InfoHint from "../../../components/general/InfoHint.tsx";
import "../../../assets/css/insights.css";
import "../../../assets/css/tools.css";

const TABLE_REPS = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12];
const TABLE_RIR = [0, 1, 2, 3];
const RIR_CHOICES = [0, 1, 2, 3, 4];
const STEPS = [2.5, 1.25] as const;
type Step = (typeof STEPS)[number];
const STEP_KEY = "bloom.1rm.step";

const fmtKg = (n: number) => `${round1(n).toLocaleString("en-GB")} kg`;
const fmtStep = (n: number) => n.toLocaleString("en-GB", { maximumFractionDigits: 2 });
const fmtDate = (d: string) => new Date(d).toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" });
const fmtSet = (s: BestSet) => `${round1(s.weightKg)} kg × ${s.reps}${s.rir ? ` @ ${s.rir} RIR` : ""}`;

/** Parses a number field; empty or junk is null so the result can say what's missing. */
const num = (v: string) => (v.trim() === "" || !Number.isFinite(Number(v.replace(",", "."))) ? null : Number(v.replace(",", ".")));

function readStep(): Step {
    try { return Number(localStorage.getItem(STEP_KEY)) === 1.25 ? 1.25 : 2.5; } catch { return 2.5; }
}

interface SessionPoint extends TrendPoint { set: BestSet }

function OneRepMaxPage() {
    const [logs, setLogs] = useState<LoggedWorkout[]>(() => peekLogs() ?? []);
    const [exercises, setExercises] = useState<Record<string, Exercise>>(
        () => Object.fromEntries((peekExercises() ?? []).map(e => [e.id, e])),
    );
    const [exerciseId, setExerciseId] = useState("");
    const [weight, setWeight] = useState("");
    const [reps, setReps] = useState("");
    const [rir, setRir] = useState(0);
    const [step, setStep] = useState<Step>(readStep);
    const [hover, setHover] = useState<{ row: number; col: number } | null>(null);

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
    const options: LiftOption[] = useMemo(() => histories.map(h => ({
        id: h.exerciseId,
        name: exercises[h.exerciseId]?.name ?? "Unknown exercise",
        e1rm: h.sessions.at(-1)?.e1rm ?? h.recent.e1rm,
        lastDate: h.lastDate,
    })), [histories, exercises]);

    function pickExercise(id: string) {
        setExerciseId(id);
        const h = histories.find(x => x.exerciseId === id);
        if (!h) return;
        setWeight(String(round1(h.recent.weightKg)));
        setReps(String(h.recent.reps));
        setRir(Math.min(4, h.recent.rir ?? 0));
    }

    function changeStep(s: Step) {
        setStep(s);
        try { localStorage.setItem(STEP_KEY, String(s)); } catch { /* per-viewer convenience only */ }
    }

    const w = num(weight), r = num(reps);
    const validInput = w !== null && r !== null && w > 0 && r >= 1 && Number.isInteger(r);
    const est = validInput ? estimateOneRepMax(w, r, rir) : null;
    const toFailure = validInput ? repsToFailure(r, rir) : null;
    const oneRm = est?.estimate ?? null;
    const halfRange = est ? (est.high - est.low) / 2 : 0;
    const vsBest = est && history ? est.estimate - history.allTime.e1rm : null;

    // Every session's best estimate; a dot marks each new record on the way.
    const trend = useMemo(() => {
        if (!history || history.sessions.length < 2) return null;
        let best = 0;
        const points: SessionPoint[] = history.sessions.map(s => {
            const highlight = s.e1rm > best + 0.05;
            if (highlight) best = s.e1rm;
            return { t: new Date(s.date).getTime(), value: round1(s.e1rm), highlight, set: s };
        });
        return { points, start: new Date(points[0].t), end: new Date() };
    }, [history]);

    const liftName = history ? exercises[history.exerciseId]?.name ?? "this lift" : null;

    return (
        <div className="insights-page tool-page">
            <header className="page-header">
                <div className="page-header-titles">
                    <h1>One rep max</h1>
                    <p className="header-subtitle">Estimate your max from a set, and see what to lift for any rep count</p>
                </div>
            </header>

            <div className="tool-body">
                <div className="tool-peek" aria-hidden>
                    <span>Estimated 1RM{liftName ? ` · ${liftName}` : ""}</span>
                    <b>{est ? fmtKg(est.estimate) : "—"}</b>
                </div>
                <aside className="panel tool-rail" aria-label="Your set">
                    <div className="tool-field">
                        <span>Exercise <em>optional</em></span>
                        <LiftPicker options={options} value={exerciseId} onChange={pickExercise} />
                        {history && (
                            <p className="tool-hint">
                                Filled in from your best set in the last {RECENT_DAYS / 7} weeks: <b>{fmtSet(history.recent)}</b>, {fmtDate(history.recent.date)}.
                            </p>
                        )}
                    </div>
                    <div className="tool-field-row">
                        <label className="tool-field">
                            <span>Weight</span>
                            <StepInput label="Weight in kg" unit="kg" decimal step={step} value={weight} onChange={setWeight} />
                        </label>
                        <label className="tool-field">
                            <span>Reps</span>
                            <StepInput label="Reps" step={1} min={1} max={30} value={reps} onChange={setReps} />
                        </label>
                    </div>
                    <div className="tool-field">
                        <span className="tool-label-hint">
                            Reps in reserve
                            <InfoHint title="Reps in reserve" label="What does RIR mean?">
                                <p>How many more reps you could have done. 0 means you went to failure.</p>
                            </InfoHint>
                        </span>
                        <div className="segmented tool-segmented" role="radiogroup" aria-label="Reps in reserve">
                            {RIR_CHOICES.map(x => (
                                <button key={x} type="button" role="radio" aria-checked={rir === x}
                                    className={`segmented-item${rir === x ? " active" : ""}`} onClick={() => setRir(x)}>
                                    {x === 0 ? "Failure" : x === 4 ? "4+" : x}
                                </button>
                            ))}
                        </div>
                    </div>
                </aside>

                <div className="tool-main">
                    <section className="panel tool-hero" aria-live="polite" aria-label="Estimated one rep max">
                        <div className="tool-hero-main">
                            <span className="tool-result-label">Estimated 1RM{liftName ? ` · ${liftName}` : ""}</span>
                            {est ? (
                                <p className="tool-result-value">
                                    {fmtKg(est.estimate)}
                                    {est.reliability !== "exact" && halfRange >= 0.5 && <span className="tool-result-range">± {round1(halfRange)}</span>}
                                </p>
                            ) : (
                                <p className="tool-result-value tool-result-empty">—</p>
                            )}
                            {!est && (
                                <p className="tool-muted">
                                    {!validInput
                                        ? "Enter a weight and the reps you did."
                                        : `More than ${MAX_REPS} reps to failure can't give a useful estimate. Use a heavier set.`}
                                </p>
                            )}
                            {est && est.reliability !== "exact" && (
                                <p className="tool-muted">
                                    Epley {round1(est.epley)} · Brzycki {round1(est.brzycki)}
                                    {rir > 0 && <> · counted as {toFailure} reps to failure</>}
                                </p>
                            )}
                            {est?.reliability === "exact" && <p className="tool-muted">A single rep to failure is your max.</p>}
                            {est?.reliability === "rough" && (
                                <p className="tool-note">Above {RELIABLE_REPS} reps to failure the estimate gets rough. Sets of 3–8 reps give the best one.</p>
                            )}
                        </div>
                        {history && (
                            <dl className="tool-hero-stats">
                                <div>
                                    <dt>Logged best</dt>
                                    <dd>{fmtKg(history.allTime.e1rm)}</dd>
                                    <span>{fmtSet(history.allTime)}, {fmtDate(history.allTime.date)}</span>
                                </div>
                                {vsBest !== null && (
                                    <div>
                                        <dt>This set</dt>
                                        <dd className={vsBest > 0.05 ? "up" : undefined}>
                                            {vsBest > 0.05 ? `+${round1(vsBest)} kg` : vsBest < -0.05 ? `−${round1(-vsBest)} kg` : "Equal"}
                                        </dd>
                                        <span>{vsBest > 0.05 ? "A new best, if you hit it" : "compared with your logged best"}</span>
                                    </div>
                                )}
                            </dl>
                        )}
                    </section>

                    {trend && (
                        <section className="panel" aria-label={`${liftName} estimated 1RM over time`}>
                            <div className="panel-head">
                                <h2 className="panel-title">Estimated 1RM over time</h2>
                                <p className="panel-desc">Your best set each session. Larger dots are new records.</p>
                            </div>
                            <TrendChart
                                points={trend.points}
                                start={trend.start}
                                end={trend.end}
                                height={200}
                                formatValue={v => `${v} kg`}
                                ariaLabel={`${liftName}: estimated 1RM per session`}
                                seriesLabel="Best set each session"
                                highlightLabel="New record"
                                recordLine
                                renderTooltip={p => (
                                    <VizTooltip
                                        title={fmtDate(p.set.date)}
                                        rows={[
                                            { label: "Est. 1RM", value: fmtKg(p.set.e1rm), color: "var(--viz-1)" },
                                            { label: "From set", value: fmtSet(p.set) },
                                            ...(p.highlight ? [{ label: "New record", value: "★" }] : []),
                                        ]}
                                    />
                                )}
                            />
                        </section>
                    )}

                    <section className="panel tool-table-panel" aria-label="What to lift">
                        <div className="tool-table-head">
                            <div className="panel-head">
                                <h2 className="panel-title">What to lift</h2>
                                <p className="panel-desc">For a set of so many reps, leaving so many in reserve. Rounded down to {fmtStep(step)} kg.</p>
                            </div>
                            <div className="segmented segmented-sm" role="radiogroup" aria-label="Round to">
                                {STEPS.map(s => (
                                    <button key={s} role="radio" aria-checked={step === s}
                                        className={`segmented-item${step === s ? " active" : ""}`} onClick={() => changeStep(s)}>
                                        {fmtStep(s)} kg
                                    </button>
                                ))}
                            </div>
                        </div>
                        <div className="tool-table-scroll">
                            <table className="history-table tool-table" onPointerLeave={() => setHover(null)}>
                                <thead>
                                    <tr>
                                        <th>Reps</th>
                                        <th className="num pct">% 1RM</th>
                                        {TABLE_RIR.map((x, c) => (
                                            <th key={x} className={`num${hover?.col === c || (validInput && rir === x) ? " cross" : ""}`}>
                                                {x === 0 ? "To failure" : `${x} RIR`}
                                            </th>
                                        ))}
                                    </tr>
                                </thead>
                                <tbody>
                                    {TABLE_REPS.map((reps, row) => {
                                        const rowOn = hover?.row === row;
                                        return (
                                            <tr key={reps} className={rowOn ? "cross" : undefined}>
                                                <td>{reps}</td>
                                                <td className="num pct">{Math.round(weightForReps(100, reps, 0))}%</td>
                                                {TABLE_RIR.map((x, c) => {
                                                    const kg = oneRm ? weightForReps(oneRm, reps, x) : 0;
                                                    const current = validInput && reps === r && x === rir;
                                                    const cls = ["num", current && "current", hover?.col === c && "cross", rowOn && hover?.col === c && "hit"].filter(Boolean).join(" ");
                                                    return (
                                                        <td key={x} className={cls} onPointerEnter={() => setHover({ row, col: c })}>
                                                            {kg > 0 ? fmtStep(roundDownToStep(kg, step)) : "—"}
                                                        </td>
                                                    );
                                                })}
                                            </tr>
                                        );
                                    })}
                                </tbody>
                            </table>
                        </div>
                    </section>
                </div>
            </div>
        </div>
    );
}

export default OneRepMaxPage;
