import GeneralWidget from "./GeneralWidget.tsx";
import WidgetHeader from "./WidgetHeader.tsx";
import { SET_RANGE } from "../../insights/analysis.ts";
import type { MuscleFocus } from "../transforms.ts";

const plural = (n: number, word: string) => `${n} ${word}${n === 1 ? "" : "s"}`;

/** Working sets per week per muscle over the last four weeks, against the usual 10–20 range. */
function MuscleFocusWidget({ focus }: { focus: MuscleFocus }) {
    const max = Math.max(SET_RANGE.high, ...focus.muscles.map(m => m.setsPerWeek));
    const pct = (v: number) => `${(v / max) * 100}%`;
    const sessions = [
        focus.strengthSessions ? plural(focus.strengthSessions, "strength session") : null,
        focus.cardioSessions ? plural(focus.cardioSessions, "cardio session") : null,
    ].filter(Boolean).join(" · ");

    return (
        <GeneralWidget
            className="muscle-widget"
            header={<WidgetHeader title="Training focus" subtitle={`Last 4 weeks${sessions ? ` · ${sessions}` : ""}`} />}
            content={
                focus.muscles.length === 0
                    ? <p className="widget-empty">No strength training in the last four weeks.</p>
                    : (
                        <>
                            <div className="muscle-scale" aria-hidden>
                                <span>Sets per week</span>
                                <span className="muscle-scale-band" style={{ left: pct(SET_RANGE.low), width: pct(SET_RANGE.high - SET_RANGE.low) }}>
                                    {SET_RANGE.low}–{SET_RANGE.high}
                                </span>
                            </div>
                            <ul className="muscle-bars">
                                {focus.muscles.map(m => (
                                    <li key={m.muscle} className="muscle-row">
                                        <span className="muscle-name">{m.muscle}</span>
                                        <span className="muscle-track">
                                            <span className="muscle-band" style={{ left: pct(SET_RANGE.low), width: pct(SET_RANGE.high - SET_RANGE.low) }} />
                                            <span className={`muscle-fill${m.setsPerWeek < SET_RANGE.low ? " low" : ""}`} style={{ width: pct(m.setsPerWeek) }} />
                                        </span>
                                        <span className="muscle-value">{m.setsPerWeek}</span>
                                    </li>
                                ))}
                            </ul>
                            {focus.untrained.length > 0 && (
                                <p className="muscle-untrained">Not trained: {focus.untrained.join(", ")}</p>
                            )}
                        </>
                    )
            }
        />
    );
}

export default MuscleFocusWidget;
