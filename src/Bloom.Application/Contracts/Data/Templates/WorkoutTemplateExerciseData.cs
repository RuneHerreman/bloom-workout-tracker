namespace Bloom.Application.Contracts.Data.Templates;

public record WorkoutTemplateExerciseData
(
    Guid ExerciseId,
    string Name,
    int Order,
    List<TemplateExerciseSetData> Sets
);
    