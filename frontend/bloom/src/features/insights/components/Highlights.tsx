import { TrendingUp, TrendingDown, Target, Trophy, AlertTriangle, Route, Gauge, CalendarCheck, Info } from "lucide-react";
import type { Highlight } from "../analysis.ts";

const ICONS: Record<Highlight["kind"], typeof Info> = {
    consistency: CalendarCheck,
    progress: TrendingUp,
    stalled: TrendingDown,
    muscle: Target,
    prs: Trophy,
    distance: Route,
    longest: Route,
    pace: Gauge,
};

const TONE_LABEL: Record<Highlight["tone"], string> = { good: "Going well", attention: "Needs attention", neutral: "" };

interface HighlightsProps {
    items: Highlight[];
    onSelectExercise?: (id: string) => void;
}

/**
 * Plain-language answers first. Each card states one finding; tone is shown by
 * icon and label as well as colour, so it never relies on colour alone.
 */
function Highlights({ items, onSelectExercise }: HighlightsProps) {
    if (!items.length) return null;
    return (
        <ul className="highlights" aria-label="Highlights">
            {items.map(h => {
                const Icon = h.tone === "attention" && h.kind !== "stalled" ? AlertTriangle : ICONS[h.kind];
                const clickable = !!(h.exerciseId && onSelectExercise);
                const body = (
                    <>
                        <span className={`highlight-icon tone-${h.tone}`} aria-hidden><Icon size={16} /></span>
                        <span className="highlight-text">
                            {TONE_LABEL[h.tone] && <span className={`highlight-tone tone-${h.tone}`}>{TONE_LABEL[h.tone]}</span>}
                            <span className="highlight-title">{h.title}</span>
                            <span className="highlight-detail">{h.detail}</span>
                        </span>
                    </>
                );
                return (
                    <li key={h.id}>
                        {clickable ? (
                            <button type="button" className="highlight" onClick={() => onSelectExercise!(h.exerciseId!)}>
                                {body}
                                <span className="highlight-link">View exercise</span>
                            </button>
                        ) : (
                            <div className="highlight">{body}</div>
                        )}
                    </li>
                );
            })}
        </ul>
    );
}

export default Highlights;
