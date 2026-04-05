using Bloom.Domain.Shared;

namespace Bloom.Domain.Exercises;

public readonly record struct ExerciseSetId(Guid Value) : IEntityId;

public abstract class ExerciseSet : Entity<ExerciseSetId>
{
    // EF Core requires a parameterless constructor
    protected ExerciseSet() {}

    protected ExerciseSet(ExerciseSetId id) : base(id) {}

    public bool IsStrength() => this is StrengthSet;
    public bool IsCardio() => this is CardioSet;
}

public class StrengthSet : ExerciseSet
{
    public int SetOrder { get; protected set; }
    public int Reps { get; protected set; }
    public int RIR { get; protected set; }

    // EF Core requires a parameterless constructor
    protected StrengthSet() {}

    protected StrengthSet(ExerciseSetId id, int order, int repetitions, int rir) : base(id)
    {
        SetOrder = order;
        Reps = repetitions;
        RIR = rir;
    }

    public static StrengthSet Create(int order, int repetitions, int rir, ExerciseSetId? id = null)
    {
        var set = new StrengthSet(
            id ?? EntityId.New<ExerciseSetId>(),
            order,
            repetitions,
            rir
        );
        set.ValidateState();
        return set;
    }

    public override void ValidateState()
    {
        if (SetOrder < 0)
            throw new InvalidOperationException("Set order cannot be negative.");

        if (Reps <= 0)
            throw new InvalidOperationException("Reps must be greater than zero.");

        if (RIR < 0)
            throw new InvalidOperationException("RIR cannot be negative.");
    }
}

public class CardioSet : ExerciseSet
{
    public TimeOnly Duration { get; protected set; }
    public decimal Distance { get; protected set; }

    // EF Core requires a parameterless constructor
    protected CardioSet() {}

    protected CardioSet(ExerciseSetId id, TimeOnly duration, decimal distance) : base(id)
    {
        Duration = duration;
        Distance = distance;
    }

    public static CardioSet Create(TimeOnly duration, decimal distance, ExerciseSetId? id = null)
    {
        var set = new CardioSet(
            id ?? EntityId.New<ExerciseSetId>(),
            duration,
            distance
        );
        set.ValidateState();
        return set;
    }

    public override void ValidateState()
    {
        if (Duration == TimeOnly.MinValue && Distance <= 0)
            throw new InvalidOperationException("Either duration or distance must be provided for cardio set.");

        if (Distance < 0)
            throw new InvalidOperationException("Distance cannot be negative.");
    }
}