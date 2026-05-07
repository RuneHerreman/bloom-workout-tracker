namespace Bloom.Application.Contracts;

public sealed record LoggedWorkoutData
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public DateTime LoggedAt { get; init; }
    public IReadOnlyList<LoggedExerciseData> Exercises { get; init; } = [];
}

public sealed record LoggedExerciseData
{
    public Guid ExerciseId { get; init; }
    public int Order { get; init; }
    public IReadOnlyList<LoggedSetData> Sets { get; init; } = [];
}

public sealed record LoggedSetData
{
    public string Type { get; init; } = string.Empty;
    public int Order { get; init; }
    public TimeSpan? Duration { get; init; }
    public decimal? Distance { get; init; }
    public string? DistanceUnit { get; init; }
    public int? Reps { get; init; }
    public decimal? Weight { get; init; }
    public string? WeightUnit { get; init; }
    public int? Rir { get; init; }
}