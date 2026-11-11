import { useState } from "react";
import { SET_RANGE, type MuscleVolume } from "../analysis.ts";

const DAY = 86_400_000;

/**
 * Ranked bars of weekly working sets per muscle, against the commonly cited
 * 10–20 sets/week range (shaded). Plain HTML: a ranked list reads better than a chart.
 */
function MuscleBalance({ muscles, periodEnd }: { muscles: MuscleVolume[]; periodEnd: Date }) {
    const [showAll, setShowAll] = useState(false);
    if (!muscles.length) return <p className="viz-empty">No strength sets in this period.</p>;

    const scaleMax = Math.max(SET_RANGE.high + 4, ...muscles.map(m => m.setsPerWeek));
    const pct = (v: number) => `${(v / scaleMax) * 100}%`;
    const visible = showAll ? muscles : muscles.slice(0, 8);

    return (
        <>
            <ul className="muscle-list">
                {visible.map(m => {
                    const weeksAgo = m.lastTrained ? Math.floor((periodEnd.getTime() - new Date(m.lastTrained).getTime()) / (7 * DAY)) : null;
                    const status = m.setsPerWeek < SET_RANGE.low ? "below" : m.setsPerWeek > SET_RANGE.high ? "above" : "within";
                    return (
                        <li key={m.muscle} className="muscle-row">
                            <span className="muscle-name">
                                {m.muscle}
                                {weeksAgo !== null && weeksAgo >= 3 && <span className="muscle-stale">not trained in {weeksAgo} weeks</span>}
                            </span>
                            <span className="muscle-track" aria-hidden>
                                <span className="muscle-band" style={{ left: pct(SET_RANGE.low), width: pct(SET_RANGE.high - SET_RANGE.low) }} />
                                <span className={`muscle-bar ${status}`} style={{ width: pct(m.setsPerWeek) }} />
                            </span>
                            <span className="muscle-value">
                                {m.setsPerWeek}
                                <span className="sr-only"> sets per week, {status} the {SET_RANGE.low}–{SET_RANGE.high} range</span>
                            </span>
                        </li>
                    );
                })}
            </ul>
            <div className="muscle-foot">
                <span className="muscle-key"><span className="muscle-key-band" /> {SET_RANGE.low}–{SET_RANGE.high} sets/week, a common target for growth</span>
                {muscles.length > 8 && (
                    <button type="button" className="link-btn" onClick={() => setShowAll(v => !v)}>
                        {showAll ? "Show fewer" : `All ${muscles.length} muscles`}
                    </button>
                )}
            </div>
        </>
    );
}

export default MuscleBalance;
