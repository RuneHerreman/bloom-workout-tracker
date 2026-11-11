import { NavLink, useNavigate } from "react-router-dom";
import { PlayIcon } from "lucide-react";
import GeneralWidget from "./GeneralWidget.tsx";
import WidgetHeader from "./WidgetHeader.tsx";
import Button from "../../../components/general/ButtonComponent.tsx";
import { RESTED_DAYS, type MuscleRest, type UpNext } from "../transforms.ts";

const ago = (days: number) => (days === 0 ? "today" : days === 1 ? "yesterday" : `${days} days ago`);

function sinceLastDone(iso: string | null): string {
    if (!iso) return "Not logged yet";
    const d = new Date(iso);
    return `Last done ${d.toLocaleDateString("en-GB", { weekday: "short", day: "numeric", month: "short" })}`;
}

const restLabel = (m: MuscleRest) => (Number.isFinite(m.days) ? `${m.muscle} · ${m.days}d` : m.muscle);

const MAX_CHIPS = 5;

/** The template's main muscles first; the rest collapse into "+N". */
function Chips({ muscles, kind }: { muscles: MuscleRest[]; kind: "rested" | "recovering" }) {
    const shown = muscles.slice(0, MAX_CHIPS);
    const hidden = muscles.slice(MAX_CHIPS);
    return (
        <ul className="muscle-chips">
            {shown.map(m => (
                <li key={m.muscle} className={`chip ${kind}`} title={Number.isFinite(m.days) ? `Trained ${ago(m.days)}` : "Not trained yet"}>{restLabel(m)}</li>
            ))}
            {hidden.length > 0 && <li className="chip more" title={hidden.map(restLabel).join(", ")}>+{hidden.length}</li>}
        </ul>
    );
}

/**
 * Suggests the template whose muscles have rested longest (after Fitbod-style
 * muscle recovery, kept to plain "days since trained" so it's easy to trust).
 */
function UpNextWidget({ next, hasTemplates }: { next: UpNext | null; hasTemplates: boolean }) {
    const navigate = useNavigate();

    return (
        <GeneralWidget
            className="up-next-widget"
            header={<WidgetHeader title="Up next" subtitle={`Based on which muscles have had ${RESTED_DAYS}+ days of rest`} />}
            content={
                !next ? (
                    <div className="up-next-empty">
                        <p>{hasTemplates ? "Add exercises to a template to get a suggestion." : "Save your usual workouts as templates and Bloom suggests the one you're most rested for."}</p>
                        <NavLink to="/templates" className="widget-link">Go to templates →</NavLink>
                    </div>
                ) : (
                    <div className="up-next">
                        <div className="up-next-head">
                            <div>
                                <p className="up-next-name">{next.template.name}</p>
                                <p className="up-next-meta">{sinceLastDone(next.lastDone)} · {next.template.exercises.length} exercises</p>
                            </div>
                            <Button
                                text="Start"
                                icon={<PlayIcon size={14} />}
                                style="green"
                                onClick={() => navigate("/logbook", { state: { startTemplateId: next.template.id } })}
                            />
                        </div>
                        {next.rested.length > 0 && (
                            <div className="up-next-group">
                                <span className="up-next-label">Rested</span>
                                <Chips muscles={next.rested} kind="rested" />
                            </div>
                        )}
                        {next.recovering.length > 0 && (
                            <div className="up-next-group">
                                <span className="up-next-label">Still recovering</span>
                                <Chips muscles={next.recovering} kind="recovering" />
                            </div>
                        )}
                    </div>
                )
            }
        />
    );
}

export default UpNextWidget;
