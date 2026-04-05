using Bloom.Domain.Exercises;
using Bloom.Domain.Shared;

namespace Bloom.Domain.LogBook;

public class LoggedStrengthSet : StrengthSet
{
    public LoggedExerciseId LoggedExerciseId { get; private set; }
    public decimal? Weight { get; private set; }

    public decimal CalculateVolume() => (Weight ?? 0) * Reps;

    // EF Core requires a parameterless constructor
    private LoggedStrengthSet() : base() {}

    private LoggedStrengthSet(
        ExerciseSetId id,
        LoggedExerciseId loggedExerciseId,
        int order,
        int repetitions,
        int rir,
        decimal? weight) : base(id, order, repetitions, rir)
    {
        LoggedExerciseId = loggedExerciseId;
        Weight = weight;
    }

    public static LoggedStrengthSet Create(
        LoggedExerciseId loggedExerciseId,
        int order,
        int repetitions,
        int rir,
        decimal? weight = null,
        ExerciseSetId? id = null)
    {
        var set = new LoggedStrengthSet(
            id ?? EntityId.New<ExerciseSetId>(),
            loggedExerciseId,
            order,
            repetitions,
            rir,
            weight
        );
        set.ValidateState();
        return set;
    }

    public override void ValidateState()
    {
        base.ValidateState();

        if (LoggedExerciseId == default)
            throw new InvalidOperationException("LoggedExerciseId must be set.");

        if (Weight.HasValue && Weight < 0)
            throw new InvalidOperationException("Weight cannot be negative.");
    }
}

public class LoggedCardioSet : CardioSet
{
    public LoggedExerciseId LoggedExerciseId { get; private set; }

    // EF Core requires a parameterless constructor
    private LoggedCardioSet() : base() {}

    private LoggedCardioSet(
        ExerciseSetId id,
        LoggedExerciseId loggedExerciseId,
        TimeOnly duration,
        decimal distance) : base(id, duration, distance)
    {
        LoggedExerciseId = loggedExerciseId;
    }

    public static LoggedCardioSet Create(
        LoggedExerciseId loggedExerciseId,
        TimeOnly duration,
        decimal distance,
        ExerciseSetId? id = null)
    {
        var set = new LoggedCardioSet(
            id ?? EntityId.New<ExerciseSetId>(),
            loggedExerciseId,
            duration,
            distance
        );
        set.ValidateState();
        return set;
    }

    public override void ValidateState()
    {
        base.ValidateState();

        if (LoggedExerciseId == default)
            throw new InvalidOperationException("LoggedExerciseId must be set.");
    }
}