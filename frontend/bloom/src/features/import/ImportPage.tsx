import { useEffect, useMemo, useRef, useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import {
    AlertTriangle, ArrowLeft, ArrowRight, BookOpen, Braces, Calendar, Check, Download, FileSpreadsheet,
    FileText, Loader2, RotateCcw, Undo2, Upload,
} from "lucide-react";
import "../../assets/css/import.css";
import HeaderComponent from "../../components/general/HeaderComponent.tsx";
import { getLogs, importLogs, undoImport, type ImportResult } from "../logbook/api.ts";
import { createTemplate, deleteTemplate, getTemplates } from "../templates/api.ts";
import { createCustomExercise, deleteCustomExercise, searchExercises } from "../exercises/api.ts";
import type { Exercise, LoggedWorkout, WorkoutTemplate } from "../../assets/js/data/apiTypes.ts";
import { downloadBlob } from "../export/exportData.ts";
import {
    ImportFileError, exampleBlob, parseImportFile, planImport, toImportBodies, toTemplateExercises,
    type DateOrder, type ParsedExerciseRef, type ParsedFile, type ParsedWorkout,
} from "../export/importData.ts";
import ExercisesStep from "./ExercisesStep.tsx";
import {
    choiceProblem, evidenceByKey, initialChoices, keyOf, splitByProblems, statusOf, timesLogged, type Choice,
} from "./importSession.ts";

type Step = "file" | "exercises" | "review" | "importing" | "done";
const STEPS: { id: Step; label: string }[] = [
    { id: "file", label: "File" },
    { id: "exercises", label: "Exercises" },
    { id: "review", label: "Review" },
];

const plural = (n: number, w: string) => `${n} ${w}${n === 1 ? "" : "s"}`;
const fmtDate = (d: Date) => d.toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" });

interface Done {
    result: ImportResult;
    templateIds: string[];
    exerciseIds: string[];
    undone: boolean;
}

function ImportPage() {
    const navigate = useNavigate();
    const [step, setStep] = useState<Step>("file");
    const [error, setError] = useState<string | null>(null);

    // What's already in the account, for matching and duplicate checks.
    const [catalog, setCatalog] = useState<Exercise[]>([]);
    const [logs, setLogs] = useState<LoggedWorkout[]>([]);
    const [templates, setTemplates] = useState<WorkoutTemplate[]>([]);
    useEffect(() => {
        Promise.all([searchExercises(), getLogs(), getTemplates()])
            .then(([c, l, t]) => { setCatalog(c); setLogs(l); setTemplates(t); })
            .catch(() => setError("Couldn't load your account. Check your connection and reload the page."));
    }, []);

    const [file, setFile] = useState<File | null>(null);
    const [parsed, setParsed] = useState<ParsedFile | null>(null);
    const [dateConfirmed, setDateConfirmed] = useState(false);
    const [choices, setChoices] = useState<Map<string, Choice>>(new Map());
    const [withTemplates, setWithTemplates] = useState(true);
    const [progress, setProgress] = useState("");
    const [done, setDone] = useState<Done | null>(null);

    const plan = useMemo(() => (parsed ? planImport(parsed, catalog, logs, templates) : null), [parsed, catalog, logs, templates]);
    const { ok: importable, rejected } = useMemo(() => splitByProblems(plan?.newWorkouts ?? []), [plan]);
    const usedTemplates = useMemo(() => (withTemplates ? plan?.newTemplates ?? [] : []), [withTemplates, plan]);
    const evidence = useMemo(() => evidenceByKey(importable, usedTemplates), [importable, usedTemplates]);
    const keys = useMemo(() => [...evidence.keys()], [evidence]);
    const logged = useMemo(() => timesLogged(logs), [logs]);

    async function read(f: File, dateOrder?: DateOrder) {
        setError(null);
        try {
            const p = await parseImportFile(f, { dateOrder });
            if (p.workouts.length === 0 && p.templates.length === 0) throw new ImportFileError("No workouts or templates found in this file.");
            const pl = planImport(p, catalog, logs, templates);
            setFile(f);
            setParsed(p);
            setChoices(initialChoices(pl, catalog));
            setDateConfirmed(!!dateOrder || !p.dateOrder.ambiguous);
            if (dateOrder || !p.dateOrder.ambiguous) setStep("exercises");
        } catch (e) {
            setError(e instanceof ImportFileError ? e.message : e instanceof Error && e.message ? e.message : "Couldn't read this file.");
        }
    }

    function reset() {
        setFile(null); setParsed(null); setChoices(new Map()); setDone(null); setError(null); setStep("file");
    }

    async function run() {
        if (!parsed || !plan) return;
        setStep("importing");
        setError(null);
        const exerciseIds: string[] = [];
        const templateIds: string[] = [];
        try {
            setProgress("Adding new exercises");
            const ids = new Map<string, string>();
            for (const key of keys) {
                const d = choices.get(key)!.decision;
                if (d.kind === "existing") { ids.set(key, d.exerciseId!); continue; }
                const made = await createCustomExercise({ name: d.name.trim(), description: `Imported from ${parsed.fileName}`, type: d.type, targetMuscles: d.muscles });
                ids.set(key, made.id);
                exerciseIds.push(made.id);
            }
            const idFor = (ref: ParsedExerciseRef) => ids.get(keyOf(ref.name))!;

            setProgress(`Importing ${plural(importable.length, "workout")}`);
            const result = importable.length
                ? await importLogs(toImportBodies(importable, idFor))
                : { imported: 0, skipped: 0, failures: [], importedIds: [] };

            if (usedTemplates.length) {
                setProgress("Adding templates");
                for (const t of usedTemplates) templateIds.push(await createTemplate(t.name, toTemplateExercises(t, idFor)));
            }
            setDone({ result, templateIds, exerciseIds, undone: false });
            setStep("done");
        } catch {
            setError("The import was interrupted. Try again: workouts and exercises that were already saved are skipped or matched by name.");
            setStep("review");
        }
    }

    async function undo() {
        if (!done) return;
        setStep("importing");
        setProgress("Removing the imported workouts");
        try {
            if (done.result.importedIds.length) await undoImport(done.result.importedIds);
            for (const id of done.templateIds) await deleteTemplate(id);
            for (const id of done.exerciseIds) await deleteCustomExercise(id);
            setDone({ ...done, undone: true });
        } catch {
            setError("Couldn't undo everything. Try again; what was already removed stays removed.");
        }
        setStep("done");
    }

    const stepIndex = STEPS.findIndex(s => s.id === step);

    return (
        <div className="imp">
            <HeaderComponent
                title="Import workouts"
                subtitle="Bring your training history into Bloom"
                action={<Link className="imp-btn ghost" to="/docs/import" target="_blank"><BookOpen size={15} /> Import format</Link>}
            />

            <div className="imp-body">
                {step !== "done" && step !== "importing" && (
                    <ol className="imp-stepper" aria-label="Steps">
                        {STEPS.map((s, i) => (
                            <li key={s.id} className={i < stepIndex ? "past" : i === stepIndex ? "current" : ""} aria-current={i === stepIndex ? "step" : undefined}>
                                <span className="imp-step-dot">{i < stepIndex ? <Check size={12} /> : i + 1}</span>
                                {s.label}
                            </li>
                        ))}
                        {parsed && <li className="imp-stepper-file"><FileText size={14} /> {parsed.fileName}</li>}
                    </ol>
                )}

                {error && <div className="error-banner">{error}</div>}

                {step === "file" && (
                    <FileStep
                        ready={catalog.length > 0}
                        parsed={parsed && !dateConfirmed ? parsed : null}
                        onFile={f => read(f)}
                        onDateOrder={order => file && read(file, order)}
                        onCancel={reset}
                    />
                )}

                {step === "exercises" && plan && (
                    keys.length === 0 ? (
                        <div className="imp-card imp-center">
                            <p>{plan.newWorkouts.length === 0 ? "Everything in this file is already in your account." : "No exercises to match."}</p>
                            <div className="imp-row">
                                <button className="imp-btn ghost" onClick={reset}><ArrowLeft size={15} /> Choose another file</button>
                                {plan.newWorkouts.length > 0 && <button className="imp-btn primary" onClick={() => setStep("review")}>Review import <ArrowRight size={15} /></button>}
                            </div>
                        </div>
                    ) : (
                        <ExercisesStep
                            keys={keys}
                            evidence={evidence}
                            choices={choices}
                            catalog={catalog}
                            logged={logged}
                            onChoose={(k, c) => setChoices(prev => new Map(prev).set(k, c))}
                            onBulk={updates => setChoices(prev => { const n = new Map(prev); for (const [k, c] of updates) n.set(k, c); return n; })}
                            onBack={reset}
                            onNext={() => setStep("review")}
                        />
                    )
                )}

                {step === "review" && plan && parsed && (
                    <ReviewStep
                        parsed={parsed}
                        importable={importable}
                        rejected={rejected}
                        duplicates={plan.duplicateWorkouts}
                        templates={plan.newTemplates.length}
                        existingTemplates={plan.existingTemplates}
                        withTemplates={withTemplates}
                        setWithTemplates={setWithTemplates}
                        keys={keys}
                        choices={choices}
                        catalog={catalog}
                        problems={keys.filter(k => choiceProblem(choices.get(k), new Set(catalog.map(e => e.name.toLowerCase())), new Set())).length}
                        onBack={() => setStep("exercises")}
                        onImport={run}
                    />
                )}

                {step === "importing" && (
                    <div className="imp-card imp-center" role="status">
                        <Loader2 size={28} className="imp-spin" />
                        <p>{progress}…</p>
                        <p className="imp-muted small">Keep this page open.</p>
                    </div>
                )}

                {step === "done" && done && (
                    <DoneStep done={done} onUndo={undo} onAgain={reset} onOpen={path => navigate(path)} />
                )}
            </div>
        </div>
    );
}

// ── Step 1: file ──────────────────────────────────────────────────────────────

function FileStep({ ready, parsed, onFile, onDateOrder, onCancel }: {
    ready: boolean;
    parsed: ParsedFile | null;
    onFile: (f: File) => void;
    onDateOrder: (o: DateOrder) => void;
    onCancel: () => void;
}) {
    const input = useRef<HTMLInputElement>(null);
    const [dragging, setDragging] = useState(false);

    if (parsed) {
        // Dates like 02/11/2026 could be either order and nothing in the file decides it.
        const sample = parsed.dateOrder.sample ?? "02/11/2026";
        const [a, b, y] = sample.split(/[/.-]/).map(Number);
        const asDate = (d: number, m: number) => fmtDate(new Date(y, m - 1, d));
        return (
            <div className="imp-card imp-question">
                <Calendar size={22} className="imp-question-icon" />
                <div>
                    <h2>How are dates written in this file?</h2>
                    <p className="imp-muted">Every date in <b>{parsed.fileName}</b> would make sense either way, so tell Bloom how to read <code>{sample}</code>.</p>
                    <div className="imp-options">
                        <button type="button" className="imp-option" onClick={() => onDateOrder("dmy")}>
                            <b>{asDate(a, b)}</b><span>Day first (DD/MM/YYYY)</span>
                        </button>
                        <button type="button" className="imp-option" onClick={() => onDateOrder("mdy")}>
                            <b>{asDate(b, a)}</b><span>Month first (MM/DD/YYYY)</span>
                        </button>
                    </div>
                    <button type="button" className="imp-btn ghost" onClick={onCancel}><ArrowLeft size={15} /> Choose another file</button>
                </div>
            </div>
        );
    }

    return (
        <div className="imp-file">
            <div
                className={`imp-drop${dragging ? " dragging" : ""}`}
                onDragOver={e => { e.preventDefault(); setDragging(true); }}
                onDragLeave={() => setDragging(false)}
                onDrop={e => { e.preventDefault(); setDragging(false); if (e.dataTransfer.files[0]) onFile(e.dataTransfer.files[0]); }}
            >
                <span className="imp-drop-icon"><Upload size={26} /></span>
                <h2>Drop your file here</h2>
                <p className="imp-muted">Your own spreadsheet, another app's export, or a Bloom backup.</p>
                <button type="button" className="imp-btn primary" disabled={!ready} onClick={() => input.current?.click()}>
                    {ready ? "Choose file" : "Loading your account…"}
                </button>
                <input ref={input} type="file" accept=".csv,.xlsx,.json" hidden onChange={e => { if (e.target.files?.[0]) onFile(e.target.files[0]); e.target.value = ""; }} />
            </div>

            <div className="imp-formats">
                <div className="imp-format">
                    <FileText size={18} />
                    <b>CSV</b>
                    <p>One row per set. Only <b>Date</b>, <b>Workout</b> and <b>Exercise</b> are required; use your own exercise names.</p>
                </div>
                <div className="imp-format">
                    <FileSpreadsheet size={18} />
                    <b>Excel</b>
                    <p>The same columns on a sheet named <b>Sets</b>, so you can keep working in your spreadsheet.</p>
                </div>
                <div className="imp-format">
                    <Braces size={18} />
                    <b>JSON</b>
                    <p>A Bloom backup. Brings templates, set types and GPS tracks too.</p>
                </div>
            </div>
            <p className="imp-file-links">
                <button type="button" className="imp-link" onClick={() => downloadBlob(exampleBlob("csv"), "bloom-import-example.csv")}><Download size={14} /> Example CSV</button>
                <Link className="imp-link" to="/docs/import" target="_blank"><BookOpen size={14} /> Every column and value</Link>
            </p>
        </div>
    );
}

// ── Step 3: review ────────────────────────────────────────────────────────────

function ReviewStep({ parsed, importable, rejected, duplicates, templates, existingTemplates, withTemplates, setWithTemplates, keys, choices, catalog, problems, onBack, onImport }: {
    parsed: ParsedFile;
    importable: ParsedWorkout[];
    rejected: { workout: ParsedWorkout; problems: string[] }[];
    duplicates: number;
    templates: number;
    existingTemplates: number;
    withTemplates: boolean;
    setWithTemplates: (v: boolean) => void;
    keys: string[];
    choices: Map<string, Choice>;
    catalog: Exercise[];
    problems: number;
    onBack: () => void;
    onImport: () => void;
}) {
    const byId = useMemo(() => new Map(catalog.map(e => [e.id, e])), [catalog]);
    const sets = importable.reduce((n, w) => n + w.exercises.reduce((m, e) => m + e.sets.length, 0), 0);
    const newExercises = keys.filter(k => choices.get(k)?.decision.kind === "create");
    const unchecked = keys.filter(k => statusOf(choices.get(k)) === "check").length;
    const { from, to } = useMemo(() => {
        const times = importable.map(w => w.loggedAt.getTime());
        return times.length ? { from: new Date(Math.min(...times)), to: new Date(Math.max(...times)) } : { from: null, to: null };
    }, [importable]);
    const nameFor = (ref: ParsedExerciseRef) => {
        const d = choices.get(keyOf(ref.name))?.decision;
        return d?.kind === "existing" ? byId.get(d.exerciseId ?? "")?.name ?? ref.name : d?.name ?? ref.name;
    };

    // Workouts per month across the range, as a small bar strip.
    const months = useMemo(() => {
        if (!from || !to) return [];
        const out: { key: string; label: string; count: number }[] = [];
        for (let d = new Date(from.getFullYear(), from.getMonth(), 1); d <= to; d = new Date(d.getFullYear(), d.getMonth() + 1, 1)) {
            out.push({ key: `${d.getFullYear()}-${d.getMonth()}`, label: d.toLocaleDateString("en-GB", { month: "short", year: "2-digit" }), count: 0 });
        }
        for (const w of importable) {
            const m = out.find(x => x.key === `${w.loggedAt.getFullYear()}-${w.loggedAt.getMonth()}`);
            if (m) m.count++;
        }
        return out;
    }, [importable, from, to]);
    const peak = Math.max(1, ...months.map(m => m.count));
    const preview = [...importable].sort((a, b) => b.loggedAt.getTime() - a.loggedAt.getTime())[0];

    return (
        <div className="imp-step">
            <div className="imp-review">
                <div className="imp-review-main">
                    <div className="imp-metrics">
                        <div className="imp-metric"><span>New workouts</span><b>{importable.length}</b>{from && to && <small>{fmtDate(from)} to {fmtDate(to)}</small>}</div>
                        <div className="imp-metric"><span>Sets</span><b>{sets.toLocaleString("en-GB")}</b><small>across {plural(keys.length, "exercise")}</small></div>
                        <div className="imp-metric"><span>New exercises</span><b>{newExercises.length}</b><small>{keys.length - newExercises.length} matched to yours</small></div>
                        <div className="imp-metric"><span>Already in Bloom</span><b>{duplicates}</b><small>skipped</small></div>
                    </div>

                    {months.length > 1 && (
                        <div className="imp-card">
                            <p className="imp-eyebrow">Workouts per month</p>
                            <div className="imp-months" role="img" aria-label="Workouts per month in this import">
                                {months.map(m => (
                                    <div key={m.key} className="imp-month" title={`${m.label}: ${plural(m.count, "workout")}`}>
                                        <span style={{ height: `${(m.count / peak) * 100}%` }} />
                                    </div>
                                ))}
                            </div>
                            <div className="imp-months-axis"><span>{months[0].label}</span><span>{months[months.length - 1].label}</span></div>
                        </div>
                    )}

                    {(rejected.length > 0 || parsed.warnings.length > 0) && (
                        <div className="imp-card imp-warn">
                            <p className="imp-warn-title"><AlertTriangle size={15} /> Not imported</p>
                            {rejected.length > 0 && (
                                <details>
                                    <summary>{plural(rejected.length, "workout")} with sets Bloom can't store</summary>
                                    <ul>{rejected.slice(0, 30).map((r, i) => <li key={i}><b>{r.workout.name}</b> ({fmtDate(r.workout.loggedAt)}): {r.problems.join("; ")}</li>)}</ul>
                                </details>
                            )}
                            {parsed.warnings.length > 0 && (
                                <details>
                                    <summary>{plural(parsed.warnings.length, "row")} couldn't be read</summary>
                                    <ul>{parsed.warnings.slice(0, 30).map((w, i) => <li key={i}>{w}</li>)}</ul>
                                </details>
                            )}
                        </div>
                    )}

                    {templates + existingTemplates > 0 && (
                        <label className="imp-card imp-check">
                            <input type="checkbox" checked={withTemplates} onChange={e => setWithTemplates(e.target.checked)} />
                            <span><b>Also add {plural(templates, "template")}</b>{existingTemplates > 0 && <small> · {existingTemplates} you already have are left as they are</small>}</span>
                        </label>
                    )}
                </div>

                {preview && (
                    <aside className="imp-card imp-preview">
                        <p className="imp-eyebrow">How it will look · most recent workout</p>
                        <h3>{preview.name}</h3>
                        <p className="imp-muted small">{preview.loggedAt.toLocaleDateString("en-GB", { weekday: "long", day: "numeric", month: "long", year: "numeric" })}, {preview.loggedAt.toLocaleTimeString("en-GB", { hour: "2-digit", minute: "2-digit" })}</p>
                        <ul className="imp-preview-list">
                            {preview.exercises.map((e, i) => {
                                const name = nameFor(e.ref);
                                return (
                                    <li key={i}>
                                        <div className="imp-preview-ex">
                                            <b>{name}</b>
                                            {name.toLowerCase() !== e.ref.name.trim().toLowerCase() && <small>from “{e.ref.name}”</small>}
                                        </div>
                                        <div className="imp-preview-sets">
                                            {e.sets.map((s, j) => (
                                                <span key={j} className={s.marker === "WarmUp" ? "warm" : ""}>
                                                    {s.type === "Cardio" ? `${s.distance ?? "?"} ${String(s.distanceUnit ?? "km").toLowerCase()} · ${(s.duration ?? "").replace(/^00:/, "")}` : `${s.weight ?? "?"} × ${s.reps ?? "?"}`}
                                                </span>
                                            ))}
                                        </div>
                                    </li>
                                );
                            })}
                        </ul>
                    </aside>
                )}
            </div>

            <div className="imp-footer" role="group" aria-label="Actions">
                <button type="button" className="imp-btn ghost" onClick={onBack}><ArrowLeft size={15} /> Exercises</button>
                <div className="imp-footer-right">
                    {unchecked > 0 && <span className="imp-muted">{plural(unchecked, "exercise")} not checked: imported as suggested</span>}
                    <button type="button" className="imp-btn primary" disabled={importable.length === 0 || problems > 0} onClick={onImport}>
                        <Upload size={15} /> Import {plural(importable.length, "workout")}
                    </button>
                </div>
            </div>
        </div>
    );
}

// ── Done ──────────────────────────────────────────────────────────────────────

function DoneStep({ done, onUndo, onAgain, onOpen }: { done: Done; onUndo: () => void; onAgain: () => void; onOpen: (path: string) => void }) {
    const [confirming, setConfirming] = useState(false);
    const { result } = done;

    if (done.undone) {
        return (
            <div className="imp-card imp-center imp-done">
                <span className="imp-done-icon muted"><RotateCcw size={24} /></span>
                <h2>Import undone</h2>
                <p className="imp-muted">The {plural(result.imported, "workout")}{done.templateIds.length ? `, ${plural(done.templateIds.length, "template")}` : ""}{done.exerciseIds.length ? ` and ${plural(done.exerciseIds.length, "new exercise")}` : ""} from this import were removed.</p>
                <button className="imp-btn primary" onClick={onAgain}>Import a file</button>
            </div>
        );
    }

    return (
        <div className="imp-card imp-center imp-done">
            <span className="imp-done-icon"><Check size={26} /></span>
            <h2>{plural(result.imported, "workout")} imported</h2>
            <p className="imp-muted">
                {[
                    result.skipped ? `${result.skipped} already in Bloom were skipped` : null,
                    done.exerciseIds.length ? `${plural(done.exerciseIds.length, "new exercise")} added` : null,
                    done.templateIds.length ? `${plural(done.templateIds.length, "template")} added` : null,
                ].filter(Boolean).join(" · ") || "Your history is in your logbook."}
            </p>
            {result.failures.length > 0 && (
                <details className="imp-warn-inline">
                    <summary><AlertTriangle size={14} /> {plural(result.failures.length, "workout")} couldn't be imported</summary>
                    <ul>{result.failures.slice(0, 30).map(f => <li key={f.index}>{f.name} ({fmtDate(new Date(f.loggedAt))}): {f.reason}</li>)}</ul>
                </details>
            )}
            <div className="imp-row">
                <button className="imp-btn primary" onClick={() => onOpen("/logbook")}>Open logbook</button>
                <button className="imp-btn secondary" onClick={() => onOpen("/insights")}>See your progress</button>
            </div>
            <div className="imp-undo">
                {confirming ? (
                    <>
                        <span>Remove everything this import added?</span>
                        <button className="imp-btn danger" onClick={onUndo}><Undo2 size={14} /> Yes, undo import</button>
                        <button className="imp-btn ghost" onClick={() => setConfirming(false)}>Keep it</button>
                    </>
                ) : (
                    <button className="imp-link" onClick={() => setConfirming(true)}><Undo2 size={14} /> Undo this import</button>
                )}
            </div>
        </div>
    );
}

export default ImportPage;
