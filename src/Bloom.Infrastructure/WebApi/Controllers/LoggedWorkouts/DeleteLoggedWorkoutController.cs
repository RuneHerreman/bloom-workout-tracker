using Bloom.Application.Contracts.Ports;
using Bloom.Application.LoggedWorkouts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.LoggedWorkouts;

public sealed record DeleteLoggedWorkoutRequest(
    [FromRoute] Guid LoggedWorkoutId,
    [FromQuery] Guid UserId,
    [FromServices] IUseCase<DeleteLoggedWorkoutInput> UseCase
);

public static class DeleteLoggedWorkoutController
{
    public static async Task<Results<NoContent, BadRequest>> Invoke(
        [AsParameters] DeleteLoggedWorkoutRequest request
    )
    {
        await request.UseCase.Execute(new DeleteLoggedWorkoutInput(
            request.LoggedWorkoutId,
            request.UserId
        ));

        return TypedResults.NoContent();
    }
}