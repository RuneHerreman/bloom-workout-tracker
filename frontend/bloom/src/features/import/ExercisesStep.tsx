import { useEffect, useMemo, useRef, useState } from "react";
import { ArrowLeft, ArrowRight, Ban, Check, CheckCheck, ChevronRight, CircleHelp, Plus, Search, Sparkles, CircleCheck, CirclePlus } from "lucide-react";
import type { Exercise } from "../../assets/js/data/apiTypes.ts";
import type { ExerciseType } from "../../types.ts";
import { nameSimilarity, rankExercises } from "../export/importData.ts";
import { candidatesFor, choiceProblem, statusOf, type Choice, type Evidence, type Status } from "./importSession.ts";

const TYPES: ExerciseType[] = ["Strength", "Cardio", "Plyometric"];
const plural = (n: number, w: string) => `${n} ${w}${n === 1 ? "" : "s"}`;
const span = (a: Date | null, b: Date | null) => {
    if (!a || !b) return "";
    const f = (d: Date) => d.toLocaleDateString("en-GB", { month: "short", year: "numeric" });
    return f(a) === f(b) ? f(a) : `${f(a)} to ${f(b)}`;
};

type Filter = Status | "all";

interface Props {
    keys: string[];
    evidence: Map<string, Evidence>;
    choices: Map<string, Choice>;
    catalog: Exercise[];
    logged: Map<string, number>;
    onChoose: (key: string, choice: Choice) => void;
    onBulk: (updates: [string, Choice][]) => void;
    onBack: () => void;
    onNext: () => void;
}

