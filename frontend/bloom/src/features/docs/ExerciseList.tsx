import { useDeferredValue, useMemo, useState } from "react";
import { Download, Search } from "lucide-react";
import type { Exercise } from "../../assets/js/data/apiTypes.ts";
import { downloadBlob } from "../export/exportData.ts";

const PAGE = 60;

/** Quotes a CSV cell when needed. */
const cell = (v: string) => (/[",\n]/.test(v) ? `"${v.replace(/"/g, '""')}"` : v);

function exercisesCsv(exercises: Exercise[]): Blob {
    const rows = [["Name", "Type", "Target muscles", "Custom"], ...exercises.map(e => [e.name, e.type, e.targetMuscles.join(", "), e.isCustom ? "yes" : ""])];
    // BOM so Excel opens it as UTF-8.
    return new Blob(["﻿" + rows.map(r => r.map(cell).join(",")).join("\r\n")], { type: "text/csv;charset=utf-8" });
}

/** Every exercise in the account, so people converting their own notes can see which names Bloom knows. */
function ExerciseList({ exercises }: { exercises: Exercise[] }) {
    const [query, setQuery] = useState("");
    const [type, setType] = useState<"All" | Exercise["type"]>("All");
    const [limit, setLimit] = useState(PAGE);
    const deferred = useDeferredValue(query);

    const filtered = useMemo(() => {
        const q = deferred.trim().toLowerCase();
        return exercises
            .filter(e => (type === "All" || e.type === type) && (!q || e.name.toLowerCase().includes(q) || e.targetMuscles.some(m => m.toLowerCase().includes(q))))
            .sort((a, b) => a.name.localeCompare(b.name));
    }, [exercises, deferred, type]);

    if (exercises.length === 0) return <p className="docs-note">Loading your exercises…</p>;

    return (
        <div className="docs-exercises">
            <div className="docs-exercises-bar">
                <label className="docs-search">
                    <Search size={14} />
                    <input type="text" placeholder="Search by name or muscle…" value={query} onChange={e => { setQuery(e.target.value); setLimit(PAGE); }} aria-label="Search exercises" />
                </label>
                <div className="filter-chips">
                    {(["All", "Strength", "Cardio", "Plyometric"] as const).map(t => (
                        <button key={t} type="button" className={`filter-chip${type === t ? " active" : ""}`} aria-pressed={type === t} onClick={() => { setType(t); setLimit(PAGE); }}>{t}</button>
                    ))}
                </div>
                <button type="button" className="docs-btn" onClick={() => downloadBlob(exercisesCsv(exercises), "bloom-exercises.csv")}>
                    <Download size={13} /> Download all ({exercises.length})
                </button>
            </div>

            <div className="docs-table-wrap">
                <table className="docs-table">
                    <thead><tr><th>Name</th><th>Type</th><th>Target muscles</th></tr></thead>
                    <tbody>
                        {filtered.slice(0, limit).map(e => (
                            <tr key={e.id}>
                                <td>{e.name}{e.isCustom && <span className="docs-req"> yours</span>}</td>
                                <td className="docs-type">{e.type}</td>
                                <td>{e.targetMuscles.join(", ")}</td>
                            </tr>
                        ))}
                        {filtered.length === 0 && <tr><td colSpan={3}>No exercises match.</td></tr>}
                    </tbody>
                </table>
            </div>
            <div className="docs-exercises-foot">
                <span className="docs-note">{filtered.length} of {exercises.length} exercises</span>
                {filtered.length > limit && <button type="button" className="docs-btn" onClick={() => setLimit(l => l + PAGE)}>Show more</button>}
            </div>
        </div>
    );
}

export default ExerciseList;
