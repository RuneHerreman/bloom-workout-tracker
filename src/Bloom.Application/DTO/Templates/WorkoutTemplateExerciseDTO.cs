namespace Bloom.Application.DTO.Templates;

public record WorkoutTemplateExerciseDTO
(
    Guid Id,
    Guid ExerciseId,
    int Order,
    List<TemplateExerciseSetDTO> Sets
);
    