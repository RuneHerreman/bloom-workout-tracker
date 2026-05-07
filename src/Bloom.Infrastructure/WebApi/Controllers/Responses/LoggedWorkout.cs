using Bloom.Application.Contracts;

namespace Bloom.Infrastructure.WebApi.Controllers.Responses;

public record LoggedWorkout(
    Guid Id,
    Guid UserId,
    DateTime LoggedAt,
    IReadOnlyList<LoggedExercise> Exercises
);

public record LoggedExercise(
    Guid ExerciseId,
    int Order,
    IReadOnlyList<LoggedSet> Sets
);

public record LoggedSet(
    string Type,
    int Order,
    TimeSpan? Duration,
    decimal? Distance,
    string? DistanceUnit,
    int? Reps,
    decimal? Weight,
    string? WeightUnit,
    int? Rir
);

public static class LoggedWorkoutExtensions
{
    public static LoggedWorkout ToResponse(this LoggedWorkoutData data) =>
        new(
            data.Id,
            data.UserId,
            data.LoggedAt,
            data.Exercises.Select(e => e.ToResponse()).ToList()
        );

    public static LoggedExercise ToResponse(this LoggedExerciseData data) =>
        new(
            data.ExerciseId,
            data.Order,
            data.Sets.Select(s => s.ToResponse()).ToList()
        );

    public static LoggedSet ToResponse(this LoggedSetData data) =>
        new(
            data.Type,
            data.Order,
            data.Duration,
            data.Distance,
            data.DistanceUnit,
            data.Reps,
            data.Weight,
            data.WeightUnit,
            data.Rir
        );
}