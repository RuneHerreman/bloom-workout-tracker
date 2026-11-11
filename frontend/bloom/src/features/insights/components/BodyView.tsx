import { useCallback, useEffect, useMemo, useState } from "react";
import { Pencil, Trash2 } from "lucide-react";
import type { BodyEntry } from "../../../assets/js/data/apiTypes.ts";
import { getBody, peekBody, logBody, deleteBody, formatEntryDate } from "../../body/api.ts";
import LogWeightButton from "../../body/LogWeightButton.tsx";
import BodyChart from "./BodyChart.tsx";
import { MIN_BODY_FAT_READINGS, SERIES, buildSeries, seriesStats, localTime, type BodySeries, type SeriesStats } from "../bodySeries.ts";
import type { Period, Range } from "../analysis.ts";

const ROWS = 20;

const num = (v: number, digits = 1) => (Math.round(v * 10 ** digits) / 10 ** digits).toLocaleString("en-GB", { maximumFractionDigits: digits });
const signed = (v: number, digits = 1) => `${v > 0 ? "+" : v < 0 ? "−" : ""}${num(Math.abs(v), digits)}`;
const fmtRange = (start: Date, end: Date) =>
    `${start.toLocaleDateString("en-GB", { day: "numeric", month: "short" })} – ${end.toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" })}`;

/** Accepts "78.4" and "78,4"; empty is null, junk is NaN. */
function parse(v: string): number | null {
    const t = v.trim().replace(",", ".");
    if (t === "") return null;
    return /^\d+(\.\d+)?$/.test(t) ? Number(t) : NaN;
}

function StatRow({ series, stats }: { series: BodySeries; stats: SeriesStats }) {
    const unit = SERIES[series].unit;
    const pts = series === "bodyFat";
    const withUnit = (v: string) => (pts ? `${v}%` : `${v} ${unit}`);
    const changeUnit = pts ? " pts" : ` ${unit}`;
    return (
        <dl className="records">
            <div><dt>Trend now</dt><dd>{withUnit(num(stats.now))}</dd><span>smoothed over recent {pts || series === "lean" ? "readings" : "weigh-ins"}</span></div>
            <div>
                <dt>Change</dt>
                <dd>{stats.change === null ? "—" : `${signed(stats.change)}${changeUnit}`}</dd>
                <span>{stats.change === null ? "needs a second reading" : "this period"}</span>
            </div>
            <div>
                <dt>Rate</dt>
                <dd>{stats.rate ? `${signed(stats.rate.value, 2)}${changeUnit}` : "—"}</dd>
                <span>
                    {stats.rate
                        ? stats.rate.per === "week"
                            ? `per week · ${signed(stats.rate.pct ?? 0, 2)}% of body weight`
                            : "per month"
                        : series === "weight" ? "needs 6+ weigh-ins spread over 2 weeks" : "needs 4+ readings spread over 4 weeks"}
                </span>
            </div>
            <div><dt>Range</dt><dd>{num(stats.low)} – {withUnit(num(stats.high))}</dd><span>lowest – highest trend</span></div>
        </dl>
    );
}

