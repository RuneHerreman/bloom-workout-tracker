namespace Bloom.Application.DTO.Templates;

public record WorkoutTemplateExerciseDTO
(
    Guid ExerciseId,
    string Name,
    int Order,
    List<TemplateExerciseSetDTO> Sets
);
    