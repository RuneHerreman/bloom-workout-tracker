using Bloom.Domain.Entity;

namespace Bloom.Application.DTO;

public record ExerciseDTO
(
    Guid Id,
    string Name,
    string Description,
    string Type,
    string PrimaryMuscleGroup
);