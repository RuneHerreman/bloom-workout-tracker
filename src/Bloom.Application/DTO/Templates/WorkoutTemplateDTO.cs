
namespace Bloom.Application.DTO.Templates;

public record WorkoutTemplateDTO
(
    Guid Id,
    string Name,
    List<WorkoutTemplateExerciseDTO> Exercises
);