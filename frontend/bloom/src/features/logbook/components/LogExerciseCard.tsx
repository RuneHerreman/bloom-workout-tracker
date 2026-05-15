import type { LoggedExercise } from "../api.ts";
import type { Exercise } from "../../exercises/api.ts";
import { displayDuration } from "../logbookUtils.ts";

interface LogExerciseCardProps {
    exercise: LoggedExercise;
    exerciseInfo?: Exercise;
}

function LogExerciseCard({ exercise, exerciseInfo }: LogExerciseCardProps) {
    const exerciseType = exerciseInfo?.type ?? "Strength";
    const isCardio = exerciseType === "Cardio";
    const bodyClass = `log-body ${isCardio ? "is-cardio" : "is-strength"}`;

    return (
        <div className="log-exercise-card">
            <header>
                <div>
                    <h3 className="log-exercise-name">{exerciseInfo?.name ?? "Unknown exercise"}</h3>
                    <p className="log-exercise-info">{exerciseType} · {exerciseInfo?.targetMuscles.join(" - ")}</p>
                </div>
            </header>
            <section className={bodyClass}>
                <div className="log-set-grid-header">
                    <p>Set</p>
                    {isCardio ? (
                        <><p>Distance</p><p>Duration</p></>
                    ) : (
                        <><p>Reps</p><p>Weight</p><p>RIR</p></>
                    )}
                </div>
                {[...exercise.sets].sort((a, b) => a.order - b.order).map((set, i) => (
                    <div key={i} className="log-set-row">
                        <p>{i + 1}</p>
                        {isCardio ? (
                            <>
                                <p>{set.distance != null ? `${Math.round(set.distance * 1000)} m` : "—"}</p>
                                <p>{displayDuration(set.duration)}</p>
                            </>
                        ) : (
                            <>
                                <p>{set.reps ?? "—"}</p>
                                <p>{set.weight != null ? `${set.weight} ${set.weightUnit ?? ""}`.trim() : "—"}</p>
                                <p>{set.rir != null ? set.rir : "—"}</p>
                            </>
                        )}
                    </div>
                ))}
            </section>
        </div>
    );
}

export default LogExerciseCard;