import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Trophy } from "lucide-react";
import type { ExerciseProgress, ExerciseSession, Range } from "../analysis.ts";
import TrendChart, { type TrendPoint } from "./TrendChart.tsx";
import { VizTooltip } from "./ChartParts.tsx";
import InfoHint from "../../../components/general/InfoHint.tsx";

type Metric = "e1rm" | "heaviest" | "volume";

const METRICS: { value: Metric; label: string }[] = [
    { value: "e1rm", label: "Est. 1RM" },
    { value: "heaviest", label: "Heaviest weight" },
    { value: "volume", label: "Volume" },
];

interface Point extends TrendPoint { session: ExerciseSession }

const fmtDate = (iso: string) => new Date(iso).toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" });
const fmtKg = (v: number) => `${Math.round(v * 10) / 10} kg`;
const fmtVolume = (v: number) => (v >= 10_000 ? `${(v / 1000).toFixed(1)} t` : `${Math.round(v).toLocaleString("en-US")} kg`);

function StrengthDetail({ exercise, range }: { exercise: ExerciseProgress; range: Range }) {
    const [metric, setMetric] = useState<Metric>("e1rm");
    const navigate = useNavigate();
    const s = exercise.sessions;

    const valueOf = (x: ExerciseSession) => (metric === "e1rm" ? x.e1rm : metric === "heaviest" ? x.heaviest : x.volumeKg);
    const points: Point[] = s.map(x => ({ t: new Date(x.date).getTime(), value: valueOf(x), highlight: metric === "e1rm" && x.isPr, session: x }));
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
                                {change > 0 ? "+" : ""}{change}% est. 1RM this period
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
                renderTooltip={p => (
                    <VizTooltip
                        title={fmtDate(p.session.date)}
                        rows={[
                            { label: METRICS.find(m => m.value === metric)!.label, value: format(p.value), color: "var(--viz-1)" },
                            { label: "Best set", value: `${p.session.topSet.weight} kg × ${p.session.topSet.reps}` },
                            { label: "Working sets", value: p.session.workingSets },
                            ...(p.session.isPr ? [{ label: "Personal record", value: "★" }] : []),
                        ]}
                    />
                )}
            />
            {metric === "e1rm" && (
                <p className="detail-note">
                    Larger dots are new personal records.
                    <InfoHint title="Estimated 1RM" label="What is estimated 1RM?">
                        <p>The weight you could likely lift once, worked out from your best set (Epley formula). It lets “80 kg × 8” and “90 kg × 3” be compared on one line.</p>
                    </InfoHint>
                </p>
            )}

            <dl className="records">
                <div><dt>Est. 1RM</dt><dd>{fmtKg(bestE1rm.e1rm)}</dd><span>{fmtDate(bestE1rm.date)}</span></div>
                <div><dt>Heaviest weight</dt><dd>{fmtKg(heaviest.heaviest)}</dd><span>{fmtDate(heaviest.date)}</span></div>
                <div><dt>Best set</dt><dd>{bestE1rm.topSet.weight} kg × {bestE1rm.topSet.reps}</dd><span>by est. 1RM</span></div>
                <div><dt>Best session volume</dt><dd>{fmtVolume(bestVolume.volumeKg)}</dd><span>{fmtDate(bestVolume.date)}</span></div>
            </dl>

            <div className="history">
                <h4 className="history-title">Recent sessions</h4>
                <table className="history-table">
                    <thead>
                        <tr><th>Date</th><th>Best set</th><th className="num">Sets</th><th className="num">Volume</th><th className="num">Est. 1RM</th></tr>
                    </thead>
                    <tbody>
                        {[...s].reverse().slice(0, 6).map(x => (
                            <tr key={x.logId + x.date} onClick={() => navigate("/logbook", { state: { selectedLogId: x.logId } })} title="Open this workout">
                                <td>{fmtDate(x.date)}</td>
                                <td>{x.topSet.weight} kg × {x.topSet.reps}</td>
                                <td className="num">{x.workingSets}</td>
                                <td className="num">{fmtVolume(x.volumeKg)}</td>
                                <td className="num">
                                    {x.isPr && <Trophy size={12} className="pr-icon" aria-label="Personal record" />}
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
