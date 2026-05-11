using System.ComponentModel.DataAnnotations;
using Bloom.Application.LoggedWorkouts;

namespace Bloom.Infrastructure.WebApi.Controllers.LoggedWorkouts;

public sealed record LoggedExerciseBody(
    [Required]
    Guid ExerciseId,

    [Range(0, int.MaxValue)]
    int Order,

    [Required]
    [MinLength(1, ErrorMessage = "At least one set is required")]
    List<LoggedSetBody> Sets,

    string? GpxData = null
);

public sealed record LoggedSetBody(
    [Required]
    [RegularExpression("^(Strength|Cardio|Plyometric)$", ErrorMessage = "Type must be Strength, Cardio, or Plyometric")]
    string Type,

    [Range(0, int.MaxValue)]
    int Order,

    TimeSpan? Duration,

    [Range(0.01, double.MaxValue, ErrorMessage = "Distance must be greater than 0")]
    decimal? Distance,

    [RegularExpression("^(Km|Miles)$", ErrorMessage = "DistanceUnit must be Km or Miles")]
    string? DistanceUnit,

    [Range(1, int.MaxValue, ErrorMessage = "Reps must be greater than 0")]
    int? Reps,

    [Range(0.01, double.MaxValue, ErrorMessage = "Weight must be greater than 0")]
    decimal? Weight,

    [RegularExpression("^(Kg|Lbs)$", ErrorMessage = "WeightUnit must be Kg or Lbs")]
    string? WeightUnit,

    [Range(0, 10, ErrorMessage = "RIR must be between 0 and 10")]
    int? Rir
);

internal static class LoggedWorkoutBodyExtensions
{
    internal static LoggedExerciseInput ToInput(this LoggedExerciseBody body) =>
        new(
            body.ExerciseId,
            body.Order,
            body.Sets.Select(s => s.ToInput()).ToList(),
            body.GpxData
        );

    internal static LoggedSetInput ToInput(this LoggedSetBody body) =>
        new(
            body.Type,
            body.Order,
            body.Duration,
            body.Distance,
            body.DistanceUnit,
            body.Reps,
            body.Weight,
            body.WeightUnit,
            body.Rir
        );
}
