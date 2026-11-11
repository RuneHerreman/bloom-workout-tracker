import { useEffect, useMemo, useState } from "react";
import { FileSpreadsheet, FileText, Braces, Download, Check } from "lucide-react";
import Overlay from "../../components/general/OverlayComponent.tsx";
import Skeleton from "../../components/general/Skeleton.tsx";
import { getLogs } from "../logbook/api.ts";
import { getBody } from "../body/api.ts";
import { getTemplates } from "../templates/api.ts";
import { searchExercises } from "../exercises/api.ts";
import type { BodyEntry, Exercise, LoggedWorkout, WorkoutTemplate } from "../../assets/js/data/apiTypes.ts";
import {
    type ExportFormat, type ExportRange, RANGE_LABELS,
    filterByRange, filterBodyByRange, buildCsv, buildWorkbook, buildJson, downloadBlob, exportFilename,
} from "./exportData.ts";

const FORMATS: { value: ExportFormat; label: string; ext: string; desc: string; icon: typeof FileText }[] = [
    { value: "xlsx", label: "Excel",  ext: ".xlsx", desc: "Sets & workout summary sheets", icon: FileSpreadsheet },
    { value: "csv",  label: "CSV",    ext: ".csv",  desc: "One row per set, works anywhere", icon: FileText },
    { value: "json", label: "JSON",   ext: ".json", desc: "Full backup incl. templates",     icon: Braces },
];

interface Data {
    logs: LoggedWorkout[];
    templates: WorkoutTemplate[];
    exercises: Record<string, Exercise>;
    body: BodyEntry[];
}

function ExportOverlay({ onClose }: { onClose: () => void }) {
    const [format, setFormat] = useState<ExportFormat>("xlsx");
    const [range, setRange] = useState<ExportRange>("all");
    const [includeGps, setIncludeGps] = useState(false);
    const [data, setData] = useState<Data | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [done, setDone] = useState(false);

    useEffect(() => {
        Promise.all([getLogs(), getTemplates(), searchExercises(), getBody().catch(() => [])])
            .then(([logs, templates, exs, body]) => setData({
                logs, templates, exercises: Object.fromEntries(exs.map(e => [e.id, e])), body,
            }))
            .catch(() => setError("Couldn't load your data. Check your connection and try again."));
    }, []);

    const selected = useMemo(() => (data ? filterByRange(data.logs, range) : []), [data, range]);
    const selectedBody = useMemo(() => (data ? filterBodyByRange(data.body, range) : []), [data, range]);
    // The CSV is one table of sets; the body log goes in the Excel and JSON files.
    const bodyIncluded = format !== "csv" && selectedBody.length > 0;
    const setCount = selected.reduce((n, l) => n + l.exercises.reduce((m, e) => m + e.sets.length, 0), 0);
    const hasGps = selected.some(l => l.exercises.some(e => e.gpxData));

    function handleExport() {
        if (!data) return;
        const blob = format === "csv"
            ? buildCsv(selected, data.exercises)
            : format === "xlsx"
                ? buildWorkbook(selected, data.exercises, selectedBody)
                : buildJson(selected, data.templates, data.exercises, { includeGps, range, body: selectedBody });
        downloadBlob(blob, exportFilename(format));
        setDone(true);
        setTimeout(() => setDone(false), 2200);
    }

    return (
        <Overlay title="Export your data" onClose={onClose}>
            <div className="export-panel">
                {error && <div className="error-banner">{error}</div>}

                <fieldset className="export-group">
                    <legend className="export-legend">Format</legend>
                    <div className="export-formats" role="radiogroup">
                        {FORMATS.map(f => {
                            const Icon = f.icon;
                            const active = format === f.value;
                            return (
                                <button
                                    key={f.value}
                                    type="button"
                                    role="radio"
                                    aria-checked={active}
                                    className={`export-format${active ? " active" : ""}`}
                                    onClick={() => setFormat(f.value)}
                                >
                                    <Icon size={18} className="export-format-icon" />
                                    <span className="export-format-label">{f.label} <small>{f.ext}</small></span>
                                    <span className="export-format-desc">{f.desc}</span>
                                    {active && <Check size={12} className="export-format-check" />}
                                </button>
                            );
                        })}
                    </div>
                </fieldset>

                <fieldset className="export-group">
                    <legend className="export-legend">Period</legend>
                    <div className="filter-chips">
                        {(Object.keys(RANGE_LABELS) as ExportRange[]).map(r => (
                            <button
                                key={r}
                                type="button"
                                className={`filter-chip${range === r ? " active" : ""}`}
                                aria-pressed={range === r}
                                onClick={() => setRange(r)}
                            >
                                {RANGE_LABELS[r]}
                            </button>
                        ))}
                    </div>
                </fieldset>

                {format === "json" && (
                    <label className={`export-check${hasGps ? "" : " disabled"}`}>
                        <input
                            type="checkbox"
                            checked={includeGps && hasGps}
                            disabled={!hasGps}
                            onChange={e => setIncludeGps(e.target.checked)}
                        />
                        Include GPS tracks <small>(makes the file much larger)</small>
                    </label>
                )}

                <div className="export-summary">
                    {data ? (
                        selected.length || bodyIncluded ? (
                            <p>
                                <b>{selected.length}</b> workout{selected.length !== 1 ? "s" : ""} · <b>{setCount}</b> set{setCount !== 1 ? "s" : ""}
                                {format === "json" && <> · <b>{data.templates.length}</b> template{data.templates.length !== 1 ? "s" : ""}</>}
                                {bodyIncluded && <> · <b>{selectedBody.length}</b> body log day{selectedBody.length !== 1 ? "s" : ""}</>}
                                {format === "csv" && selectedBody.length > 0 && <span className="export-note">The body log is in the Excel and JSON exports.</span>}
                            </p>
                        ) : (
                            <p>No workouts in this period.</p>
                        )
                    ) : !error ? (
                        <Skeleton width="12rem" height="0.9rem" />
                    ) : null}
                </div>

                <button
                    type="button"
                    className="button-component green export-submit"
                    disabled={!data || (selected.length === 0 && !bodyIncluded && format !== "json")}
                    onClick={handleExport}
                >
                    {done ? <Check size={14} /> : <Download size={14} />}
                    {done ? "Downloaded" : `Download ${exportFilename(format)}`}
                </button>
            </div>
        </Overlay>
    );
}

export default ExportOverlay;
