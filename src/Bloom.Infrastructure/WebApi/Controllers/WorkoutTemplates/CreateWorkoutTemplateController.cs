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

public sealed record CreateWorkoutTemplateBody(
    [Required]
    [MaxLength(100)]
    string Name,

    [Required]
    [MinLength(1, ErrorMessage = "At least one exercise is required")]
    List<TemplateExerciseBody> Exercises
);

public sealed record CreateWorkoutTemplateResponse(Guid WorkoutTemplateId);

public static class CreateWorkoutTemplateController
{
    public static async Task<Results<Ok<CreateWorkoutTemplateResponse>, BadRequest>> Invoke(
        [AsParameters] CreateWorkoutTemplateRequest request
    )
    {
        var output = await request.UseCase.Execute(new CreateWorkoutTemplateInput(
            request.Body.Name,
            request.Body.Exercises.Select(e => e.ToInput()).ToList()
        ));

        return TypedResults.Ok(new CreateWorkoutTemplateResponse(output.WorkoutTemplateId));
    }
}
