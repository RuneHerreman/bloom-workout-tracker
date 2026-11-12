import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import type { BodyEntry, LoggedWorkout, Sex, User } from "../../../assets/js/data/apiTypes.ts";
import { getMe, peekMe, updateSex } from "../../auth/api.ts";
import { getBody, peekBody, localToday } from "../../body/api.ts";
import { getLogs, peekLogs } from "../../logbook/api.ts";
import { buildSeries, dayIndex } from "../../insights/bodySeries.ts";
import {
    ACTIVITY_LEVELS, KCAL_PER_KG, activityFactor, activityFromSessions, ageInYears, weeklyRate, type ActivityKey, type Goal,
} from "../../../utils/bodyMath.ts";
import { DEFAULT_RATE, PROTEIN_OPTIONS, RATES, macroPlan } from "./macroPlan.ts";
import InfoHint from "../../../components/general/InfoHint.tsx";
import "../../../assets/css/insights.css";
import "../../../assets/css/tools.css";

const DAY = 86_400_000;
/** A body fat reading older than this isn't used for resting energy. */
const BODY_FAT_MAX_AGE_DAYS = 60;
const STORE = "bloom.macros";
const MEALS = [3, 4, 5];
/** How far ahead "at this rate" looks: far enough to mean something, short enough that 7700 kcal/kg still holds. */
const PROJECTION_WEEKS = 12;

/** What each level means in training terms; matches how the suggestion is picked from logged sessions. */
const ACTIVITY_DESC: Record<ActivityKey, string> = {
    sedentary: "Desk job, little exercise",
    light: "Training 1–2 times a week",
    moderate: "Training 3–4 times a week",
    very: "Training 5+ times a week",
    extra: "Hard training and a physical job",
};

const GOALS: { value: Goal; label: string }[] = [
    { value: "lose", label: "Lose" },
    { value: "maintain", label: "Maintain" },
    { value: "gain", label: "Gain" },
];

const num = (n: number, d = 1) => n.toLocaleString("en-GB", { maximumFractionDigits: d });
const kcal = (n: number) => Math.round(n).toLocaleString("en-GB");
const signed = (n: number, d = 1) => `${n > 0 ? "+" : n < 0 ? "−" : ""}${num(Math.abs(n), d)}`;
const parse = (v: string) => { const n = Number(v.trim().replace(",", ".")); return v.trim() && Number.isFinite(n) ? n : null; };

