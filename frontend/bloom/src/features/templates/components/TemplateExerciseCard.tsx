import type { TemplateExercise } from "../../../assets/js/data/apiTypes.ts";
import type { Exercise } from "../../exercises/api.ts";
import Button from "../../../components/general/ButtonComponent.tsx";

interface TemplateExerciseCardProps {
    exercise: TemplateExercise;
    exerciseInfo?: Exercise;
}
function TemplateExerciseCard({ exercise, exerciseInfo }: TemplateExerciseCardProps) {
  return (
    <div className="template-exercise-card">
        <header>
            <div>
                <h3 className={"detail-exercise-name"}>{exerciseInfo?.name}</h3>
                <p className={"detail-exercise-info"}>{exerciseInfo?.type} · {exerciseInfo?.targetMuscles.join(" - ")}</p>
            </div>
        </header>
        <section className={"detail-body"}>
            <div className={"set-grid-header"}>
                <p>Set</p>
                {
                    exerciseInfo?.type === "Strength" ? (
                        <>
                            <p>Reps</p>
                        </>
                    ) : (
                        <>
                            <p>Distance</p>
                            <p>Duration</p>
                        </>
                    )
                }
            </div>
            {exercise.sets.sort((a, b) => a.order - b.order).map((set, index) => (
                <div key={index} className={"set-row"}>
                    <p>{set.order}</p>
                    {
                        exerciseInfo?.type === "Strength" ? (
                            <>
                                <input value={set.reps?.toString()}></input>
                            </>
                        ) : (
                            <>
                                <input value={`${set.distance} ${set.distanceUnit}`}></input>
                                <input value={set.duration?.toString()}></input>
                            </>
                        )
                    }
                </div>
            ))}
        </section>
        <section className={"detail-footer"}>
            <Button text={"Add set"} style={"modern"} icon="+"/>
        </section>
    </div>
  );
}

export default TemplateExerciseCard;