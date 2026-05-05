using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.LoggedWorkouts.ValueObjects;
using Bloom.Domain.Shared;

namespace Bloom.Domain.LoggedWorkouts;

public readonly record struct LoggedSetId(Guid Value) : IEntityId;

public class LoggedSet : Entity<LoggedSetId>
{
    public ExerciseType Type { get; private set; }

    // Cardio fields (required when Type == Cardio)
    public Duration? Duration { get; private set; }
    public Distance? Distance { get; private set; }

    // Strength-like fields (required when Type == Strength or Plyometric)
    public Reps? Reps { get; private set; }
    public Weight? Weight { get; private set; }
    public RIR? Rir { get; private set; }

    private LoggedSet() { }

    private LoggedSet(LoggedSetId id, ExerciseType type) : base(id)
    {
        Type = type;
    }

    public static LoggedSet CreateCardio(TimeSpan duration, decimal distance, DistanceUnit unit)
    {
        var set = new LoggedSet(EntityId.New<LoggedSetId>(), ExerciseType.Cardio)
        {
            Duration = ValueObjects.Duration.Create(duration),
            Distance = ValueObjects.Distance.Create(distance, unit),

            Reps = null,
            Weight = null,
            Rir = null
        };

        set.ValidateState();
        return set;
    }

    public static LoggedSet CreateStrength(int reps, decimal weight, WeightUnit unit, int rir)
        => CreateStrengthLike(ExerciseType.Strength, reps, weight, unit, rir);

    public static LoggedSet CreatePlyometric(int reps, decimal weight, WeightUnit unit, int rir)
        => CreateStrengthLike(ExerciseType.Plyometric, reps, weight, unit, rir);

    private static LoggedSet CreateStrengthLike(
        ExerciseType type,
        int reps,
        decimal weight,
        WeightUnit unit,
        int rir)
    {
        if (type is not (ExerciseType.Strength or ExerciseType.Plyometric))
            throw new ArgumentOutOfRangeException(nameof(type), type, "Type must be Strength or Plyometric.");

        var set = new LoggedSet(EntityId.New<LoggedSetId>(), type)
        {
            Reps = ValueObjects.Reps.Create(reps),
            Weight = ValueObjects.Weight.Create(weight, unit),
            Rir = ValueObjects.RIR.Create(rir),

            Duration = null,
            Distance = null
        };

        set.ValidateState();
        return set;
    }

    public override void ValidateState()
    {
        switch (Type)
        {
            case ExerciseType.Cardio:
                Asserts.EnsureNotEmpty(Duration);
                Asserts.EnsureNotEmpty(Distance);

                Asserts.EnsureTrue(Reps is null);
                Asserts.EnsureTrue(Weight is null);
                Asserts.EnsureTrue(Rir is null);
                break;

            case ExerciseType.Strength:
            case ExerciseType.Plyometric:
                Asserts.EnsureNotEmpty(Reps);
                Asserts.EnsureNotEmpty(Weight);
                Asserts.EnsureNotEmpty(Rir);

                Asserts.EnsureTrue(Duration is null);
                Asserts.EnsureTrue(Distance is null);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported ExerciseType for LoggedSet.");
        }
    }

}