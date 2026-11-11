import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import type { BodyEntry, LoggedWorkout, Sex, User } from "../../../assets/js/data/apiTypes.ts";
import { getMe, peekMe, updateSex } from "../../auth/api.ts";
import { getBody, peekBody, localToday } from "../../body/api.ts";
import { getLogs, peekLogs } from "../../logbook/api.ts";
import { buildSeries, dayIndex } from "../../insights/bodySeries.ts";
import {
    ACTIVITY_LEVELS, KCAL_PER_KG, activityFromSessions, ageInYears, weeklyRate, type ActivityKey, type Goal,
} from "../../../utils/bodyMath.ts";
import { DEFAULT_RATE, PROTEIN_OPTIONS, RATES, macroPlan } from "./macroPlan.ts";
import InfoHint from "../../../components/general/InfoHint.tsx";
import "../../../assets/css/insights.css";
import "../../../assets/css/tools.css";

const DAY = 86_400_000;
/** A body fat reading older than this isn't used for resting energy. */
const BODY_FAT_MAX_AGE_DAYS = 60;
const STORE = "bloom.macros";

const GOALS: { value: Goal; label: string }[] = [
    { value: "lose", label: "Lose" },
    { value: "maintain", label: "Maintain" },
    { value: "gain", label: "Gain" },
];

const num = (n: number, d = 1) => n.toLocaleString("en-GB", { maximumFractionDigits: d });
const kcal = (n: number) => Math.round(n).toLocaleString("en-GB");
const signed = (n: number, d = 1) => `${n > 0 ? "+" : n < 0 ? "−" : ""}${num(Math.abs(n), d)}`;
const parse = (v: string) => { const n = Number(v.trim().replace(",", ".")); return v.trim() && Number.isFinite(n) ? n : null; };

interface Saved { goal: Goal; rate: number; protein: number | null }
function readSaved(): Saved {
    try {
        const s = JSON.parse(localStorage.getItem(STORE) ?? "{}") as Partial<Saved>;
        const goal = GOALS.some(g => g.value === s.goal) ? s.goal! : "maintain";
        const rate = typeof s.rate === "number" && RATES[goal].includes(s.rate) ? s.rate : DEFAULT_RATE[goal];
        const protein = typeof s.protein === "number" && PROTEIN_OPTIONS.includes(s.protein) ? s.protein : null;
        return { goal, rate, protein };
    } catch {
        return { goal: "maintain", rate: 0, protein: null };
    }
}

/** Whole percentages that add up to 100 (largest remainder), so the bars never read 101%. */
function wholePercents(values: number[]): number[] {
    const total = values.reduce((a, b) => a + b, 0);
    if (!total) return values.map(() => 0);
    const exact = values.map(v => (v / total) * 100);
    const out = exact.map(Math.floor);
    const order = exact.map((v, i) => [v - Math.floor(v), i] as const).sort((a, b) => b[0] - a[0]);
    const short = 100 - out.reduce((a, b) => a + b, 0);
    for (let k = 0; k < short; k++) out[order[k][1]]++;
    return out;
}

/** Sessions a week over the last four weeks of logged workouts. */
function recentSessionsPerWeek(logs: LoggedWorkout[], now = Date.now()): number | null {
    if (!logs.length) return null;
    const since = now - 28 * DAY;
    return logs.filter(l => new Date(l.loggedAt).getTime() >= since).length / 4;
}

