import { useEffect, useState } from "react";
import { getTemplates } from "../../templates/api.ts";
import type { WorkoutTemplate } from "../../templates/api.ts";
import Overlay from "../../../components/general/OverlayComponent.tsx";
import Button from "../../../components/general/ButtonComponent.tsx";
import {Plus} from "lucide-react";

interface StartSessionOverlayProps {
    onFromTemplate: (template: WorkoutTemplate) => void;
    onBlank: () => void;
    onClose: () => void;
}

function StartSessionOverlay({ onFromTemplate, onBlank, onClose }: StartSessionOverlayProps) {
    const [templates, setTemplates] = useState<WorkoutTemplate[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        getTemplates().then(ts => { setTemplates(ts); setLoading(false); });
    }, []);

    return (
        <Overlay title="What are we doing today?" subtitle="New Log" onClose={onClose}>
            {loading ? (
                <p className="feature-sidebar-loading">Loading…</p>
            ) : (
                <div className="start-session-content">
                    {templates.length > 0 ? (
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
                    <Button style="grey" onClick={onBlank} icon={<Plus size={14}/>} text={"Log without template"}/>
                </div>
            )}
        </Overlay>
    );
}

export default StartSessionOverlay;