function EntryRow({ entry, trend, onChanged }: { entry: BodyEntry; trend: number | undefined; onChanged: () => void }) {
    const [mode, setMode] = useState<"view" | "edit" | "confirm">("view");
    const [weight, setWeight] = useState("");
    const [bodyFat, setBodyFat] = useState("");
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);

    function startEdit() {
        setWeight(num(entry.weightKg, 2));
        setBodyFat(entry.bodyFatPct != null ? num(entry.bodyFatPct) : "");
        setError(null);
        setMode("edit");
    }

    async function save() {
        const kg = parse(weight), bf = parse(bodyFat);
        if (kg === null || Number.isNaN(kg) || kg < 20 || kg > 400) { setError("Weight: 20–400 kg"); return; }
        if (bf !== null && (Number.isNaN(bf) || bf < 3 || bf > 70)) { setError("Body fat: 3–70%"); return; }
        setBusy(true);
        try {
            await logBody(entry.date, bf === null ? { weightKg: kg, clearBodyFat: true } : { weightKg: kg, bodyFatPct: bf });
            setMode("view");
            onChanged();
        } catch (e) {
            setError((e as { error?: string })?.error ?? "Couldn't save");
        } finally {
            setBusy(false);
        }
    }

    async function remove() {
        setBusy(true);
        try { await deleteBody(entry.date); onChanged(); }
        catch (e) { setError((e as { error?: string })?.error ?? "Couldn't delete"); setBusy(false); }
    }

    const source = entry.source === "Signup" ? "from signup" : entry.source === "Import" ? "imported" : null;

    if (mode === "edit") {
        return (
            <tr className="body-row editing">
                <td>{formatEntryDate(entry.date)}</td>
                <td className="num">
                    <input className="body-cell-input" inputMode="decimal" aria-label="Weight in kg" value={weight} autoFocus
                        onChange={e => setWeight(e.target.value)} onKeyDown={e => { if (e.key === "Enter") save(); if (e.key === "Escape") setMode("view"); }} />
                </td>
                <td className="num">
                    <input className="body-cell-input" inputMode="decimal" aria-label="Body fat in percent" value={bodyFat} placeholder="—"
                        onChange={e => setBodyFat(e.target.value)} onKeyDown={e => { if (e.key === "Enter") save(); if (e.key === "Escape") setMode("view"); }} />
                </td>
                <td className="body-row-msg">{error && <span className="body-row-error">{error}</span>}</td>
                <td className="body-row-actions">
                    <button type="button" className="link-btn" onClick={() => setMode("view")} disabled={busy}>Cancel</button>
                    <button type="button" className="link-btn strong" onClick={save} disabled={busy}>Save</button>
                </td>
            </tr>
        );
    }

    return (
        <tr className="body-row">
            <td>
                {formatEntryDate(entry.date)}
                {source && <span className="body-source">{source}</span>}
            </td>
            <td className="num">{num(entry.weightKg, 2)} kg</td>
            <td className="num">{entry.bodyFatPct != null ? `${num(entry.bodyFatPct)}%` : <span className="body-blank">—</span>}</td>
            <td className="num body-delta">{trend !== undefined ? signed(entry.weightKg - trend) : ""}</td>
            <td className="body-row-actions">
                {mode === "confirm" ? (
                    <>
                        <span className="body-confirm">Delete this day?</span>
                        <button type="button" className="link-btn" onClick={() => setMode("view")} disabled={busy}>Keep</button>
                        <button type="button" className="link-btn danger" onClick={remove} disabled={busy}>Delete</button>
                    </>
                ) : (
                    <>
                        {error && <span className="body-row-error">{error}</span>}
                        <button type="button" className="icon-btn" onClick={startEdit} aria-label={`Edit ${entry.date}`} title="Edit"><Pencil size={13} /></button>
                        <button type="button" className="icon-btn" onClick={() => setMode("confirm")} aria-label={`Delete ${entry.date}`} title="Delete"><Trash2 size={13} /></button>
                    </>
                )}
            </td>
        </tr>
    );
}