function MacroPage() {
    const [user, setUser] = useState<User | null>(() => peekMe() ?? null);
    const [body, setBody] = useState<BodyEntry[]>(() => peekBody() ?? []);
    const [logs, setLogs] = useState<LoggedWorkout[]>(() => peekLogs() ?? []);

    const [weight, setWeight] = useState("");
    const [height, setHeight] = useState("");
    const [bodyFat, setBodyFat] = useState("");
    const [activity, setActivity] = useState<ActivityKey | null>(null);
    const [saved, setSaved] = useState<Saved>(readSaved);
    const [sexSaving, setSexSaving] = useState(false);

    useEffect(() => {
        getMe().then(setUser).catch(() => {});
        getBody().then(setBody).catch(() => {});
        getLogs().then(setLogs).catch(() => {});
    }, []);

    // ── What we know about you ──
    const weightSeries = useMemo(() => buildSeries(body, "weight"), [body]);
    const trendWeight = weightSeries.at(-1)?.trend ?? user?.weight ?? null;
    const recentBodyFat = useMemo(() => {
        const series = buildSeries(body, "bodyFat");
        const last = series.at(-1);
        if (!last) return null;
        const ageDays = dayIndex(localToday()) - last.day;
        return ageDays <= BODY_FAT_MAX_AGE_DAYS ? { value: last.trend, date: last.date } : null;
    }, [body]);
    const trendRate = useMemo(() => {
        const since = dayIndex(localToday()) - 28;
        return weeklyRate(weightSeries.filter(p => p.day >= since).map(p => ({ day: p.day, kg: p.raw })));
    }, [weightSeries]);
    const age = useMemo(() => {
        if (!user?.birthDate) return null;
        const [y, m, d] = user.birthDate.split("-").map(Number);
        const t = new Date();
        return ageInYears([y, m, d], [t.getFullYear(), t.getMonth() + 1, t.getDate()]);
    }, [user?.birthDate]);
    const sessions = useMemo(() => recentSessionsPerWeek(logs), [logs]);
    const suggestedActivity: ActivityKey = activityFromSessions(sessions ?? user?.activeDays ?? 0);

    // Prefill once the numbers arrive; what you type afterwards wins.
    useEffect(() => { if (!weight && trendWeight) setWeight(num(trendWeight)); }, [trendWeight]); // eslint-disable-line react-hooks/exhaustive-deps
    useEffect(() => { if (!height && user?.height) setHeight(String(user.height)); }, [user?.height]); // eslint-disable-line react-hooks/exhaustive-deps
    useEffect(() => { if (!bodyFat && recentBodyFat) setBodyFat(num(recentBodyFat.value)); }, [recentBodyFat]); // eslint-disable-line react-hooks/exhaustive-deps

    function save(next: Partial<Saved>) {
        const merged = { ...saved, ...next };
        setSaved(merged);
        try { localStorage.setItem(STORE, JSON.stringify(merged)); } catch { /* a convenience only */ }
    }
    function pickGoal(goal: Goal) { save({ goal, rate: DEFAULT_RATE[goal] }); }

    async function pickSex(sex: Sex) {
        setSexSaving(true);
        try {
            await updateSex(sex);
            setUser(u => (u ? { ...u, sex } : u));
        } finally {
            setSexSaving(false);
        }
    }

    const sex = user?.sex ?? null;
    const proteinPerKg = saved.protein ?? (saved.goal === "lose" ? 2.2 : 1.6);
    const activeKey = activity ?? suggestedActivity;
    const w = parse(weight), h = parse(height), bf = parse(bodyFat);
    const plan = macroPlan({
        weightKg: w ?? 0,
        heightCm: h ?? 0,
        age: age ?? 0,
        sex: sex === "Male" ? "male" : sex === "Female" ? "female" : null,
        bodyFatPct: bf !== null && bf >= 3 && bf <= 70 ? bf : null,
        activity: activeKey,
        goal: saved.goal,
        ratePct: saved.rate,
        proteinPerKg,
    });

    const missing = !w ? "your weight" : !h ? "your height" : !age ? "your date of birth" : !plan ? "your sex, or a body fat reading" : null;
    const macroRows = plan ? [
        { key: "protein", label: "Protein", grams: plan.macros.proteinG, kcalPerG: 4 },
        { key: "fat", label: "Fat", grams: plan.macros.fatG, kcalPerG: 9 },
        { key: "carbs", label: "Carbs", grams: plan.macros.carbsG, kcalPerG: 4 },
    ] : [];
    const shares = wholePercents(macroRows.map(r => r.grams * r.kcalPerG));

    return (
        <div className="insights-page tool-page">
            <header className="page-header">
                <div className="page-header-titles">
                    <h1>Macros</h1>
                    <p className="header-subtitle">A daily calorie and macro target, worked out from your own numbers</p>
                </div>
            </header>

            <div className="tool-body">
                <section className="panel tool-panel" aria-label="Macro calculator">
                    <div className="tool-inputs macro-inputs">
                        <label className="tool-field">
                            <span>Weight</span>
                            <span className="tool-input-unit">
                                <input inputMode="decimal" aria-label="Weight in kg" value={weight} placeholder="0" onChange={e => setWeight(e.target.value)} />
                                <span aria-hidden>kg</span>
                            </span>
                        </label>
                        <label className="tool-field">
                            <span>Height</span>
                            <span className="tool-input-unit">
                                <input inputMode="numeric" aria-label="Height in cm" value={height} placeholder="0" onChange={e => setHeight(e.target.value)} />
                                <span aria-hidden>cm</span>
                            </span>
                        </label>
                        <label className="tool-field">
                            <span>Body fat <em>optional</em></span>
                            <span className="tool-input-unit">
                                <input inputMode="decimal" aria-label="Body fat in percent" value={bodyFat} placeholder="—" onChange={e => setBodyFat(e.target.value)} />
                                <span aria-hidden>%</span>
                            </span>
                        </label>
                        <div className="tool-field">
                            <span>Sex</span>
                            <div className="segmented macro-sex" role="radiogroup" aria-label="Sex">
                                {(["Male", "Female"] as Sex[]).map(s => (
                                    <button key={s} type="button" role="radio" aria-checked={sex === s} disabled={sexSaving}
                                        className={`segmented-item${sex === s ? " active" : ""}`} onClick={() => sex !== s && pickSex(s)}>
                                        {s}
                                    </button>
                                ))}
                            </div>
                        </div>
                    </div>
                    <p className="tool-history">
                        {trendWeight ? <>Weight is your trend from the body log{weightSeries.length === 0 ? " (your profile weight)" : ""}. </> : null}
                        {recentBodyFat && <>Body fat is from your log ({num(recentBodyFat.value)}% trend); clear it to ignore it. </>}
                        {age !== null && <>Age {age} from your <Link to="/profile">profile</Link>.</>}
                    </p>

                    <div className="tool-inputs macro-plan-inputs">
                        <label className="tool-field">
                            <span>Activity</span>
                            <select aria-label="Activity level" value={activeKey} onChange={e => setActivity(e.target.value as ActivityKey)}>
                                {ACTIVITY_LEVELS.map(a => <option key={a.key} value={a.key}>{a.label} · × {a.factor}</option>)}
                            </select>
                        </label>
                        <div className="tool-field macro-goal">
                            <span>Goal</span>
                            <div className="segmented" role="radiogroup" aria-label="Goal">
                                {GOALS.map(g => (
                                    <button key={g.value} type="button" role="radio" aria-checked={saved.goal === g.value}
                                        className={`segmented-item${saved.goal === g.value ? " active" : ""}`} onClick={() => pickGoal(g.value)}>
                                        {g.label}
                                    </button>
                                ))}
                            </div>
                        </div>
                        <label className="tool-field">
                            <span>Rate</span>
                            <select aria-label="Rate per week" value={saved.rate} disabled={saved.goal === "maintain"}
                                onChange={e => save({ rate: Number(e.target.value) })}>
                                {saved.goal === "maintain"
                                    ? <option value={0}>—</option>
                                    : RATES[saved.goal].map(r => <option key={r} value={r}>{num(r, 2)}% of body weight / week</option>)}
                            </select>
                        </label>
                        <label className="tool-field">
                            <span className="tool-label-hint">
                                Protein
                                <InfoHint title="Protein" label="How much protein?">
                                    <p>1.4–2.0 g per kg covers muscle building for most people who lift (ISSN). In a deficit, around 2.2 g per kg helps keep muscle while you lose fat.</p>
                                </InfoHint>
                            </span>
                            <select aria-label="Protein per kg" value={proteinPerKg} onChange={e => save({ protein: Number(e.target.value) })}>
                                {PROTEIN_OPTIONS.map(p => <option key={p} value={p}>{num(p)} g per kg</option>)}
                            </select>
                        </label>
                    </div>
                    <p className="tool-history">
                        {activity === null && (sessions !== null
                            ? <>Activity picked from your training: {num(sessions)} sessions a week over the last 4 weeks.</>
                            : <>Activity picked from your goal of {user?.activeDays ?? 0} days a week.</>)}
                    </p>

                    <div className="tool-split">
                        <div className="tool-result" aria-live="polite">
                            <span className="tool-result-label">Daily target</span>
                            {plan ? (
                                <>
                                    <p className="tool-result-value">{kcal(plan.kcal)}<span className="tool-result-range">kcal</span></p>
                                    <p className="tool-muted">
                                        Maintenance ≈ {kcal(plan.maintenance)} kcal
                                        {plan.kgPerWeek !== 0 && (plan.capped
                                            // At the floor the deficit is smaller than chosen; say what it actually gives.
                                            ? <> · {signed(plan.kcal - plan.maintenance, 0)}, about {signed(((plan.kcal - plan.maintenance) * 7) / KCAL_PER_KG, 2)} kg a week</>
                                            : <> · {signed(plan.kcal - plan.maintenance, 0)} for {signed(plan.kgPerWeek, 2)} kg a week</>)}
                                    </p>
                                    {plan.capped && (
                                        <p className="tool-note">That rate would put you under 1.1 × your resting energy, so this is the lowest target Bloom gives. Pick a slower rate.</p>
                                    )}
                                    <p className="tool-muted">
                                        Resting energy {kcal(plan.bmr)} kcal ({plan.formula === "katch" ? "Katch-McArdle, from your lean mass" : "Mifflin-St Jeor"}).
                                    </p>
                                    <p className="tool-history-best macro-caveat">
                                        An estimate, usually within 10%. After 2–3 weeks, check your weight trend and adjust by 100–200 kcal.
                                        {trendRate && <span>Your trend over the last 4 weeks: <b>{signed(trendRate.kgPerWeek, 2)} kg a week</b>.</span>}
                                    </p>
                                </>
                            ) : (
                                <>
                                    <p className="tool-result-value tool-result-empty">—</p>
                                    <p className="tool-muted">Add {missing} to work out a target.</p>
                                </>
                            )}
                        </div>

                        <div className="tool-table-wrap">
                            <div className="tool-table-head"><h2 className="history-title">Macros per day</h2></div>
                            {plan ? (
                                <>
                                    <ul className="macro-bars">
                                        {macroRows.map((r, i) => {
                                            const share = shares[i] / 100;
                                            return (
                                                <li key={r.key} className={`macro-row ${r.key}`}>
                                                    <span className="macro-name">{r.label}</span>
                                                    <span className="macro-grams">{r.grams} g</span>
                                                    <span className="macro-track" aria-hidden><span style={{ width: `${share * 100}%` }} /></span>
                                                    <span className="macro-share">{shares[i]}%</span>
                                                </li>
                                            );
                                        })}
                                    </ul>
                                    <p className="tool-muted">
                                        Protein {num(proteinPerKg)} g per kg · fat 25% of calories (at least 0.6 g per kg) · carbs the rest.
                                    </p>
                                    {plan.macros.lowCarb && <p className="tool-note">Carbs end up under 2 g per kg at this target, which can make hard training feel heavy.</p>}
                                </>
                            ) : (
                                <p className="tool-muted">Your protein, fat and carbs show up here.</p>
                            )}
                        </div>
                    </div>
                </section>
            </div>
        </div>
    );
}

export default MacroPage;
