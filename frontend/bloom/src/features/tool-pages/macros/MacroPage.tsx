import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { ChevronDown } from "lucide-react";
import type { BodyEntry, LoggedWorkout, Sex, User } from "../../../assets/js/data/apiTypes.ts";
import { getMe, peekMe, updateSex } from "../../auth/api.ts";
import { getBody, peekBody, localToday, formatEntryDate } from "../../body/api.ts";
import { getLogs, peekLogs } from "../../logbook/api.ts";
import { buildSeries, dayIndex } from "../../insights/bodySeries.ts";
import {
    ACTIVITY_LEVELS, KCAL_PER_KG, activityFactor, activityFromSessions, ageInYears, weeklyRate, type ActivityKey, type Goal,
} from "../../../utils/bodyMath.ts";
import { DEFAULT_RATE, PROTEIN_OPTIONS, RATES, macroPlan } from "./macroPlan.ts";
import PlanChart from "./PlanChart.tsx";
import LogWeightButton from "../../body/LogWeightButton.tsx";
import InfoHint from "../../../components/general/InfoHint.tsx";
import "../../../assets/css/insights.css";
import "../../../assets/css/tools.css";

const DAY = 86_400_000;
/** A body fat reading older than this isn't used for resting energy. */
const BODY_FAT_MAX_AGE_DAYS = 60;
/** A weigh-in older than this makes the weight, and so the target, stale. */
const STALE_WEIGHT_DAYS = 14;
/** How much weight history the plan chart shows. */
const HISTORY_DAYS = 56;
const STORE = "bloom.macros";
const MEALS = [3, 4, 5];
/** How far ahead the plan looks: far enough to mean something, short enough that 7700 kcal/kg still holds. */
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
/** "3 wk ago", "5 mo ago", "2 y ago": how old a weigh-in is, short enough for one row. */
const ago = (days: number) => days < 60 ? `${Math.round(days / 7)} wk ago` : days < 730 ? `${Math.round(days / 30.4)} mo ago` : `${Math.floor(days / 365)} y ago`;
const parse = (v: string) => { const n = Number(v.trim().replace(",", ".")); return v.trim() && Number.isFinite(n) ? n : null; };

/** Small line icons for the "You" rows, drawn inline (24 × 24, stroke only). */
const ICONS = {
    weight: "M6 7h12l2 12H4L6 7Z M9 7a3 3 0 0 1 6 0 M12 11l1.5 2.5",
    height: "M4 17 17 4l3 3L7 20Z M8 13l1.5 1.5 M11 10l1.5 1.5 M14 7l1.5 1.5",
    fat: "M12 3s6 6.5 6 11a6 6 0 0 1-12 0c0-4.5 6-11 6-11Z",
    sex: "M12 12a4 4 0 1 0 0-8 4 4 0 0 0 0 8Z M4 21a8 8 0 0 1 16 0",
    age: "M4 9h16v11H4Z M4 13h16 M8 3v4 M16 3v4",
} as const;

function Icon({ d }: { d: string }) {
    return (
        <svg className="tool-you-icon" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor"
            strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden>
            <path d={d} />
        </svg>
    );
}

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

/** Whole percentages that add up to 100 (largest remainder), so the bar never reads 101%. */
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

/**
 * How your real trend compares with the plan, and what to change if it's off.
 * The adjustment is the gap in kg a week turned into daily kcal, rounded to 50
 * and kept to 100–300 kcal: enough to matter, small enough to check again after.
 */
