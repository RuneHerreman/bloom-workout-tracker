import { useEffect, useRef, useState, type ReactNode } from "react";
import { useNavigate } from "react-router-dom";
import { ArrowLeft, Check, Copy, Download } from "lucide-react";
import "../../assets/css/docs.css";
import HeaderComponent from "../../components/general/HeaderComponent.tsx";
import { searchExercises, peekExercises, type Exercise } from "../exercises/api.ts";
import ExerciseList from "./ExerciseList.tsx";
import { downloadBlob } from "../export/exportData.ts";
import { exampleBlob, exampleCsvGrid, exampleCsvText, exampleJsonText } from "../export/importData.ts";

// ── Field reference data ──────────────────────────────────────────────────────

interface Field { name: string; required?: boolean; type: string; values?: ReactNode; notes?: ReactNode }

const CSV_FIELDS: Field[] = [
    { name: "Date", required: true, type: "date", values: <><code>2026-11-02</code>, <code>02/11/2026</code>, <code>02.11.2026</code>, or an Excel date</>, notes: "Day the workout happened (your local time). For dates with slashes Bloom works out day/month order from the file; if every date could be either, it asks." },
    { name: "Time", type: "time", values: <><code>18:30</code> (24 h)</>, notes: "Start time. Left out: 12:00. Used together with Date and Workout to group rows." },
    { name: "Workout", required: true, type: "text", values: "1–200 characters", notes: "Rows with the same Date, Time and Workout form one workout." },
    { name: "Exercise", required: true, type: "text", values: "An exercise name", notes: <>Use your own names. Exact matches (ignoring case) are linked automatically; for the rest you choose an exercise or add a new one in the <a href="#matching">matching step</a>.</> },
    { name: "Exercise type", type: "enum", values: <>Strength, Cardio, Plyometric</>, notes: "Left out: Cardio when the row has a distance or duration, otherwise Strength. Also used when the exercise has to be created." },
    { name: "Target muscles", type: "list", values: "Comma separated, e.g. Chest, Triceps", notes: "Only used when the exercise has to be created." },
    { name: "Set", type: "number or W", values: <>1, 2, 3 … or <code>W</code></>, notes: <>Informational: sets are stored in the order of the rows. <code>W</code> marks a warm-up when Set type is empty.</> },
    { name: "Set type", type: "enum", values: <>Working set (or empty), Warm-up (<code>W</code>), Drop set (<code>D</code>), Failure (<code>F</code>)</>, notes: "Any capitalisation. Warm-ups don't count as working sets in Insights." },
    { name: "Reps", type: "integer", values: "1 or more", notes: "Required on Strength and Plyometric sets." },
    { name: "Weight", type: "decimal", values: <>0 or more, <code>82.5</code> or <code>82,5</code></>, notes: "Required on Strength and Plyometric sets. Use 0 for bodyweight." },
    { name: "Weight unit", type: "enum", values: "Kg, Lbs", notes: "Left out: Kg. Weights are stored in the unit given." },
    { name: "RIR", type: "integer", values: "0–10", notes: "Reps in reserve. Optional." },
    { name: "Distance", type: "decimal", values: "Greater than 0", notes: "Required on Cardio sets." },
    { name: "Distance unit", type: "enum", values: "Km, Miles", notes: "Left out: Km." },
    { name: "Duration", type: "time span", values: <><code>hh:mm:ss</code> or <code>mm:ss</code></>, notes: <>Required on Cardio sets. If empty, <b>Duration (s)</b> is used.</> },
    { name: "Duration (s)", type: "integer", values: "Seconds", notes: "Fallback for Duration." },
    { name: "Gear", type: "list", values: "Comma separated", notes: "Gear used for the exercise, e.g. shoes, belt." },
    { name: "Exercise note", type: "text", values: "Up to 2000 characters", notes: "Taken from the exercise's first row." },
    { name: "Workout note", type: "text", values: "Up to 2000 characters", notes: "Taken from the workout's first row." },
];

