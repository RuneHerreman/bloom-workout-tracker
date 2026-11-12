import { useEffect, useState } from "react";
import { getTemplates } from "../../templates/api.ts";
import type { WorkoutTemplate } from "../../templates/api.ts";
import type { LoggedWorkout } from "../api.ts";
import Overlay from "../../../components/general/OverlayComponent.tsx";
import Button from "../../../components/general/ButtonComponent.tsx";
import Skeleton from "../../../components/general/Skeleton.tsx";
import { Plus, PenLine } from "lucide-react";

interface StartSessionOverlayProps {
    draft: LoggedWorkout | null;
    onFromTemplate: (template: WorkoutTemplate) => void;
    onBlank: () => void;
    onResumeDraft: () => void;
    onClose: () => void;
}

function StartSessionOverlay({ draft, onFromTemplate, onBlank, onResumeDraft, onClose }: StartSessionOverlayProps) {
    const [templates, setTemplates] = useState<WorkoutTemplate[] | null>(null);

    useEffect(() => {
        getTemplates().then(setTemplates).catch(() => setTemplates([]));
    }, []);

    const draftSets = draft?.exercises.reduce((n, e) => n + e.sets.length, 0) ?? 0;

    return (
        <Overlay title="Log a workout" onClose={onClose}>
            <div className="start-session-content">
                {draft && (
                    <button type="button" className="start-session-draft" onClick={onResumeDraft}>
                        <PenLine size={16} />
                        <span>
                            <span className="start-session-card-name">Continue draft{draft.name ? `: ${draft.name}` : ""}</span>
                            <span className="start-session-card-meta">
                                {draft.exercises.length} exercise{draft.exercises.length !== 1 ? "s" : ""} · {draftSets} set{draftSets !== 1 ? "s" : ""} · not saved yet
                            </span>
                        </span>
                    </button>
                )}
                <p className="start-session-label">From a template</p>
                {templates === null ? (
                    <div className="start-session-grid">
                        {[0, 1, 2].map(i => <Skeleton key={i} height="3.6rem" radius="var(--widget-border-radius)" />)}
                    </div>
                ) : templates.length > 0 ? (
                    <div className="start-session-grid">
                        {templates.map(t => (
                            <button key={t.id} className="start-session-card" onClick={() => onFromTemplate(t)}>
                                <span className="start-session-card-name">{t.name}</span>
                                <span className="start-session-card-meta">
                                    {t.exercises.length} {t.exercises.length === 1 ? "exercise" : "exercises"}
                                </span>
                            </button>
                        ))}
                    </div>
                ) : (
                    <p className="start-session-empty">No templates yet</p>
                )}
                <Button style="grey" onClick={onBlank} icon={<Plus size={14} />} text="Empty workout" />
                <p className="start-session-hint">Grey numbers are what you did last time: leave a field empty to keep them. Nothing is saved until you press Save.</p>
            </div>
        </Overlay>
    );
}

export default StartSessionOverlay;