function planCheck(planned: number, actual: number): { tone: "ok" | "off"; text: string } {
    const gapKcal = ((planned - actual) * KCAL_PER_KG) / 7;
    const adjust = Math.sign(gapKcal) * Math.min(300, Math.max(100, Math.round(Math.abs(gapKcal) / 50) * 50));
    const change = `${adjust > 0 ? "+" : "−"}${Math.abs(adjust)} kcal`;
    if (Math.abs(planned) < 0.01) {
        return Math.abs(actual) <= 0.15
            ? { tone: "ok", text: "Your weight is holding steady, as planned." }
            : { tone: "off", text: `You're ${actual < 0 ? "losing" : "gaining"} ${num(Math.abs(actual), 2)} kg a week instead of holding. Try ${change} a day.` };
    }
    const ratio = actual / planned;
    if (ratio >= 0.6 && ratio <= 1.5) return { tone: "ok", text: "Your trend matches the plan. Keep going." };
    const word = planned < 0 ? "losing" : "gaining";
    const how = ratio < 0 ? `${planned < 0 ? "gaining" : "losing"} ${num(Math.abs(actual), 2)} kg a week` : `${word} ${num(Math.abs(actual), 2)} kg a week`;
    return { tone: "off", text: `You're ${how}, against ${num(Math.abs(planned), 2)} planned. Try ${change} a day, then check again in 2–3 weeks.` };
}