const WORKOUT_FIELDS: Field[] = [
    { name: "id", type: "string", values: "Up to 100 characters", notes: "Optional. If you already have a workout with this id, or this id came in with an earlier import, it is skipped." },
    { name: "loggedAt", required: true, type: "date-time", values: <>ISO 8601: <code>2026-11-02T18:30:00</code></>, notes: <>Without a zone it is read as your local time; <code>…Z</code> or <code>+01:00</code> is exact.</> },
    { name: "name", required: true, type: "string", values: "1–200 characters" },
    { name: "note", type: "string | null", values: "Up to 2000 characters" },
    { name: "exercises", required: true, type: "array", values: "Exercise objects, in order" },
];

const EXERCISE_FIELDS: Field[] = [
    { name: "exerciseName", required: true, type: "string", notes: "Your name for the exercise; matched by name like in CSV." },
    { name: "exerciseId", type: "GUID", notes: "Optional, never needed. Bloom backups include it; when it exists in your account it's used instead of the name." },
    { name: "exerciseType", type: "enum", values: "Strength | Cardio | Plyometric", notes: "Used when the exercise has to be created." },
    { name: "note", type: "string | null", values: "Up to 2000 characters" },
    { name: "gear", type: "string[]", values: "Names" },
    { name: "gpx", type: "string", notes: "Optional GPX track as XML text." },
    { name: "sets", required: true, type: "array", values: "Set objects, in order" },
];

const SET_FIELDS: Field[] = [
    { name: "type", required: true, type: "enum", values: "Strength | Cardio | Plyometric" },
    { name: "order", type: "integer", values: "0 or more", notes: "Left out: position in the array." },
    { name: "marker", type: "enum | null", values: "WarmUp | DropSet | Failure | null", notes: "null is a normal working set." },
    { name: "reps", type: "integer", values: "1 or more", notes: "Required for Strength and Plyometric." },
    { name: "weight", type: "number", values: "0 or more", notes: "Required for Strength and Plyometric. 0 = bodyweight." },
    { name: "weightUnit", type: "enum", values: "Kg | Lbs" },
    { name: "rir", type: "integer | null", values: "0–10" },
    { name: "distance", type: "number", values: "Greater than 0", notes: "Required for Cardio." },
    { name: "distanceUnit", type: "enum", values: "Km | Miles" },
    { name: "duration", type: "string", values: "hh:mm:ss", notes: "Required for Cardio." },
];

const TEMPLATE_FIELDS: Field[] = [
    { name: "name", required: true, type: "string", notes: "A template whose name you already have (ignoring case) is skipped." },
    { name: "exercises[].exerciseName / exerciseId / note / gear", type: "as above" },
    { name: "exercises[].sets[]", type: "array", notes: <>Planned sets: <code>type</code>, <code>order</code>, <code>reps</code>, <code>duration</code>, <code>distance</code>, <code>distanceUnit</code>.</> },
];

const CUSTOM_FIELDS: Field[] = [
    { name: "id", type: "GUID", notes: "The exercise's id in the file, so workouts can point at it." },
    { name: "name", required: true, type: "string" },
    { name: "description", type: "string", notes: "Left out: \"Imported custom exercise\"." },
    { name: "type", required: true, type: "enum", values: "Strength | Cardio | Plyometric" },
    { name: "targetMuscles", type: "string[]" },
];

// ── Small pieces ──────────────────────────────────────────────────────────────