interface Saved { goal: Goal; rate: number; protein: number | null; meals: number }
function readSaved(): Saved {
    try {
        const s = JSON.parse(localStorage.getItem(STORE) ?? "{}") as Partial<Saved>;
        const goal = GOALS.some(g => g.value === s.goal) ? s.goal! : "maintain";
        const rate = typeof s.rate === "number" && RATES[goal].includes(s.rate) ? s.rate : DEFAULT_RATE[goal];
        const protein = typeof s.protein === "number" && PROTEIN_OPTIONS.includes(s.protein) ? s.protein : null;
        const meals = typeof s.meals === "number" && MEALS.includes(s.meals) ? s.meals : 3;
        return { goal, rate, protein, meals };
    } catch {
        return { goal: "maintain", rate: 0, protein: null, meals: 3 };
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
    const [editingYou, setEditingYou] = useState(false);

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

    const realKgPerWeek = plan ? (plan.capped ? ((plan.kcal - plan.maintenance) * 7) / KCAL_PER_KG : plan.kgPerWeek) : 0;
    const projection = plan && w && Math.abs(realKgPerWeek) > 0.005
        ? { kg: w + realKgPerWeek * PROJECTION_WEEKS, date: new Date(Date.now() + PROJECTION_WEEKS * 7 * DAY) }
        : null;
    const youMissing = !w || !h || !sex;
    // Opened because sex isn't set: stay open until "Done", so picking it doesn't
    // collapse the rail under your pointer. (Weight and height are prefilled.)
    if (user && !sex && !editingYou) setEditingYou(true);
    const showYouFields = editingYou || youMissing;
    const recommendedProtein = saved.goal === "lose" ? 2.2 : 1.6;

    return (
        <div className="insights-page tool-page">
            <header className="page-header">
                <div className="page-header-titles">
                    <h1>Macros</h1>
                    <p className="header-subtitle">A daily calorie and macro target, worked out from your own numbers</p>
                </div>
            </header>

            <div className="tool-body">
                <div className="tool-peek" aria-hidden>
                    <span>{plan ? `P ${plan.macros.proteinG} · F ${plan.macros.fatG} · C ${plan.macros.carbsG} g` : "Daily target"}</span>
                    <b>{plan ? `${kcal(plan.kcal)} kcal` : "—"}</b>
                </div>
                <aside className="panel tool-rail" aria-label="Your numbers and goal">
                    <div className="tool-you">
                        <div className="tool-you-head">
                            <span className="tool-group-label">You</span>
                            {!youMissing && (
                                <button type="button" className="tool-link-btn" onClick={() => setEditingYou(e => !e)} aria-expanded={showYouFields}>
                                    {showYouFields ? "Done" : "Edit"}
                                </button>
                            )}
                        </div>
                        {!showYouFields ? (
                            <p className="tool-you-summary">
                                {[w && `${num(w)} kg`, h && `${num(h, 0)} cm`, bf !== null && `${num(bf)}% fat`, sex, age !== null && `${age} y`].filter(Boolean).join(" · ")}
                            </p>
                        ) : (
                            <div className="tool-you-fields">
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
                                    <div className="segmented tool-segmented" role="radiogroup" aria-label="Sex">
                                        {(["Male", "Female"] as Sex[]).map(s => (
                                            <button key={s} type="button" role="radio" aria-checked={sex === s} disabled={sexSaving}
                                                className={`segmented-item${sex === s ? " active" : ""}`} onClick={() => sex !== s && pickSex(s)}>
                                                {s}
                                            </button>
                                        ))}
                                    </div>
                                </div>
                            </div>
                        )}
                        <p className="tool-hint">
                            {trendWeight ? <>Weight is your trend from the body log{weightSeries.length === 0 ? " (your profile weight)" : ""}. </> : null}
                            {recentBodyFat && <>Body fat is your logged trend; clear it to ignore it. </>}
                            {age !== null ? <>Age from your <Link to="/profile">profile</Link>.</> : <>Add your date of birth in your <Link to="/profile">profile</Link>.</>}
                        </p>
                    </div>

                    <div className="tool-field">
                        <span>Goal</span>
                        <div className="segmented tool-segmented" role="radiogroup" aria-label="Goal">
                            {GOALS.map(g => (
                                <button key={g.value} type="button" role="radio" aria-checked={saved.goal === g.value}
                                    className={`segmented-item${saved.goal === g.value ? " active" : ""}`} onClick={() => pickGoal(g.value)}>
                                    {g.label}
                                </button>
                            ))}
                        </div>
                    </div>

                    {saved.goal !== "maintain" && (
                        <div className="tool-field">
                            <span>Rate <em>% of body weight a week</em></span>
                            <div className="segmented tool-segmented" role="radiogroup" aria-label="Rate per week">
                                {RATES[saved.goal].map(r => (
                                    <button key={r} type="button" role="radio" aria-checked={saved.rate === r}
                                        className={`segmented-item${saved.rate === r ? " active" : ""}`} onClick={() => save({ rate: r })}>
                                        {num(r, 2)}%
                                    </button>
                                ))}
                            </div>
                            {w ? <p className="tool-hint">About <b>{signed((w * (saved.goal === "lose" ? -saved.rate : saved.rate)) / 100, 2)} kg</b> a week.</p> : null}
                        </div>
                    )}

                    <div className="tool-field">
                        <span className="tool-label-hint">
                            Protein <em>g per kg</em>
                            <InfoHint title="Protein" label="How much protein?">
                                <p>1.4–2.0 g per kg covers muscle building for most people who lift (ISSN). In a deficit, around 2.2 g per kg helps keep muscle while you lose fat.</p>
                            </InfoHint>
                        </span>
                        <div className="segmented tool-segmented" role="radiogroup" aria-label="Protein per kg">
                            {PROTEIN_OPTIONS.map(p => (
                                <button key={p} type="button" role="radio" aria-checked={proteinPerKg === p}
                                    className={`segmented-item${proteinPerKg === p ? " active" : ""}${p === recommendedProtein ? " suggested" : ""}`}
                                    onClick={() => save({ protein: p })}>
                                    {num(p)}
                                </button>
                            ))}
                        </div>
                        <p className="tool-hint">
                            {w ? <><b>{Math.round(w * proteinPerKg)} g</b> a day. </> : null}
                            The dot marks the usual pick when you {saved.goal === "lose" ? "cut" : saved.goal === "gain" ? "bulk" : "maintain"}.
                        </p>
                    </div>

                    <div className="tool-field">
                        <span>Activity</span>
                        <div className="tool-choices" role="radiogroup" aria-label="Activity level">
                            {ACTIVITY_LEVELS.map(a => (
                                <button key={a.key} type="button" role="radio" aria-checked={activeKey === a.key}
                                    className={`tool-choice${activeKey === a.key ? " active" : ""}`} onClick={() => setActivity(a.key)}>
                                    <span className="tool-choice-title">
                                        {a.label}
                                        {a.key === suggestedActivity && <span className="tool-tag">Your training</span>}
                                    </span>
                                    <span className="tool-choice-desc">{ACTIVITY_DESC[a.key]} · × {a.factor}</span>
                                </button>
                            ))}
                        </div>
                        <p className="tool-hint">
                            {sessions !== null
                                ? <>You logged {num(sessions)} sessions a week over the last 4 weeks.</>
                                : <>Suggested from your goal of {user?.activeDays ?? 0} days a week.</>}
                        </p>
                    </div>
                </aside>

                <div className="tool-main">
                    <section className="panel tool-hero" aria-live="polite" aria-label="Daily target">
                        <div className="tool-hero-main">
                            <span className="tool-result-label">Daily target</span>
                            {plan ? (
                                <>
                                    <p className="tool-result-value">{kcal(plan.kcal)}<span className="tool-result-range">kcal</span></p>
                                    {plan.capped && (
                                        <p className="tool-note">That rate would put you under 1.1 × your resting energy, so this is the lowest target Bloom gives. Pick a slower rate.</p>
                                    )}
                                    <p className="tool-muted">An estimate, usually within 10%. After 2–3 weeks, check your weight trend and adjust by 100–200 kcal.</p>
                                </>
                            ) : (
                                <>
                                    <p className="tool-result-value tool-result-empty">—</p>
                                    <p className="tool-muted">Add {missing} to work out a target.</p>
                                </>
                            )}
                        </div>
                        {plan && (
                            <dl className="tool-hero-stats">
                                <div>
                                    <dt>Maintenance</dt>
                                    <dd>{kcal(plan.maintenance)} kcal</dd>
                                    <span>Resting energy × {activityFactor(activeKey)} for activity</span>
                                </div>
                                <div>
                                    <dt>{plan.kcal < plan.maintenance ? "Deficit" : plan.kcal > plan.maintenance ? "Surplus" : "Change"}</dt>
                                    <dd>{plan.kcal === plan.maintenance ? "None" : `${signed(plan.kcal - plan.maintenance, 0)} kcal`}</dd>
                                    <span>{realKgPerWeek ? `${signed(realKgPerWeek, 2)} kg a week` : "Hold your weight"}</span>
                                </div>
                                <div>
                                    <dt>Resting energy</dt>
                                    <dd>{kcal(plan.bmr)} kcal</dd>
                                    <span>{plan.formula === "katch" ? "Katch-McArdle, from your lean mass" : "Mifflin-St Jeor"}</span>
                                </div>
                            </dl>
                        )}
                        {plan && (projection || trendRate) && (
                            <p className="tool-hero-foot">
                                {projection && <>At this rate: <b>about {num(projection.kg)} kg</b> by {projection.date.toLocaleDateString("en-GB", { day: "numeric", month: "short" })}, in {PROJECTION_WEEKS} weeks. </>}
                                {trendRate && <>Your trend over the last 4 weeks: <b>{signed(trendRate.kgPerWeek, 2)} kg a week</b>.</>}
                            </p>
                        )}
                    </section>

                    <section className="panel" aria-label="Macros per day">
                        <div className="tool-table-head">
                            <div className="panel-head">
                                <h2 className="panel-title">Macros</h2>
                                <p className="panel-desc">Protein {num(proteinPerKg)} g per kg · fat 25% of calories (at least 0.6 g per kg) · carbs the rest.</p>
                            </div>
                            <div className="segmented segmented-sm" role="radiogroup" aria-label="Meals a day">
                                {MEALS.map(m => (
                                    <button key={m} type="button" role="radio" aria-checked={saved.meals === m}
                                        className={`segmented-item${saved.meals === m ? " active" : ""}`} onClick={() => save({ meals: m })}>
                                        {m} meals
                                    </button>
                                ))}
                            </div>
                        </div>
                        {plan ? (
                            <>
                                <div className="macro-stack" role="img" aria-label={macroRows.map((r, i) => `${r.label} ${shares[i]}%`).join(", ")}>
                                    {macroRows.map((r, i) => <span key={r.key} className={r.key} style={{ flexGrow: shares[i] }} />)}
                                </div>
                                <div className="tool-table-scroll">
                                    <table className="history-table tool-table macro-table">
                                        <thead>
                                            <tr>
                                                <th>Macro</th>
                                                <th className="num">Per day</th>
                                                <th className="num">Per meal</th>
                                                <th className="num">g per kg</th>
                                                <th className="num">kcal</th>
                                                <th className="num">Share</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            {macroRows.map((r, i) => (
                                                <tr key={r.key} className={`macro-row ${r.key}`}>
                                                    <td><span className="macro-dot" aria-hidden />{r.label}</td>
                                                    <td className="num macro-grams">{r.grams} g</td>
                                                    <td className="num">{Math.round(r.grams / saved.meals)} g</td>
                                                    <td className="num">{w ? num(r.grams / w) : "—"}</td>
                                                    <td className="num">{kcal(r.grams * r.kcalPerG)}</td>
                                                    <td className="num">{shares[i]}%</td>
                                                </tr>
                                            ))}
                                        </tbody>
                                    </table>
                                </div>
                                {plan.macros.lowCarb && <p className="tool-note">Carbs end up under 2 g per kg at this target, which can make hard training feel heavy.</p>}
                            </>
                        ) : (
                            <p className="tool-muted">Your protein, fat and carbs show up here.</p>
                        )}
                    </section>
                </div>
            </div>
        </div>
    );
}

export default MacroPage;
