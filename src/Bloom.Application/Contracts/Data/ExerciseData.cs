using Bloom.Domain.Exercises.ValueObjects;

namespace Bloom.Application.Contracts;

public sealed record ExerciseData
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public IReadOnlyList<TargetMuscleData> TargetMuscles { get; init; } = new List<TargetMuscleData>();
}

public sealed record TargetMuscleData(string Value);