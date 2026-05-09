using System.ComponentModel.DataAnnotations;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.LoggedWorkouts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.LoggedWorkouts;

public sealed record CreateLoggedWorkoutRequest(
    [FromBody] CreateLoggedWorkoutBody Body,
    [FromServices] IUseCase<CreateLoggedWorkoutInput, CreateLoggedWorkoutOutput> UseCase
);

public sealed record CreateLoggedWorkoutBody(
    [Required]
    [MinLength(1, ErrorMessage = "At least one exercise is required")]
    List<LoggedExerciseBody> Exercises,
    DateTime? LoggedAt = null
);

public sealed record CreateLoggedWorkoutResponse(Guid LoggedWorkoutId);

public static class CreateLoggedWorkoutController
{
    public static async Task<Results<Ok<CreateLoggedWorkoutResponse>, BadRequest>> Invoke(
        [AsParameters] CreateLoggedWorkoutRequest request
    )
    {
        var output = await request.UseCase.Execute(new CreateLoggedWorkoutInput(
            request.Body.Exercises.Select(e => e.ToInput()).ToList(),
            request.Body.LoggedAt
        ));

        return TypedResults.Ok(new CreateLoggedWorkoutResponse(output.LoggedWorkoutId));
    }
}
