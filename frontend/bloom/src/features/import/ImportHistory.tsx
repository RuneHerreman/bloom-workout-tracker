import { useState } from "react";
import { FileText, Loader2, Undo2 } from "lucide-react";
import type { ImportBatchSummary } from "../logbook/api.ts";

const plural = (n: number, w: string) => `${n} ${w}${n === 1 ? "" : "s"}`;
const when = (iso: string) => new Date(iso).toLocaleString("en-GB", { day: "numeric", month: "short", year: "numeric", hour: "2-digit", minute: "2-digit" });

/** Past imports, newest first; each can be undone (with a confirm) as long as it hasn't been already. */
function ImportHistory({ history, onUndo }: { history: ImportBatchSummary[] | null; onUndo: (id: string) => Promise<void> }) {
    const [confirming, setConfirming] = useState<string | null>(null);
    const [busy, setBusy] = useState<string | null>(null);

    if (!history || history.length === 0) return null;

    return (
        <section className="imp-card imp-history" aria-label="Your imports">
            <h2>Your imports</h2>
            <ul>
                {history.map(b => {
                    const added = [
                        b.workouts ? plural(b.workouts, "workout") : null,
                        b.exercises ? plural(b.exercises, "new exercise") : null,
                        b.templates ? plural(b.templates, "template") : null,
                        b.bodyEntries ? plural(b.bodyEntries, "weigh-in") : null,
                    ].filter(Boolean).join(" · ");
                    return (
                        <li key={b.id} className={b.undoneAt ? "undone" : ""}>
                            <FileText size={16} aria-hidden />
                            <div className="imp-history-main">
                                <b>{b.fileName}</b>
                                <span className="imp-muted small">{when(b.importedAt)} · {added || "nothing added"}</span>
                            </div>
                            {b.undoneAt ? (
                                <span className="imp-history-tag">Undone {new Date(b.undoneAt).toLocaleDateString("en-GB", { day: "numeric", month: "short" })}</span>
                            ) : busy === b.id ? (
                                <span className="imp-muted small"><Loader2 size={14} className="imp-spin" /> Undoing…</span>
                            ) : confirming === b.id ? (
                                <span className="imp-history-confirm">
                                    <span className="imp-muted small">Remove what this import added?</span>
                                    <button type="button" className="imp-btn danger" onClick={async () => { setBusy(b.id); setConfirming(null); await onUndo(b.id); setBusy(null); }}>
                                        Undo import
                                    </button>
                                    <button type="button" className="imp-btn ghost" onClick={() => setConfirming(null)}>Keep</button>
                                </span>
                            ) : (
                                <button type="button" className="imp-link" onClick={() => setConfirming(b.id)}><Undo2 size={14} /> Undo</button>
                            )}
                        </li>
                    );
                })}
            </ul>
            <p className="imp-muted small">Undo removes the workouts, templates and weigh-ins an import added, and new exercises nothing else uses. What you logged yourself isn't touched.</p>
        </section>
    );
}

export default ImportHistory;
