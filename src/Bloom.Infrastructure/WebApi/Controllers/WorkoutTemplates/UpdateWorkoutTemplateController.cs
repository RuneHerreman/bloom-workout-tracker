using System.ComponentModel.DataAnnotations;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.WorkoutTemplates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.WorkoutTemplates;

public sealed record UpdateWorkoutTemplateRequest(
    [FromRoute] Guid TemplateId,
    [FromBody] UpdateWorkoutTemplateBody Body,
    [FromServices] IUseCase<UpdateWorkoutTemplateInput, UpdateWorkoutTemplateOutput> UseCase
);

public sealed record UpdateWorkoutTemplateBody(
    [Required]
    [MaxLength(100)]
    string Name,

    [Required]
    List<TemplateExerciseBody> Exercises
);

public sealed record UpdateWorkoutTemplateResponse(Guid WorkoutTemplateId);

public static class UpdateWorkoutTemplateController
{
    public static async Task<Results<Ok<UpdateWorkoutTemplateResponse>, BadRequest>> Invoke(
        [AsParameters] UpdateWorkoutTemplateRequest request
    )
    {
        var output = await request.UseCase.Execute(new UpdateWorkoutTemplateInput(
            request.TemplateId,
            request.Body.Name,
            request.Body.Exercises.Select(e => e.ToInput()).ToList()
        ));

        return TypedResults.Ok(new UpdateWorkoutTemplateResponse(output.WorkoutTemplateId));
    }
}
