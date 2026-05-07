using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.WorkoutTemplates.ValueObjects;

namespace Bloom.Application.Contracts;

public sealed record WorkoutTemplateData
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<TemplateExerciseData> TemplateExercises { get; init; } = [];
}

public sealed record TemplateExerciseData
{
    public Guid Id { get; init; }
    public Guid ExerciseId { get; init; }
    public int Order { get; init; }
    public IReadOnlyList<PlannedSetData> Sets { get; init; } = [];
}

public sealed record PlannedSetData
{
    public Guid Id { get; init; }
    public ExerciseType Type { get; init; }
    public int Order { get; init; }
    public int? Reps { get; init; }
    public TimeSpan? Duration { get; init; }
    public PlannedDistanceData? Distance { get; init; }
}

public sealed record PlannedDistanceData
{
    public decimal Value { get; init; }
    public PlannedDistanceUnit Unit { get; init; }
}