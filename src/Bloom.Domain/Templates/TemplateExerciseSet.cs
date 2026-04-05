using Bloom.Domain.Exercises;
using Bloom.Domain.Shared;

namespace Bloom.Domain.Templates;

public class TemplateStrengthSet : StrengthSet
{
    public WorkoutTemplateExerciseId WorkoutTemplateExerciseId { get; private set; }

    // EF Core requires a parameterless constructor
    private TemplateStrengthSet() : base() {}

    private TemplateStrengthSet(
        ExerciseSetId id,
        WorkoutTemplateExerciseId workoutTemplateExerciseId,
        int order,
        int repetitions,
        int rir) : base(id, order, repetitions, rir)
    {
        WorkoutTemplateExerciseId = workoutTemplateExerciseId;
    }

    public static TemplateStrengthSet Create(
        WorkoutTemplateExerciseId workoutTemplateExerciseId,
        int order,
        int repetitions,
        int rir,
        ExerciseSetId? id = null)
    {
        var set = new TemplateStrengthSet(
            id ?? EntityId.New<ExerciseSetId>(),
            workoutTemplateExerciseId,
            order,
            repetitions,
            rir
        );
        set.ValidateState();
        return set;
    }

    public override void ValidateState()
    {
        base.ValidateState();

        if (WorkoutTemplateExerciseId == default)
            throw new InvalidOperationException("WorkoutTemplateExerciseId must be set.");
    }
}

public class TemplateCardioSet : CardioSet
{
    public WorkoutTemplateExerciseId WorkoutTemplateExerciseId { get; private set; }

    // EF Core requires a parameterless constructor
    private TemplateCardioSet() : base() {}

    private TemplateCardioSet(
        ExerciseSetId id,
        WorkoutTemplateExerciseId workoutTemplateExerciseId,
        TimeOnly duration,
        decimal distance) : base(id, duration, distance)
    {
        WorkoutTemplateExerciseId = workoutTemplateExerciseId;
    }

    public static TemplateCardioSet Create(
        WorkoutTemplateExerciseId workoutTemplateExerciseId,
        TimeOnly duration,
        decimal distance,
        ExerciseSetId? id = null)
    {
        var set = new TemplateCardioSet(
            id ?? EntityId.New<ExerciseSetId>(),
            workoutTemplateExerciseId,
            duration,
            distance
        );
        set.ValidateState();
        return set;
    }

    public override void ValidateState()
    {
        base.ValidateState();

        if (WorkoutTemplateExerciseId == default)
            throw new InvalidOperationException("WorkoutTemplateExerciseId must be set.");
    }
}