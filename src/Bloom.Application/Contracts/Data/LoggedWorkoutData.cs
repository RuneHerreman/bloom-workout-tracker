using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.LoggedWorkouts.ValueObjects;

namespace Bloom.Application.Contracts;

public sealed record LoggedWorkoutData
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public DateTime LoggedAt { get; init; }
    public IReadOnlyList<LoggedExerciseData> LoggedExercises { get; init; } = [];
}

public sealed record LoggedExerciseData
{
    public Guid Id { get; init; }
    public Guid ExerciseId { get; init; }
    public int Order { get; init; }
    public IReadOnlyList<LoggedSetData> Sets { get; init; } = [];
}

public sealed record LoggedSetData
{
    public Guid Id { get; init; }
    public ExerciseType Type { get; init; }
    public int Order { get; init; }
    public TimeSpan? Duration { get; init; }
    public DistanceData? Distance { get; init; }
    public int? Reps { get; init; }
    public WeightData? Weight { get; init; }
    public int? Rir { get; init; }
}

public sealed record DistanceData
{
    public decimal Value { get; init; }
    public DistanceUnit Unit { get; init; }
}

public sealed record WeightData
{
    public decimal Value { get; init; }
    public WeightUnit Unit { get; init; }
}