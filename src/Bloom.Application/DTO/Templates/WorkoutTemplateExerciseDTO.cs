namespace Bloom.Application.DTO.Templates;

public record WorkoutTemplateExerciseDTO
(
    Guid ExerciseId,
    int Order,
    List<TemplateExerciseSetDTO> Sets
);
    