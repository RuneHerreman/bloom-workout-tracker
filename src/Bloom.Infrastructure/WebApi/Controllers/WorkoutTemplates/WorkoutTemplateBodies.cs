using System.ComponentModel.DataAnnotations;
using Bloom.Application.WorkoutTemplates;

namespace Bloom.Infrastructure.WebApi.Controllers.WorkoutTemplates;

public sealed record TemplateExerciseBody(
    [Required]
    Guid ExerciseId,

    [Range(0, int.MaxValue)]
    int Order,

    [Required]
    [MinLength(1, ErrorMessage = "At least one set is required")]
    List<PlannedSetBody> Sets
);

public sealed record PlannedSetBody(
    [Required]
    [RegularExpression("^(Strength|Cardio|Plyometric)$", ErrorMessage = "Type must be Strength, Cardio, or Plyometric")]
    string Type,

    [Range(0, int.MaxValue)]
    int Order,

    [Range(1, int.MaxValue, ErrorMessage = "Reps must be greater than 0")]
    int? Reps,

    TimeSpan? Duration,

    [Range(0.01, double.MaxValue, ErrorMessage = "Distance must be greater than 0")]
    decimal? Distance,

    [RegularExpression("^(Km|Miles)$", ErrorMessage = "DistanceUnit must be Km or Miles")]
    string? DistanceUnit
);

internal static class WorkoutTemplateBodyExtensions
{
    internal static TemplateExerciseInput ToInput(this TemplateExerciseBody body) =>
        new(
            body.ExerciseId,
            body.Order,
            body.Sets.Select(s => s.ToInput()).ToList()
        );

    internal static PlannedSetInput ToInput(this PlannedSetBody body) =>
        new(
            body.Type,
            body.Order,
            body.Reps,
            body.Duration,
            body.Distance,
            body.DistanceUnit
        );
}