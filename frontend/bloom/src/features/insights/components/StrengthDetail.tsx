import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Trophy } from "lucide-react";
import type { ExerciseProgress, ExerciseSession, Range } from "../analysis.ts";
import TrendChart, { type TrendPoint } from "./TrendChart.tsx";
import { VizTooltip } from "./ChartParts.tsx";
import InfoHint from "../../../components/general/InfoHint.tsx";

type Metric = "e1rm" | "heaviest" | "volume";

const METRICS: { value: Metric; label: string; desc: string }[] = [
    { value: "heaviest", label: "Heaviest weight", desc: "Your heaviest set each session" },
    { value: "e1rm", label: "Est. 1RM", desc: "Estimated one-rep max from your best set" },
    { value: "volume", label: "Volume", desc: "Weight × reps, summed over working sets" },
];

interface Point extends TrendPoint { session: ExerciseSession }

const fmtDate = (iso: string) => new Date(iso).toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" });
const fmtKg = (v: number) => `${Math.round(v * 10) / 10} kg`;
const fmtVolume = (v: number) => (v >= 10_000 ? `${(v / 1000).toFixed(1)} t` : `${Math.round(v).toLocaleString("en-US")} kg`);

function StrengthDetail({ exercise, range }: { exercise: ExerciseProgress; range: Range }) {
    const [metric, setMetric] = useState<Metric>("heaviest");
    const navigate = useNavigate();
    const s = exercise.sessions;

    const valueOf = (x: ExerciseSession) => (metric === "e1rm" ? x.e1rm : metric === "heaviest" ? x.heaviest : x.volumeKg);
    const points: Point[] = s.map(x => ({ t: new Date(x.date).getTime(), value: valueOf(x), highlight: metric === "heaviest" ? x.isWeightPr : metric === "e1rm" && x.isPr, session: x }));
    const format = metric === "volume" ? fmtVolume : fmtKg;

    const bestE1rm = s.reduce((a, b) => (b.e1rm > a.e1rm ? b : a));
    const heaviest = s.reduce((a, b) => (b.heaviest > a.heaviest ? b : a));
    const bestVolume = s.reduce((a, b) => (b.volumeKg > a.volumeKg ? b : a));
    const change = exercise.changePct;

    return (
        <div className="detail">
            <header className="detail-head">
                <div>
                    <h3 className="detail-title">{exercise.name}</h3>
                    <p className="detail-sub">
                        {exercise.muscles.join(", ")}
                        {change !== null && (
                            <> · <span className={change > 0 ? "text-up" : change < 0 ? "text-down" : ""}>
                                {change > 0 ? "+" : ""}{change}% heaviest weight this period
                            </span></>
                        )}
                    </p>
                </div>
                <div className="segmented segmented-sm" role="radiogroup" aria-label="Metric">
                    {METRICS.map(m => (
                        <button key={m.value} role="radio" aria-checked={metric === m.value}
                            className={`segmented-item${metric === m.value ? " active" : ""}`}
                            onClick={() => setMetric(m.value)}>
                            {m.label}
                        </button>
                    ))}
                </div>
            </header>

            <TrendChart
                points={points}
                start={range.start}
                end={range.end}
                formatValue={format}
                ariaLabel={`${exercise.name}: ${METRICS.find(m => m.value === metric)!.label} per session`}
                seriesLabel={METRICS.find(m => m.value === metric)!.desc}
                highlightLabel={metric === "volume" ? "" : "New personal record"}
                recordLine={metric !== "volume"}
                renderTooltip={p => (
                    <VizTooltip
                        title={fmtDate(p.session.date)}
                        rows={[
                            metric === "heaviest"
                                ? { label: "Heaviest set", value: `${p.session.heaviest} kg × ${p.session.heaviestReps}`, color: "var(--viz-1)" }
                                : { label: METRICS.find(m => m.value === metric)!.label, value: format(p.value), color: "var(--viz-1)" },
                            ...(metric === "e1rm" ? [{ label: "From set", value: `${p.session.topSet.weight} kg × ${p.session.topSet.reps}` }] : []),
                            { label: "Working sets", value: p.session.workingSets },
                            ...(metric !== "volume" ? [{ label: "Volume", value: fmtVolume(p.session.volumeKg) }] : []),
                            ...(p.highlight ? [{ label: "New personal record", value: "★" }] : []),
                        ]}
                    />
                )}
            />
            {metric === "e1rm" && (
                <p className="detail-note">
                    Est. 1RM: the weight you could likely lift once, worked out from your best set.
                    <InfoHint title="Estimated 1RM" label="What is estimated 1RM?">
                        <p>The weight you could likely lift once, worked out from your best set (Epley formula). It lets “80 kg × 8” and “90 kg × 3” be compared on one line.</p>
                    </InfoHint>
                </p>
            )}

            <dl className="records">
                <div><dt>Heaviest set</dt><dd>{heaviest.heaviest} kg × {heaviest.heaviestReps}</dd><span>{fmtDate(heaviest.date)}</span></div>
                <div><dt>Est. 1RM</dt><dd>{fmtKg(bestE1rm.e1rm)}</dd><span>from {bestE1rm.topSet.weight} kg × {bestE1rm.topSet.reps}</span></div>
                <div><dt>Sessions</dt><dd>{s.length}</dd><span>this period</span></div>
                <div><dt>Best session volume</dt><dd>{fmtVolume(bestVolume.volumeKg)}</dd><span>{fmtDate(bestVolume.date)}</span></div>
            </dl>

            <div className="history">
                <h4 className="history-title">Recent sessions</h4>
                <table className="history-table">
                    <thead>
                        <tr><th>Date</th><th>Heaviest set</th><th className="num">Sets</th><th className="num">Volume</th><th className="num">Est. 1RM</th></tr>
                    </thead>
                    <tbody>
                        {[...s].reverse().slice(0, 6).map(x => (
                            <tr key={x.logId + x.date} onClick={() => navigate("/logbook", { state: { selectedLogId: x.logId } })} title="Open this workout">
                                <td>{fmtDate(x.date)}</td>
                                <td>
                                    {x.isWeightPr && <Trophy size={12} className="pr-icon" aria-label="Personal record" />}
                                    {x.heaviest} kg × {x.heaviestReps}
                                </td>
                                <td className="num">{x.workingSets}</td>
                                <td className="num">{fmtVolume(x.volumeKg)}</td>
                                <td className="num">
                                    {fmtKg(x.e1rm)}
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
}

export default StrengthDetail;
