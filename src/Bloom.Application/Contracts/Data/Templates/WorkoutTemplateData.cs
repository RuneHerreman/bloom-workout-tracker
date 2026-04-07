
namespace Bloom.Application.Contracts.Data.Templates;

public record WorkoutTemplateData
(
    Guid Id,
    string Name,
    List<WorkoutTemplateExerciseData> Exercises
);