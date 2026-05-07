namespace Bloom.Application.Contracts;

public sealed record WorkoutTemplateData
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<TemplateExerciseData> Exercises { get; init; } = [];
}

public sealed record TemplateExerciseData
{
    public Guid ExerciseId { get; init; }
    public int Order { get; init; }
    public IReadOnlyList<PlannedSetData> Sets { get; init; } = [];
}

public sealed record PlannedSetData
{
    public string Type { get; init; } = string.Empty;
    public int Order { get; init; }
    public int? Reps { get; init; }
    public TimeSpan? Duration { get; init; }
    public decimal? Distance { get; init; }
    public string? DistanceUnit { get; init; }
}