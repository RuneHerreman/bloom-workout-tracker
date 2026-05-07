using Bloom.Application.Contracts;

namespace Bloom.Infrastructure.WebApi.Controllers.Responses;

public record WorkoutTemplate(
    Guid Id,
    Guid UserId,
    string Name,
    IReadOnlyList<TemplateExercise> Exercises
);

public record TemplateExercise(
    Guid ExerciseId,
    int Order,
    IReadOnlyList<PlannedSet> Sets
);

public record PlannedSet(
    string Type,
    int Order,
    int? Reps,
    TimeSpan? Duration,
    decimal? Distance,
    string? DistanceUnit
);

public static class WorkoutTemplateExtensions
{
    public static WorkoutTemplate ToResponse(this WorkoutTemplateData data) =>
        new(
            data.Id,
            data.UserId,
            data.Name,
            data.Exercises.Select(e => e.ToResponse()).ToList()
        );

    public static TemplateExercise ToResponse(this TemplateExerciseData data) =>
        new(
            data.ExerciseId,
            data.Order,
            data.Sets.Select(s => s.ToResponse()).ToList()
        );

    public static PlannedSet ToResponse(this PlannedSetData data) =>
        new(
            data.Type,
            data.Order,
            data.Reps,
            data.Duration,
            data.Distance,
            data.DistanceUnit
        );
}