using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.Shared;
using Bloom.Domain.WorkoutTemplates.ValueObjects;

namespace Bloom.Domain.WorkoutTemplates;

public readonly record struct PlannedSetId(Guid Value) : IEntityId;

public class PlannedSet : Entity<PlannedSetId>
{
    public ExerciseType Type { get; private set; }
    public int Order { get; private set; } 
    // Cardio
    public PlannedDuration? Duration { get; private set; }
    public PlannedDistance? Distance { get; private set; }

    // Strength / Plyo
    public PlannedReps? Reps { get; private set; }

    private PlannedSet() { }

    private PlannedSet(PlannedSetId id, ExerciseType type, int order) : base(id)
    {
        Type = type;
        Order = order;
    } 

    public static PlannedSet CreateCardio(int order, TimeSpan duration, decimal distance, PlannedDistanceUnit unit)
    {
        var set = new PlannedSet(EntityId.New<PlannedSetId>(), ExerciseType.Cardio, order)
        {
            Duration = PlannedDuration.Create(duration),
            Distance = PlannedDistance.Create(distance, unit),
            Reps = null
        };
        set.ValidateState();
        return set;
    }

    public static PlannedSet CreateStrengthLike(ExerciseType type, int order, int reps)
    {
        if (type is not (ExerciseType.Strength or ExerciseType.Plyometric))
            throw new ArgumentOutOfRangeException(nameof(type), type, "Type must be Strength or Plyometric.");

        var set = new PlannedSet(EntityId.New<PlannedSetId>(), type, order)
        {
            Reps = PlannedReps.Create(reps),
            Duration = null,
            Distance = null
        };
        set.ValidateState();
        return set;
    }

    public override void ValidateState()
    {
        Asserts.EnsureNotNegative(Order);

        switch (Type)
        {
            case ExerciseType.Cardio:
                Asserts.EnsureNotEmpty(Duration);
                Asserts.EnsureNotEmpty(Distance);
                Asserts.EnsureTrue(Reps is null);
                break;

            case ExerciseType.Strength:
            case ExerciseType.Plyometric:
                Asserts.EnsureNotEmpty(Reps);
                Asserts.EnsureTrue(Duration is null);
                Asserts.EnsureTrue(Distance is null);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported ExerciseType for PlannedSet.");
        }
    }
}