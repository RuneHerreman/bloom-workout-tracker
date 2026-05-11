using System.ComponentModel.DataAnnotations;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.LoggedWorkouts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.LoggedWorkouts;

public sealed record UpdateLoggedWorkoutRequest(
    [FromRoute] Guid LoggedWorkoutId,
    [FromBody] UpdateLoggedWorkoutBody Body,
    [FromServices] IUseCase<UpdateLoggedWorkoutInput, UpdateLoggedWorkoutOutput> UseCase
);

public sealed record UpdateLoggedWorkoutBody(
    [Required, MinLength(1), MaxLength(200)] string Name,

    [Required]
    DateTime LoggedAt,

    [Required]
    [MinLength(1, ErrorMessage = "At least one exercise is required")]
    List<LoggedExerciseBody> Exercises,

    string? Note = null
);

public sealed record UpdateLoggedWorkoutResponse(Guid LoggedWorkoutId);

public static class UpdateLoggedWorkoutController
{
    public static async Task<Results<Ok<UpdateLoggedWorkoutResponse>, BadRequest>> Invoke(
        [AsParameters] UpdateLoggedWorkoutRequest request
    )
    {
        var output = await request.UseCase.Execute(new UpdateLoggedWorkoutInput(
            request.LoggedWorkoutId,
            request.Body.Name,
            request.Body.LoggedAt,
            request.Body.Exercises.Select(e => e.ToInput()).ToList(),
            request.Body.Note
        ));

        return TypedResults.Ok(new UpdateLoggedWorkoutResponse(output.LoggedWorkoutId));
    }
}
