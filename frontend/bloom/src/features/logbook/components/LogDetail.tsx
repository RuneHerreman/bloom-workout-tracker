import { useState } from "react";
import type { LoggedWorkout } from "../api.ts";
import { updateLog } from "../api.ts";
import type { Exercise } from "../../exercises/api.ts";
import { toDateInputValue } from "../logbookUtils.ts";
import LogExerciseCard from "./LogExerciseCard.tsx";
import Button from "../../../components/general/ButtonComponent.tsx";
import { Save, Trash2 } from "lucide-react";

interface LogDetailProps {
    log: LoggedWorkout;
    exercises: Record<string, Exercise>;
    onSave: (id: string, name: string, loggedAt: string, note: string | null) => void;
    onDelete: (id: string) => void;
}

function LogDetail({ log, exercises, onSave, onDelete }: LogDetailProps) {
    const initialDate = toDateInputValue(log.loggedAt);

    const [name, setName] = useState(log.name);
    const [date, setDate] = useState(initialDate);
    const [note, setNote] = useState(log.note ?? "");
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const hasChanges = name !== log.name
        || date !== initialDate
        || note !== (log.note ?? "");

    async function handleSave() {
        setSaving(true);
        setError(null);
        try {
            const loggedAt = new Date(date).toISOString();
            await updateLog(log.id, name, loggedAt, log.exercises, note || null);
            onSave(log.id, name, loggedAt, note || null);
        } catch (e) {
            setError(e instanceof Error ? e.message : "Failed to save log");
        } finally {
            setSaving(false);
        }
    }

    return (
        <div className="log-detail-view">
            {error && <div className="error-banner">{error}</div>}
            <div className="log-detail-header">
                <div className="log-detail-title-group">
                    <input
                        className="template-title-input"
                        value={name}
                        onChange={e => setName(e.target.value)}
                    />
                    <input
                        type="date"
                        className="log-date-input"
                        value={date}
                        onChange={e => setDate(e.target.value)}
                    />
                </div>
                <div className="actions-row">
                    <Button text="Save Changes" style="green" icon={<Save size={14} />} disabled={!hasChanges || saving} onClick={handleSave} />
                    <Button text="Delete Log" style="red" icon={<Trash2 size={14} />} onClick={() => onDelete(log.id)} />
                </div>
            </div>
            <textarea
                className="log-note-area"
                placeholder="Add a note about this workout…"
                value={note}
                onChange={e => setNote(e.target.value)}
            />
            {[...log.exercises].sort((a, b) => a.order - b.order).map(ex => (
                <LogExerciseCard
                    key={ex.exerciseId}
                    exercise={ex}
                    exerciseInfo={exercises[ex.exerciseId]}
                />
            ))}
        </div>
    );
}

export default LogDetail;