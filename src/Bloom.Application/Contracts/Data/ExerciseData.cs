namespace Bloom.Application.Contracts.Data;

public record ExerciseData
(
    Guid Id,
    string Name,
    string Description,
    string Type,
    string PrimaryMuscleGroup
);