/** Step 2: every exercise name in the file on the left, the one you're deciding on the right. */
function ExercisesStep({ keys, evidence, choices, catalog, logged, onChoose, onBulk, onBack, onNext }: Props) {
    const byId = useMemo(() => new Map(catalog.map(e => [e.id, e])), [catalog]);
    const catalogNames = useMemo(() => new Set(catalog.map(e => e.name.trim().toLowerCase())), [catalog]);
    const muscleOptions = useMemo(() => [...new Set(catalog.flatMap(e => e.targetMuscles))].sort(), [catalog]);

    // Names that need a look first, then the biggest ones.
    const ordered = useMemo(() => [...keys].sort((a, b) => {
        const sa = statusOf(choices.get(a)) === "check" ? 0 : 1, sb = statusOf(choices.get(b)) === "check" ? 0 : 1;
        return sa - sb || (evidence.get(b)?.sets ?? 0) - (evidence.get(a)?.sets ?? 0);
        // Order is fixed when the step opens so rows don't jump while you work.
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }), [keys, evidence]);

    const counts = { check: 0, matched: 0, new: 0, skip: 0 } as Record<Status, number>;
    for (const k of keys) counts[statusOf(choices.get(k))]++;
    const [filter, setFilter] = useState<Filter>(counts.check > 0 ? "check" : "all");
    const visible = ordered.filter(k => filter === "all" || statusOf(choices.get(k)) === filter);
    const [selected, setSelected] = useState<string | null>(() => ordered.find(k => statusOf(choices.get(k)) === "check") ?? ordered[0] ?? null);
    const [detailOpen, setDetailOpen] = useState(false); // phones: the detail covers the list
    const listRef = useRef<HTMLUListElement>(null);

    const newNamesExcept = (key: string) => new Set(
        keys.filter(k => k !== key).map(k => choices.get(k)?.decision).filter(d => d?.kind === "create").map(d => (d as { name: string }).name.trim().toLowerCase())
    );
    const problems = keys.filter(k => choiceProblem(choices.get(k), catalogNames, newNamesExcept(k)));

    // After a decision, move to the next name that still needs a look.
    function decide(key: string, choice: Choice) {
        onChoose(key, choice);
        const i = ordered.indexOf(key);
        const next = [...ordered.slice(i + 1), ...ordered.slice(0, i)].find(k => k !== key && statusOf(choices.get(k)) === "check");
        if (next) setSelected(next);
        else setDetailOpen(false);
    }

    const suggestions = keys.filter(k => { const c = choices.get(k); return c && !c.checked && c.decision.kind === "existing" && c.decision.exerciseId; });
    const unsure = keys.filter(k => { const c = choices.get(k); return c && !c.checked && c.decision.kind === "create"; });

    // Keyboard: ↑/↓ move through the list, Enter accepts the best match, N adds as new.
    const detailApi = useRef<{ acceptBest: () => void; addNew: () => void; skip: () => void } | null>(null);
    useEffect(() => {
        function onKey(e: KeyboardEvent) {
            const t = e.target as HTMLElement;
            if (t.closest("input, textarea, select") || e.metaKey || e.ctrlKey || e.altKey) return;
            const i = selected ? visible.indexOf(selected) : -1;
            if (e.key === "ArrowDown" || e.key === "ArrowUp") {
                e.preventDefault();
                const next = visible[Math.min(visible.length - 1, Math.max(0, i + (e.key === "ArrowDown" ? 1 : -1)))];
                if (next) setSelected(next);
            } else if (e.key === "Enter" && !t.closest("button, a")) {
                e.preventDefault();
                detailApi.current?.acceptBest();
            } else if (e.key.toLowerCase() === "n") {
                e.preventDefault();
                detailApi.current?.addNew();
            } else if (e.key.toLowerCase() === "s") {
                e.preventDefault();
                detailApi.current?.skip();
            }
        }
        window.addEventListener("keydown", onKey);
        return () => window.removeEventListener("keydown", onKey);
    }, [visible, selected]);

    useEffect(() => {
        listRef.current?.querySelector<HTMLElement>(".imp-name.selected")?.scrollIntoView({ block: "nearest" });
    }, [selected]);

    const done = counts.matched + counts.new;
    const sel = selected ? evidence.get(selected) : undefined;

    return (
        <div className="imp-step">
            <div className={`imp-split${detailOpen ? " detail-open" : ""}`}>
                <section className="imp-card imp-queue" aria-label="Exercise names in your file">
                    <div className="imp-queue-head">
                        <div className="imp-queue-title">
                            <b>{plural(keys.length, "exercise name")}</b>
                            <span>{done} of {keys.length} done</span>
                        </div>
                        <div className="imp-progress" aria-hidden><span style={{ width: `${keys.length ? (done / keys.length) * 100 : 100}%` }} /></div>
                        <div className="imp-filters" role="tablist" aria-label="Show">
                            {([["check", "To check"], ["matched", "Matched"], ["new", "New"], ["skip", "Skipped"], ["all", "All"]] as [Filter, string][]).filter(([f]) => f !== "skip" || counts.skip > 0).map(([f, label]) => (
                                <button key={f} role="tab" aria-selected={filter === f} className={`imp-filter ${f}${filter === f ? " active" : ""}`} onClick={() => setFilter(f)}>
                                    {label} <span>{f === "all" ? keys.length : counts[f as Status]}</span>
                                </button>
                            ))}
                        </div>
                    </div>
                    {(suggestions.length > 0 || unsure.length > 0) && (
                        <div className="imp-bulk">
                            {suggestions.length > 0 && (
                                <button type="button" onClick={() => onBulk(suggestions.map(k => [k, { ...choices.get(k)!, checked: true }]))}>
                                    <CheckCheck size={14} /> Accept {plural(suggestions.length, "suggestion")}
                                </button>
                            )}
                            {unsure.length > 0 && (
                                <button type="button" onClick={() => onBulk(unsure.map(k => [k, { ...choices.get(k)!, checked: true }]))}>
                                    <Plus size={14} /> Add {unsure.length} as new
                                </button>
                            )}
                        </div>
                    )}
                    <ul className="imp-names" ref={listRef}>
                        {visible.length === 0 && <li className="imp-empty">Nothing here. {filter === "check" ? "Every name is decided." : ""}</li>}
                        {visible.map(k => {
                            const ev = evidence.get(k)!;
                            const c = choices.get(k);
                            const st = statusOf(c);
                            const target = c?.decision.kind === "existing" ? byId.get(c.decision.exerciseId ?? "")?.name : null;
                            const bad = problems.includes(k);
                            return (
                                <li key={k}>
                                    <button
                                        type="button"
                                        className={`imp-name ${st}${selected === k ? " selected" : ""}${bad ? " bad" : ""}`}
                                        onClick={() => { setSelected(k); setDetailOpen(true); }}
                                    >
                                        <span className="imp-name-icon" aria-hidden>
                                            {st === "matched" ? <CircleCheck size={16} /> : st === "new" ? <CirclePlus size={16} /> : st === "skip" ? <Ban size={16} /> : target ? <Sparkles size={16} /> : <CircleHelp size={16} />}
                                        </span>
                                        <span className="imp-name-text">
                                            <span className="imp-name-file">{ev.name}</span>
                                            <span className="imp-name-sub">
                                                {st === "matched" ? `→ ${target}` : st === "new" ? `New · ${c?.decision.kind === "create" ? c.decision.type : ""}` : st === "skip" ? "Not imported" : target ? `${target}?` : "No close match"}
                                                {" · "}{plural(ev.sets, "set")}
                                            </span>
                                        </span>
                                        <ChevronRight size={15} className="imp-name-chev" aria-hidden />
                                    </button>
                                </li>
                            );
                        })}
                    </ul>
                </section>

                <section className="imp-card imp-detail" aria-label="Decide">
                    {selected && sel ? (
                        <Detail
                            key={selected}
                            ev={sel}
                            choice={choices.get(selected)!}
                            catalog={catalog}
                            byId={byId}
                            logged={logged}
                            muscleOptions={muscleOptions}
                            problem={choiceProblem(choices.get(selected), catalogNames, newNamesExcept(selected))}
                            onDecide={c => decide(selected, c)}
                            onChange={c => onChoose(selected, c)}
                            onClose={() => setDetailOpen(false)}
                            apiRef={detailApi}
                        />
                    ) : <p className="imp-muted">Choose a name on the left.</p>}
                </section>
            </div>

            <div className="imp-footer" role="group" aria-label="Actions">
                <button type="button" className="imp-btn ghost" onClick={onBack}><ArrowLeft size={15} /> Back</button>
                <div className="imp-footer-right">
                    <span className="imp-muted">
                        {problems.length > 0
                            ? `${plural(problems.length, "name")} still need${problems.length === 1 ? "s" : ""} a fix`
                            : counts.check > 0 ? `${counts.check} left to check · they import as shown` : "All exercises decided"}
                    </span>
                    <button type="button" className="imp-btn primary" disabled={problems.length > 0} onClick={onNext}>
                        Review import <ArrowRight size={15} />
                    </button>
                </div>
            </div>
            <p className="imp-keys imp-muted" aria-hidden><kbd>↑</kbd><kbd>↓</kbd> move · <kbd>Enter</kbd> use best match · <kbd>N</kbd> add as new · <kbd>S</kbd> skip</p>
        </div>
    );
}