function MacroPage() {
    const [user, setUser] = useState<User | null>(() => peekMe() ?? null);
    const [body, setBody] = useState<BodyEntry[]>(() => peekBody() ?? []);
    const [logs, setLogs] = useState<LoggedWorkout[]>(() => peekLogs() ?? []);

    const [weight, setWeight] = useState("");
    const [height, setHeight] = useState("");
    const [bodyFat, setBodyFat] = useState("");
    const [activity, setActivity] = useState<ActivityKey | null>(null);
    const [activityOpen, setActivityOpen] = useState(false);
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
    const today = dayIndex(localToday());
    const lastWeighIn = weightSeries.at(-1) ?? null;
    const weighInAge = lastWeighIn ? today - lastWeighIn.day : null;
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
    const recentWeights = useMemo(() => weightSeries.filter(p => p.day >= today - HISTORY_DAYS), [weightSeries, today]);
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

    /** A new weigh-in: add it, and let the weight refill from the new trend. */
    function handleLogged(entry: BodyEntry) {
        setBody(prev => [...prev.filter(e => e.date !== entry.date), entry]);
        setWeight("");
    }

    const sex = user?.sex ?? null;
    const recommendedProtein = saved.goal === "lose" ? 2.2 : 1.6;
    const proteinPerKg = saved.protein ?? recommendedProtein;
    const activeKey = activity ?? suggestedActivity;
    const activeLevel = ACTIVITY_LEVELS.find(a => a.key === activeKey)!;
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
    const change = plan ? plan.kcal - plan.maintenance : 0;
    const check = plan && trendRate ? planCheck(realKgPerWeek, trendRate.kgPerWeek) : null;
    const staleWeight = weighInAge !== null && weighInAge > STALE_WEIGHT_DAYS;
    const showChart = plan && recentWeights.length >= 2 && !staleWeight;

    const youMissing = !w || !h || !sex;
    // Opened because sex isn't set: stay open until "Done", so picking it doesn't
    // collapse the rail under your pointer. (Weight and height are prefilled.)
    if (user && !sex && !editingYou) setEditingYou(true);
    const showYouFields = editingYou || youMissing;

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
                            <dl className="tool-you-rows">
                                <div className={staleWeight ? "stale" : undefined}>
                                    <dt><Icon d={ICONS.weight} />Weight</dt>
                                    <dd>
                                        {w ? `${num(w)} kg` : "—"}
                                        {staleWeight && lastWeighIn && weighInAge !== null && (
                                            <small title={`Last weigh-in: ${formatEntryDate(lastWeighIn.date)}`}>{ago(weighInAge)}</small>
                                        )}
                                    </dd>
                                    {staleWeight && <LogWeightButton onLogged={handleLogged} className="tool-you-log" />}
                                </div>
                                <div><dt><Icon d={ICONS.height} />Height</dt><dd>{h ? `${num(h, 0)} cm` : "—"}</dd></div>
                                {bf !== null && <div><dt><Icon d={ICONS.fat} />Body fat</dt><dd>{num(bf)}%</dd></div>}
                                <div><dt><Icon d={ICONS.sex} />Sex</dt><dd>{sex ?? "—"}</dd></div>
                                <div><dt><Icon d={ICONS.age} />Age</dt><dd>{age !== null ? age : "—"}</dd></div>
                            </dl>
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
                            {staleWeight
                                ? <>Your weight is from an old weigh-in; log a new one so the target fits. </>
                                : trendWeight ? <>Weight is your body log trend{weightSeries.length === 0 ? " (or profile weight)" : ""}. </> : null}
                            {age !== null ? <>The rest is from your <Link to="/profile">profile</Link>.</> : <>Add your date of birth in your <Link to="/profile">profile</Link>.</>}
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
                            <span>Rate <em>per week</em></span>
                            <div className="segmented tool-segmented two-line" role="radiogroup" aria-label="Rate per week">
                                {RATES[saved.goal].map(r => (
                                    <button key={r} type="button" role="radio" aria-checked={saved.rate === r}
                                        className={`segmented-item${saved.rate === r ? " active" : ""}`} onClick={() => save({ rate: r })}>
                                        {w ? <>{signed((w * (saved.goal === "lose" ? -r : r)) / 100, 1)} kg<small>{num(r, 2)}%</small></> : `${num(r, 2)}%`}
                                    </button>
                                ))}
                            </div>
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
                                    aria-description={p === recommendedProtein ? "Suggested" : undefined}
                                    className={`segmented-item${proteinPerKg === p ? " active" : ""}${p === recommendedProtein ? " suggested" : ""}`}
                                    onClick={() => save({ protein: p })}>
                                    {num(p)}
                                </button>
                            ))}
                        </div>
                        <p className="tool-hint">
                            {w ? <><b>{Math.round(w * proteinPerKg)} g</b> a day · </> : null}
                            <span className="tool-dot-key" aria-hidden /> suggested {saved.goal === "lose" ? "for a cut" : saved.goal === "gain" ? "for a bulk" : "to maintain"}
                        </p>
                    </div>

                    <div className="tool-field">
                        <span>Activity</span>
                        <button type="button" className="tool-choice tool-choice-summary" aria-expanded={activityOpen} aria-label={`Activity: ${activeLevel.label}, ${ACTIVITY_DESC[activeKey].toLowerCase()}. Change`}
                            onClick={() => setActivityOpen(o => !o)}>
                            <span className="tool-choice-title">
                                {activeLevel.label}
                                {activeKey === suggestedActivity && <span className="tool-tag">Your training</span>}
                                <ChevronDown size={15} className="tool-choice-chevron" aria-hidden />
                            </span>
                            <span className="tool-choice-desc">{ACTIVITY_DESC[activeKey]} · × {activeLevel.factor}</span>
                        </button>
                        {activityOpen && (
                            <div className="tool-choices" role="radiogroup" aria-label="Activity level">
                                {ACTIVITY_LEVELS.filter(a => a.key !== activeKey).map(a => (
                                    <button key={a.key} type="button" role="radio" aria-checked={false}
                                        className="tool-choice" onClick={() => { setActivity(a.key); setActivityOpen(false); }}>
                                        <span className="tool-choice-title">
                                            {a.label}
                                            {a.key === suggestedActivity && <span className="tool-tag">Your training</span>}
                                        </span>
                                        <span className="tool-choice-desc">{ACTIVITY_DESC[a.key]} · × {a.factor}</span>
                                    </button>
                                ))}
                            </div>
                        )}
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
                                    <p className="tool-sum">
                                        <span className="tool-label-hint">
                                            Maintenance {kcal(plan.maintenance)}
                                            <InfoHint title="Maintenance" label="How is maintenance worked out?">
                                                <p>
                                                    Resting energy {kcal(plan.bmr)} kcal ({plan.formula === "katch" ? "Katch-McArdle, from your lean mass" : "Mifflin-St Jeor, from weight, height, age and sex"}),
                                                    times {activityFactor(activeKey)} for {activeLevel.label.toLowerCase()} activity.
                                                </p>
                                            </InfoHint>
                                        </span>
                                        {change !== 0 && <> {change < 0 ? "−" : "+"} {kcal(Math.abs(change))} {change < 0 ? "deficit" : "surplus"}</>}
                                        {realKgPerWeek !== 0 && <span className="tool-sum-rate"> · {signed(realKgPerWeek, 1)} kg a week</span>}
                                    </p>
                                    {plan.capped && (
                                        <p className="tool-note">That rate would put you under 1.1 × your resting energy, so this is the lowest target Bloom gives. Pick a slower rate.</p>
                                    )}
                                </>
                            ) : (
                                <>
                                    <p className="tool-result-value tool-result-empty">—</p>
                                    <p className="tool-muted">Add {missing} to work out a target.</p>
                                </>
                            )}
                        </div>
                    </section>

                    <section className="panel" aria-label="Macros per day">
                        <div className="tool-table-head">
                            <div className="panel-head">
                                <h2 className="panel-title">Macros per day</h2>
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
                                <ul className="macro-figures">
                                    {macroRows.map(r => (
                                        <li key={r.key} className={`macro-figure ${r.key}`}>
                                            <span className="macro-figure-name"><span className="macro-dot" aria-hidden />{r.label}</span>
                                            <b>{r.grams} g</b>
                                            <span>{Math.round(r.grams / saved.meals)} g a meal</span>
                                            {w ? <span>{num(r.grams / w)} g per kg</span> : null}
                                            <span>{kcal(r.grams * r.kcalPerG)} kcal</span>
                                        </li>
                                    ))}
                                </ul>
                                <div className="macro-stack" role="img" aria-label={`Share of calories: ${macroRows.map((r, i) => `${r.label} ${shares[i]}%`).join(", ")}`}>
                                    {macroRows.map((r, i) => (
                                        <span key={r.key} className={r.key} style={{ flexGrow: shares[i] }}>{shares[i] >= 8 ? `${shares[i]}%` : ""}</span>
                                    ))}
                                </div>
                                {plan.macros.lowCarb && <p className="tool-note">Carbs end up under 2 g per kg at this target, which can make hard training feel heavy.</p>}
                            </>
                        ) : (
                            <p className="tool-muted">Your protein, fat and carbs show up here.</p>
                        )}
                    </section>

                    {plan && (
                        <section className="panel" aria-label="Plan against your weight">
                            <div className="panel-head">
                                <h2 className="panel-title">The plan against your weight</h2>
                                <p className="panel-desc">
                                    {realKgPerWeek !== 0 && w
                                        ? <>At {signed(realKgPerWeek, 1)} kg a week: about <b>{num(w + realKgPerWeek * PROJECTION_WEEKS)} kg</b> by {new Date(Date.now() + PROJECTION_WEEKS * 7 * DAY).toLocaleDateString("en-GB", { day: "numeric", month: "short" })}.</>
                                        : <>Holding at about {num(w ?? 0)} kg.</>}
                                </p>
                            </div>
                            {check && !staleWeight && <p className={`tool-check ${check.tone}`}>{check.text}</p>}
                            {showChart ? (
                                <PlanChart points={recentWeights} kgPerWeek={realKgPerWeek} weeks={PROJECTION_WEEKS} />
                            ) : (
                                <p className="tool-muted">
                                    {staleWeight
                                        ? "No recent weigh-ins to compare with. Log your weight a few times a week and your trend shows up here."
                                        : "Weigh in a few times over the next weeks and your trend shows up here, next to the plan."}
                                </p>
                            )}
                            <p className="tool-muted">
                                The target is an estimate, usually within 10%. After 2–3 weeks your trend tells you more than any formula: adjust by 100–200 kcal if it's off.
                            </p>
                        </section>
                    )}
                </div>
            </div>
        </div>
    );
}

export default MacroPage;
