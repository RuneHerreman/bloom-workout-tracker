using System.ComponentModel.DataAnnotations;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.WorkoutTemplates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.WorkoutTemplates;

public sealed record CreateWorkoutTemplateRequest(
    [FromBody] CreateWorkoutTemplateBody Body,
    [FromServices] IUseCase<CreateWorkoutTemplateInput, CreateWorkoutTemplateOutput> UseCase
);

public static class CreateWorkoutTemplateController
{
    public static async Task<Results<Ok<CreateWorkoutTemplateResponse>, BadRequest>> Invoke(
        [AsParameters] CreateWorkoutTemplateRequest request
    )
    {
        var output = await request.UseCase.Execute(new CreateWorkoutTemplateInput(
            request.Body.UserId,
            request.Body.Name,
            request.Body.Exercises.Select(e => new TemplateExerciseInput(
                e.ExerciseId,
                e.Order,
                e.Sets.Select(s => new PlannedSetInput(
                    s.Type,
                    s.Order,
                    s.Reps,
                    s.Duration,
                    s.Distance,
                    s.DistanceUnit
                )).ToList()
            )).ToList()
        ));

        return TypedResults.Ok(new CreateWorkoutTemplateResponse(output.WorkoutTemplateId));
    }
}

public sealed record CreateWorkoutTemplateBody(
    [Required]
    Guid UserId,

    [Required]
    [MaxLength(100)]
    string Name,

    [Required]
    [MinLength(1, ErrorMessage = "At least one exercise is required")]
    List<CreateWorkoutTemplateExerciseBody> Exercises
);

public sealed record CreateWorkoutTemplateExerciseBody(
    [Required]
    Guid ExerciseId,

    [Range(0, int.MaxValue)]
    int Order,

    [Required]
    [MinLength(1, ErrorMessage = "At least one set is required")]
    List<CreateWorkoutTemplatePlannedSetBody> Sets
);

public sealed record CreateWorkoutTemplatePlannedSetBody(
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

public sealed record CreateWorkoutTemplateResponse(Guid WorkoutTemplateId);