interface DetailProps {
    ev: Evidence;
    choice: Choice;
    catalog: Exercise[];
    byId: Map<string, Exercise>;
    logged: Map<string, number>;
    muscleOptions: string[];
    problem: string | null;
    onDecide: (c: Choice) => void;
    onChange: (c: Choice) => void;
    onClose: () => void;
    apiRef: React.MutableRefObject<{ acceptBest: () => void; addNew: () => void; skip: () => void } | null>;
}

function Detail({ ev, choice, catalog, byId, logged, muscleOptions, problem, onDecide, onChange, onClose, apiRef }: DetailProps) {
    const { best, alternatives } = useMemo(() => candidatesFor(ev.name, catalog), [ev.name, catalog]);
    const [query, setQuery] = useState("");
    const initialNew = choice.decision.kind === "create" ? choice.decision : { kind: "create" as const, name: ev.name, type: ev.type ?? "Strength", muscles: ev.muscles };
    const [draft, setDraft] = useState(initialNew);

    const results = useMemo(() => {
        const q = query.trim().toLowerCase();
        if (!q) return [];
        const contains = catalog.filter(e => e.name.toLowerCase().includes(q)).sort((a, b) => nameSimilarity(q, b.name) - nameSimilarity(q, a.name));
        const similar = rankExercises(q, catalog).filter(r => r.score >= 0.5 && !contains.includes(r.exercise)).map(r => r.exercise);
        return [...contains, ...similar].slice(0, 6);
    }, [query, catalog]);

    const chosenId = choice.decision.kind === "existing" ? choice.decision.exerciseId : null;
    const pick = (id: string) => onDecide({ decision: { kind: "existing", exerciseId: id }, checked: true });
    const addNew = () => onDecide({ decision: { ...draft, name: draft.name.trim() || ev.name }, checked: true });

    const skip = () => onDecide({ decision: { kind: "skip" }, checked: true });
    useEffect(() => {
        apiRef.current = { acceptBest: () => best && pick(best.exercise.id), addNew, skip };
    });

    const chosen = chosenId ? byId.get(chosenId) : null;
    const status = !choice.checked
        ? chosen ? <>Suggested: <b>{chosen.name}</b>. Use it, or choose another below.</> : <>No close match. Pick an exercise or add it as new.</>
        : chosen ? <><Check size={14} /> Linked to <b>{chosen.name}</b></>
        : choice.decision.kind === "skip" ? <><Ban size={14} /> Not imported: its sets are left out</>
        : <><Check size={14} /> Added as a new <b>{choice.decision.kind === "create" ? choice.decision.type : ""}</b> exercise</>;

    return (
        <div className="imp-detail-inner">
            <button type="button" className="imp-btn ghost imp-detail-back" onClick={onClose}><ArrowLeft size={15} /> All names</button>

            <div>
                <p className="imp-eyebrow">In your file</p>
                <h2 className="imp-detail-name">{ev.name}</h2>
                <p className="imp-muted">
                    {plural(ev.sets, "set")} in {plural(ev.workouts, "workout")}{ev.templates ? `, ${plural(ev.templates, "template")}` : ""}
                    {ev.first && <> · {span(ev.first, ev.last)}</>}
                    {ev.range && <> · {ev.range}</>}
                </p>
                {ev.samples.length > 0 && (
                    <div className="imp-samples">{ev.samples.map(s => <span key={s}>{s}</span>)}</div>
                )}
            </div>

            <p className={`imp-status${choice.checked ? " done" : ""}`}>{status}</p>

            {best && (
                <div className={`imp-best${chosenId === best.exercise.id ? " chosen" : ""}`}>
                    <div>
                        <p className="imp-eyebrow accent">{best.score >= 0.99 ? "Same name" : "Best match"}</p>
                        <p className="imp-best-name">{best.exercise.name}</p>
                        <p className="imp-muted small">
                            {best.exercise.type}{best.exercise.targetMuscles.length ? ` · ${best.exercise.targetMuscles.join(", ")}` : ""}
                            {logged.get(best.exercise.id) ? ` · you've logged it ${plural(logged.get(best.exercise.id)!, "time")}` : ""}
                        </p>
                    </div>
                    <button type="button" className={`imp-btn ${chosenId === best.exercise.id && choice.checked ? "chosen" : "primary"}`} onClick={() => pick(best.exercise.id)}>
                        {chosenId === best.exercise.id && choice.checked ? <><Check size={14} /> Using this</> : <>Use this <kbd>↵</kbd></>}
                    </button>
                </div>
            )}

            <div className="imp-other">
                <p className="imp-eyebrow">{best ? "Or another exercise" : "Match to an exercise"}</p>
                {alternatives.length > 0 && (
                    <div className="imp-chips">
                        {alternatives.map(a => (
                            <button key={a.id} type="button" className={`imp-chip${chosenId === a.id ? " chosen" : ""}`} onClick={() => pick(a.id)}>{a.name}</button>
                        ))}
                    </div>
                )}
                <label className="imp-search">
                    <Search size={14} aria-hidden />
                    <input type="text" value={query} placeholder={`Search your ${catalog.length} exercises`} onChange={e => setQuery(e.target.value)} aria-label="Search your exercises" />
                </label>
                {results.length > 0 && (
                    <ul className="imp-results">
                        {results.map(r => (
                            <li key={r.id}>
                                <button type="button" onClick={() => pick(r.id)} className={chosenId === r.id ? "chosen" : ""}>
                                    <span>{r.name}</span>
                                    <small>{r.type}{r.targetMuscles.length ? ` · ${r.targetMuscles.slice(0, 2).join(", ")}` : ""}</small>
                                </button>
                            </li>
                        ))}
                    </ul>
                )}
                {query.trim() && results.length === 0 && <p className="imp-muted small">No exercise called “{query.trim()}”. Add it as new below.</p>}
            </div>

            <div className={`imp-new${choice.decision.kind === "create" ? " chosen" : ""}`}>
                <p className="imp-eyebrow">Not one of these? Add it as new</p>
                <div className="imp-new-row">
                    <input
                        type="text"
                        value={draft.name}
                        aria-label="Name of the new exercise"
                        onChange={e => {
                            const next = { ...draft, name: e.target.value };
                            setDraft(next);
                            if (choice.decision.kind === "create") onChange({ ...choice, decision: next });
                        }}
                    />
                </div>
                <div className="imp-types" role="radiogroup" aria-label="Type">
                    {TYPES.map(t => (
                        <button key={t} type="button" role="radio" aria-checked={draft.type === t} className={draft.type === t ? "active" : ""}
                            onClick={() => { const next = { ...draft, type: t }; setDraft(next); if (choice.decision.kind === "create") onChange({ ...choice, decision: next }); }}>
                            {t}
                        </button>
                    ))}
                </div>
                <details className="imp-muscles">
                    <summary>Muscles {draft.muscles.length > 0 ? `· ${draft.muscles.join(", ")}` : "(optional, used by Insights)"}</summary>
                    <div className="imp-chips">
                        {muscleOptions.map(m => {
                            const on = draft.muscles.includes(m);
                            return (
                                <button key={m} type="button" className={`imp-chip${on ? " chosen" : ""}`} aria-pressed={on}
                                    onClick={() => {
                                        const next = { ...draft, muscles: on ? draft.muscles.filter(x => x !== m) : [...draft.muscles, m] };
                                        setDraft(next);
                                        if (choice.decision.kind === "create") onChange({ ...choice, decision: next });
                                    }}>
                                    {m}
                                </button>
                            );
                        })}
                    </div>
                </details>
                {choice.decision.kind === "create" && problem && <p className="imp-problem">{problem}</p>}
                <button type="button" className={`imp-btn ${choice.decision.kind === "create" && choice.checked ? "chosen" : "secondary"}`} onClick={addNew}>
                    {choice.decision.kind === "create" && choice.checked ? <><Check size={14} /> Adding as new</> : <><Plus size={14} /> Add as new exercise <kbd>N</kbd></>}
                </button>
            </div>

            <div className="imp-skip">
                <span className="imp-muted small">Not a real exercise, like a placeholder or a note?</span>
                <button type="button" className={`imp-btn ${choice.decision.kind === "skip" ? "chosen" : "ghost"}`} onClick={skip}>
                    {choice.decision.kind === "skip" ? <><Check size={14} /> Skipped</> : <><Ban size={14} /> Don't import it <kbd>S</kbd></>}
                </button>
            </div>
        </div>
    );
}

export default ExercisesStep;
