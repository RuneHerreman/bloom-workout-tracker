using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.LoggedWorkouts.Enums;
using Bloom.Domain.LoggedWorkouts.ValueObjects;
using Bloom.Domain.Shared;

namespace Bloom.Domain.LoggedWorkouts;

public readonly record struct LoggedSetId(Guid Value) : IEntityId;

public class LoggedSet : Entity<LoggedSetId>
{
    public ExerciseType Type { get; private set; }
    public int Order { get; private set; }
    public SetMarker? Marker { get; private set; }

    // Cardio fields (required when Type == Cardio)
    public Duration? Duration { get; private set; }
    public Distance? Distance { get; private set; }

    // Strength-like fields (required when Type == Strength or Plyometric)
    public Reps? Reps { get; private set; }
    public Weight? Weight { get; private set; }
    public RIR? Rir { get; private set; }

    private LoggedSet() { }

    private LoggedSet(LoggedSetId id, ExerciseType type, int order) : base(id)
    {
        Type = type;
        Order = order;
    }

    /// <summary>
    /// A cardio set records a duration, a distance, both, or neither: bouldering has
    /// no distance, an untimed run has no duration, and "went snowboarding" has
    /// neither. Zero or missing values are stored as absent.
    /// </summary>
    public static LoggedSet CreateCardio(int order, TimeSpan? duration, decimal? distance, DistanceUnit? unit, SetMarker? marker = null)
    {
        var set = new LoggedSet(EntityId.New<LoggedSetId>(), ExerciseType.Cardio, order)
        {
            Duration = duration is { } d && d > TimeSpan.Zero ? ValueObjects.Duration.Create(d) : null,
            Distance = distance is { } km && km > 0 ? ValueObjects.Distance.Create(km, unit ?? DistanceUnit.Km) : null,
            Marker = marker,

            Reps = null,
            Weight = null,
            Rir = null
        };

        set.ValidateState();
        return set;
    }

    public static LoggedSet CreateStrength(int order, int reps, decimal weight, WeightUnit unit, int? rir, SetMarker? marker = null)
        => CreateStrengthLike(ExerciseType.Strength, order, reps, weight, unit, rir, marker);

    public static LoggedSet CreatePlyometric(int order, int reps, decimal weight, WeightUnit unit, int? rir, SetMarker? marker = null)
        => CreateStrengthLike(ExerciseType.Plyometric, order, reps, weight, unit, rir, marker);

    private static LoggedSet CreateStrengthLike(
        ExerciseType type,
        int order,
        int reps,
        decimal weight,
        WeightUnit unit,
        int? rir,
        SetMarker? marker = null)
    {
        if (type is not (ExerciseType.Strength or ExerciseType.Plyometric))
            throw new ArgumentOutOfRangeException(nameof(type), type, "Type must be Strength or Plyometric.");

        var set = new LoggedSet(EntityId.New<LoggedSetId>(), type, order)
        {
            Reps = ValueObjects.Reps.Create(reps),
            Weight = ValueObjects.Weight.Create(weight, unit),
            Rir = rir.HasValue ? ValueObjects.RIR.Create(rir.Value) : null,
            Marker = marker,

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
                // Duration and distance are both optional (see CreateCardio).
                Asserts.EnsureTrue(Reps is null);
                Asserts.EnsureTrue(Weight is null);
                Asserts.EnsureTrue(Rir is null);
                break;

            case ExerciseType.Strength:
            case ExerciseType.Plyometric:
                Asserts.EnsureNotEmpty(Reps);
                Asserts.EnsureNotEmpty(Weight);

                Asserts.EnsureTrue(Duration is null);
                Asserts.EnsureTrue(Distance is null);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported ExerciseType for LoggedSet.");
        }
    }

}