function FieldTable({ fields }: { fields: Field[] }) {
    return (
        <div className="docs-table-wrap">
            <table className="docs-table">
                <thead><tr><th>Field</th><th>Type</th><th>Accepted values</th><th>Notes</th></tr></thead>
                <tbody>
                    {fields.map(f => (
                        <tr key={f.name}>
                            <td><code>{f.name}</code>{f.required && <span className="docs-req" title="Required"> required</span>}</td>
                            <td className="docs-type">{f.type}</td>
                            <td>{f.values ?? ""}</td>
                            <td>{f.notes ?? ""}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}

function CopyButton({ text }: { text: string }) {
    const [done, setDone] = useState(false);
    return (
        <button
            type="button"
            className="docs-btn"
            onClick={() => { navigator.clipboard?.writeText(text).then(() => { setDone(true); setTimeout(() => setDone(false), 1800); }); }}
        >
            {done ? <Check size={13} /> : <Copy size={13} />} {done ? "Copied" : "Copy"}
        </button>
    );
}

/** A code block; the copy button sits in its own bar so it never covers the text. */
function Code({ text, label, copy = true }: { text: string; label: string; copy?: boolean }) {
    return (
        <div className="docs-code">
            {copy && <div className="docs-code-bar"><CopyButton text={text} /></div>}
            <pre aria-label={label}>{text}</pre>
        </div>
    );
}

const SECTIONS = [
    ["overview", "Overview"],
    ["csv", "CSV and Excel"],
    ["json", "JSON"],
    ["values", "Allowed values"],
    ["matching", "Matching exercises"],
    ["exercises", "Your exercises"],
    ["duplicates", "Duplicates"],
    ["errors", "Errors and limits"],
    ["api", "API"],
    ["examples", "Examples"],
] as const;

// ── Page ──────────────────────────────────────────────────────────────────────

/** The element that scrolls the page: the app's main area, or the window as a fallback. */
function scrollerOf(el: Element | null): HTMLElement {
    for (let p = el?.parentElement ?? null; p; p = p.parentElement) {
        const y = getComputedStyle(p).overflowY;
        if ((y === "auto" || y === "scroll") && p.scrollHeight > p.clientHeight) return p;
    }
    return document.scrollingElement as HTMLElement;
}

const OFFSET = 16; // gap left above a section heading after jumping to it

/** Holds the highlight on a clicked section for a moment, while the smooth scroll travels past others. */
function lockFor(ref: { current: number }, ms: number) {
    ref.current = performance.now() + ms;
}

/** Scrolls the page so a section's heading sits just below the top (Overview: the very top). */
function scrollToSection(root: Element | null, id: string, smooth: boolean) {
    const scroller = scrollerOf(root);
    const el = document.getElementById(id);
    if (!el) return;
    const viewTop = scroller === document.scrollingElement ? 0 : scroller.getBoundingClientRect().top;
    const top = id === SECTIONS[0][0] ? 0 : scroller.scrollTop + el.getBoundingClientRect().top - viewTop - OFFSET;
    scroller.scrollTo({ top, behavior: smooth && !matchMedia("(prefers-reduced-motion: reduce)").matches ? "smooth" : "auto" });
}

function ImportDocsPage() {
    const navigate = useNavigate();
    const [exercises, setExercises] = useState<Exercise[]>(() => peekExercises() ?? []);
    const muscles = muscleList(exercises);
    const [active, setActive] = useState<string>(SECTIONS[0][0]);
    const rootRef = useRef<HTMLDivElement>(null);
    // After a click the clicked section stays highlighted while the smooth scroll travels.
    const lockedUntil = useRef(0);

    useEffect(() => {
        searchExercises().then(setExercises).catch(() => null);
    }, []);

    // Highlight the section being read: the last one whose heading is near the top of the view.
    useEffect(() => {
        const scroller = scrollerOf(rootRef.current);
        const target: HTMLElement | Window = scroller === document.scrollingElement ? window : scroller;
        const update = () => {
            if (performance.now() < lockedUntil.current) return;
            const top = scroller === document.scrollingElement ? 0 : scroller.getBoundingClientRect().top;
            const line = top + 120;
            let current: string = SECTIONS[0][0];
            for (const [id] of SECTIONS) {
                const el = document.getElementById(id);
                if (el && el.getBoundingClientRect().top <= line) current = id;
            }
            // At the very bottom the last sections can't reach the line; pick the last one.
            if (scroller.scrollTop + scroller.clientHeight >= scroller.scrollHeight - 2) current = SECTIONS[SECTIONS.length - 1][0];
            setActive(current);
        };
        update();
        target.addEventListener("scroll", update, { passive: true });
        // Opened with a #section link: go there once the page has laid out.
        const hash = window.location.hash.slice(1);
        if (hash && SECTIONS.some(([id]) => id === hash)) requestAnimationFrame(() => scrollToSection(rootRef.current, hash, false));
        return () => target.removeEventListener("scroll", update);
    }, []);

    function jumpTo(id: string) {
        lockFor(lockedUntil, 900);
        scrollToSection(rootRef.current, id, true);
        setActive(id);
        // Keep the link shareable without adding a history entry per click (Back should leave the page).
        history.replaceState(history.state, "", `#${id}`);
    }

    // Opened in a new tab from the import dialog there's nothing to go back to.
    const goBack = () => ((history.state?.idx ?? 0) > 0 ? navigate(-1) : navigate("/logbook"));

    return (
        <div className="docs" ref={rootRef}>
            <HeaderComponent
                title="Import format"
                subtitle="What Bloom accepts when you import workouts, templates and exercises"
                action={<button type="button" className="docs-btn" onClick={goBack}><ArrowLeft size={13} /> Back</button>}
            />

            <div className="docs-body">
                <nav className="docs-toc" aria-label="On this page">
                    <span className="docs-toc-title">On this page</span>
                    {SECTIONS.map(([id, label]) => (
                        <a
                            key={id}
                            href={`#${id}`}
                            className={active === id ? "active" : ""}
                            aria-current={active === id ? "location" : undefined}
                            onClick={e => { e.preventDefault(); jumpTo(id); }}
                        >
                            {label}
                        </a>
                    ))}
                </nav>

                <article className="docs-article">
                    <section id="overview">
                        <h2>Overview</h2>
                        <p>
                            Import accepts three kinds of file. The easiest way to get one is <b>Export data</b> in your profile or logbook; you can also write
                            one yourself, for example to bring in history from a spreadsheet or another app.
                        </p>
                        <div className="docs-table-wrap">
                            <table className="docs-table">
                                <thead><tr><th>File</th><th>Contains</th><th>Best for</th></tr></thead>
                                <tbody>
                                    <tr><td><b>JSON</b> <code>.json</code></td><td>Workouts, templates, custom exercises, set types, GPS tracks</td><td>Backups and moving between accounts</td></tr>
                                    <tr><td><b>Excel</b> <code>.xlsx</code></td><td>Workouts, one row per set, on a sheet named <code>Sets</code></td><td>Editing in a spreadsheet</td></tr>
                                    <tr><td><b>CSV</b> <code>.csv</code></td><td>Workouts, one row per set</td><td>Scripts and other apps</td></tr>
                                </tbody>
                            </table>
                        </div>
                        <p>After choosing a file you <a href="#matching">match its exercise names</a> to yours, then see a summary of what will be added before anything is saved.</p>
                    </section>

                    <section id="csv">
                        <h2>CSV and Excel</h2>
                        <p>
                            <b>One row per set</b>, with a header row. Column names are case-insensitive and may be in any order; columns that aren't listed
                            (such as <code>Volume (kg)</code> in an export) are ignored. Excel files are read from the sheet named <code>Sets</code>, or the first sheet if there is none.
                            CSV must be comma separated and UTF-8.
                        </p>
                        <FieldTable fields={CSV_FIELDS} />
                        <p className="docs-note">
                            Cardio sets need <b>Distance</b> and <b>Duration</b>; Strength and Plyometric sets need <b>Reps</b> and <b>Weight</b>. A workout containing a set that
                            doesn't meet this is reported as failed; the rest of the file still imports.
                        </p>
                        <p className="docs-note">Text starting with <code>=</code>, <code>+</code>, <code>-</code> or <code>@</code> is exported with a leading <code>'</code> so spreadsheets don't run it as a formula; the importer removes it again.</p>
                    </section>

                    <section id="json">
                        <h2>JSON</h2>
                        <p>
                            A JSON backup is an object with <code>workouts</code>, and optionally <code>templates</code> and <code>customExercises</code>. Fields marked
                            optional can be left out. Unknown fields (like <code>app</code>, <code>schemaVersion</code>, <code>exportedAt</code>) are ignored.
                        </p>
                        <Code label="JSON structure" text={`{
  "workouts": [ { …workout… } ],
  "templates": [ { …template… } ],
  "customExercises": [ { …exercise… } ]
}`} />
                        <h3>Workout</h3>
                        <FieldTable fields={WORKOUT_FIELDS} />
                        <h3>Exercise (inside a workout)</h3>
                        <FieldTable fields={EXERCISE_FIELDS} />
                        <h3>Set</h3>
                        <FieldTable fields={SET_FIELDS} />
                        <h3>Template</h3>
                        <FieldTable fields={TEMPLATE_FIELDS} />
                        <h3>Custom exercise</h3>
                        <FieldTable fields={CUSTOM_FIELDS} />
                    </section>

                    <section id="values">
                        <h2>Allowed values</h2>
                        <div className="docs-enums">
                            <div>
                                <h3>Exercise / set type</h3>
                                <ul>
                                    <li><code>Strength</code> reps × weight</li>
                                    <li><code>Cardio</code> distance and duration</li>
                                    <li><code>Plyometric</code> reps × weight</li>
                                </ul>
                            </div>
                            <div>
                                <h3>Set marker</h3>
                                <ul>
                                    <li><code>null</code> / empty: working set</li>
                                    <li><code>WarmUp</code> CSV: Warm-up, <code>W</code></li>
                                    <li><code>DropSet</code> CSV: Drop set, <code>D</code></li>
                                    <li><code>Failure</code> CSV: Failure, <code>F</code></li>
                                </ul>
                            </div>
                            <div>
                                <h3>Weight unit</h3>
                                <ul><li><code>Kg</code></li><li><code>Lbs</code></li></ul>
                                <h3>Distance unit</h3>
                                <ul><li><code>Km</code></li><li><code>Miles</code></li></ul>
                            </div>
                        </div>
                        <h3>Target muscles</h3>
                        <p>
                            Free text, but these are the muscle groups used by exercises in your account{muscles.length ? "" : " (loading…)"}:
                        </p>
                        {muscles.length > 0 && <p className="docs-values">{muscles.map(m => <code key={m}>{m}</code>)}</p>}
                    </section>

                    <section id="matching">
                        <h2>Matching exercises</h2>
                        <p>
                            You don't need ids or Bloom's exact names: write exercises the way you always do. After you choose a file, the import shows every
                            exercise name in it, with what it will be linked to:
                        </p>
                        <ol>
                            <li><b>Same name as one of your exercises</b> (ignoring case): linked automatically. You can still change it.</li>
                            <li>
                                <b>Anything else needs your decision.</b> Either <b>match it</b> to an exercise you have (search by name; Bloom suggests the closest one,
                                so <code>Bench Pres</code> suggests Bench Press and <code>DB incline press</code> suggests Incline Dumbbell Press), or <b>add it as new</b> with
                                a name and a type (Strength, Cardio or Plyometric).
                            </li>
                        </ol>
                        <p>Nothing is saved until you've decided for every name. Several names can be matched to the same exercise, for example <code>Bench</code> and <code>Flat bench</code>.</p>
                        <p className="docs-note">The type you pick for a new exercise is used for the exercise itself; each set keeps what the row contains (reps and weight, or distance and duration).</p>
                    </section>

                    <section id="exercises">
                        <h2>Your exercises</h2>
                        <p>
                            These are the exercises in your account: Bloom's catalog plus the ones you created. Using these names in your file means everything
                            matches automatically, but it's not required. Download the list to look names up next to your own spreadsheet.
                        </p>
                        <ExerciseList exercises={exercises} />
                    </section>

                    <section id="duplicates">
                        <h2>Duplicates</h2>
                        <p>A workout is skipped when you already have it, so importing the same file twice never doubles your history:</p>
                        <ul>
                            <li>you already have a workout with the <b>same name</b> (ignoring case) at the <b>same minute</b>; or</li>
                            <li>in JSON, its optional <code>id</code> matches one of your workouts or an earlier import.</li>
                        </ul>
                        <p>Two workouts <i>inside</i> the file are never merged in JSON. In CSV and Excel, rows with the same Date, Time and Workout are one workout.</p>
                        <h3>Templates</h3>
                        <p>Matched by name, ignoring case; ones you already have are left untouched.</p>
                    </section>

                    <section id="errors">
                        <h2>Errors and limits</h2>
                        <ul>
                            <li>At most <b>5000 workouts</b> per import.</li>
                            <li>A file that isn't a Bloom export, or has no workouts or templates, is rejected with a message before anything happens.</li>
                            <li>Rows without a date, workout or exercise (CSV/Excel), and workouts without a name or date (JSON), are skipped and listed in the preview.</li>
                            <li>A workout with an invalid set (for example a Cardio set without a distance) is reported as <i>couldn't be imported</i> with the reason; the other workouts are imported.</li>
                            <li>If the import is interrupted, run it again: what was already saved is skipped.</li>
                            <li>Changed your mind? Right after importing, <b>Undo this import</b> removes the workouts, templates and new exercises it added.</li>
                        </ul>
                    </section>

                    <section id="api">
                        <h2>API</h2>
                        <p>
                            The importer uses one endpoint, which you can also call yourself while signed in. Exercises are referenced by <code>exerciseId</code>
                            (a GUID of an exercise you can see), so look them up with <code>GET /api/exercises</code> first.
                        </p>
                        <Code label="Request" text={`POST /api/logs/import
Content-Type: application/json

{
  "workouts": [{
    "name": "Push",                       // 1–200 chars
    "loggedAt": "2026-11-02T18:30:00Z",   // ISO 8601
    "note": null,
    "sourceId": "optional-id-for-dedupe",
    "exercises": [{
      "exerciseId": "3f2b…",              // GUID
      "order": 0,
      "note": null,
      "gear": [],
      "sets": [{
        "type": "Strength", "order": 0,
        "reps": 6, "weight": 80, "weightUnit": "Kg",
        "rir": 2, "marker": null
      }]
    }]
  }]
}`} />
                        <Code label="Response" text={`200 OK
{
  "imported": 1,
  "skipped": 0,
  "failures": [
    { "index": 3, "name": "Run", "loggedAt": "2026-11-04T07:15:00Z", "reason": "Distance required for Cardio set" }
  ]
}`} />
                        <p className="docs-note">
                            <code>duration</code> is <code>hh:mm:ss</code>; <code>distanceUnit</code> is <code>Km</code> or <code>Miles</code>; <code>marker</code> is
                            <code> WarmUp</code>, <code>DropSet</code>, <code>Failure</code> (or <code>W</code>/<code>D</code>/<code>F</code>) or <code>null</code>. Unlike creating a single log,
                            sets are validated by the domain rather than by ranges, so values the app stores itself (such as 0 kg for bodyweight) are accepted. Failed workouts are listed by their position in the request.
                        </p>
                    </section>

                    <section id="examples">
                        <h2>Examples</h2>
                        <p>The same two workouts in both layouts. Importing one after the other shows the second as already imported.</p>
                        <h3>CSV <span className="docs-actions">
                            <CopyButton text={exampleCsvText()} />
                            <button type="button" className="docs-btn" onClick={() => downloadBlob(exampleBlob("csv"), "bloom-import-example.csv")}><Download size={13} /> Download</button>
                        </span></h3>
                        <div className="import-sheet" role="table" aria-label="Example CSV file, as it opens in Excel">
                            <table>
                                <tbody>
                                    {exampleCsvGrid().map((row, r) => (
                                        <tr key={r} className={r === 0 ? "head" : ""}>
                                            <th scope="row" className="gutter">{r + 1}</th>
                                            {row.map((cell, c) => <td key={c} className={c < 3 && r === 0 ? "required" : ""}>{cell}</td>)}
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                        <p className="docs-note">Raw text of the same file:</p>
                        <Code label="CSV text" text={exampleCsvText()} copy={false} />
                        <h3>JSON <span className="docs-actions">
                            <CopyButton text={exampleJsonText()} />
                            <button type="button" className="docs-btn" onClick={() => downloadBlob(exampleBlob("json"), "bloom-import-example.json")}><Download size={13} /> Download</button>
                        </span></h3>
                        <Code label="JSON example" text={exampleJsonText()} copy={false} />
                    </section>
                </article>
            </div>
        </div>
    );
}

function muscleList(exercises: { targetMuscles: string[] }[] | undefined): string[] {
    return [...new Set((exercises ?? []).flatMap(e => e.targetMuscles))].sort();
}

export default ImportDocsPage;