/** Body weight, body fat and lean mass over the selected period, and the log itself. */
function BodyView({ range, period }: { range: Range; period: Period }) {
    const [entries, setEntries] = useState<BodyEntry[] | null>(() => peekBody() ?? null);
    const [error, setError] = useState<string | null>(null);
    const [series, setSeries] = useState<BodySeries>("weight");
    const [showAll, setShowAll] = useState(false);

    const load = useCallback(() => {
        getBody().then(setEntries).catch(e => setError((e as { error?: string })?.error ?? "Couldn't load your body log"));
    }, []);
    useEffect(load, [load]);

    // "All" means all of the body log too, which can start before the first workout.
    const start = useMemo(() => {
        if (period !== "all" || !entries?.length) return range.start;
        return new Date(Math.min(range.start.getTime(), localTime(entries[0].date)));
    }, [period, entries, range.start]);

    const bodyFatReadings = entries?.filter(e => e.bodyFatPct != null).length ?? 0;
    const active: BodySeries = series !== "weight" && bodyFatReadings < MIN_BODY_FAT_READINGS ? "weight" : series;

    const all = useMemo(() => (entries ? buildSeries(entries, active) : []), [entries, active]);
    const stats = useMemo(() => seriesStats(all, active, start, range.end), [all, active, start, range.end]);
    const weightTrendByDate = useMemo(
        () => new Map((entries ? buildSeries(entries, "weight") : []).map(p => [p.date, p.trend])),
        [entries],
    );
    const periodEntries = useMemo(() => {
        const from = new Date(start.getFullYear(), start.getMonth(), start.getDate()).getTime();
        return (entries ?? []).filter(e => localTime(e.date) >= from).reverse();
    }, [entries, start]);

    if (error) return <div className="error-banner">{error}</div>;
    if (!entries) return null;

    if (entries.length === 0) {
        return (
            <section className="panel body-empty">
                <p>Your body log is empty. Log a weigh-in and your trend starts here.</p>
                <LogWeightButton onLogged={load} />
            </section>
        );
    }

    const meta = SERIES[active];
    const rows = showAll ? periodEntries : periodEntries.slice(0, ROWS);

    return (
        <>
            <section className="panel body-panel">
                <header className="body-head">
                    <div className="panel-head">
                        <h2 className="panel-title">{meta.label}</h2>
                        <p className="panel-desc">
                            {active === "weight"
                                ? "The line is your trend: it smooths out day-to-day water swings."
                                : active === "bodyFat"
                                    ? "Readings only compare with ones from the same method and time of day."
                                    : "Weight minus fat, from the days you logged body fat."}
                        </p>
                    </div>
                    <div className="body-head-actions">
                        <div className="segmented segmented-sm" role="radiogroup" aria-label="Series">
                            {(Object.keys(SERIES) as BodySeries[]).map(s => {
                                const locked = s !== "weight" && bodyFatReadings < MIN_BODY_FAT_READINGS;
                                return (
                                    <button key={s} role="radio" aria-checked={active === s} disabled={locked}
                                        title={locked ? `Log body fat at least ${MIN_BODY_FAT_READINGS} times to see this` : undefined}
                                        className={`segmented-item${active === s ? " active" : ""}`} onClick={() => setSeries(s)}>
                                        {SERIES[s].label}
                                    </button>
                                );
                            })}
                        </div>
                        <LogWeightButton onLogged={load} />
                    </div>
                </header>

                {stats ? (
                    <>
                        <StatRow series={active} stats={stats} />
                        <BodyChart
                            points={stats.points}
                            start={start}
                            end={range.end}
                            unit={meta.unit}
                            pad={active === "bodyFat" ? 1 : 1.5}
                            rawLabel={active === "weight" ? "Weigh-in" : "Reading"}
                            trendLabel="Trend"
                            ariaLabel={`${meta.label} between ${fmtRange(start, range.end)}: readings and trend`}
                        />
                    </>
                ) : (
                    <p className="viz-empty">No {active === "weight" ? "weigh-ins" : "readings"} between {fmtRange(start, range.end)}. Try a longer period.</p>
                )}
                {bodyFatReadings < MIN_BODY_FAT_READINGS && (
                    <p className="detail-note">Body fat and lean mass show up once you've logged body fat twice.</p>
                )}
            </section>

            <section className="panel">
                <header className="panel-head">
                    <h2 className="panel-title">Entries</h2>
                    <p className="panel-desc">{periodEntries.length} {periodEntries.length === 1 ? "day" : "days"} this period · newest first</p>
                </header>
                {periodEntries.length === 0 ? (
                    <p className="viz-empty">Nothing logged this period.</p>
                ) : (
                    <>
                        <table className="history-table body-table">
                            <thead>
                                <tr>
                                    <th>Day</th>
                                    <th className="num">Weight</th>
                                    <th className="num">Body fat</th>
                                    <th className="num" title="Weigh-in minus trend: how far the day sat from your trend">vs trend</th>
                                    <th aria-label="Actions" />
                                </tr>
                            </thead>
                            <tbody>
                                {rows.map(e => (
                                    <EntryRow key={`${e.date}-${e.weightKg}-${e.bodyFatPct}`} entry={e} trend={weightTrendByDate.get(e.date)} onChanged={load} />
                                ))}
                            </tbody>
                        </table>
                        {periodEntries.length > ROWS && (
                            <button type="button" className="link-btn body-more" onClick={() => setShowAll(v => !v)}>
                                {showAll ? "Show fewer" : `Show all ${periodEntries.length}`}
                            </button>
                        )}
                    </>
                )}
            </section>
        </>
    );
}

export default BodyView;
