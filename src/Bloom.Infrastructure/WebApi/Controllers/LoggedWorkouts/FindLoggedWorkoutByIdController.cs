using Bloom.Application.Contracts.Ports;
using Bloom.Application.LoggedWorkouts;
using Bloom.Infrastructure.WebApi.Controllers.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.LoggedWorkouts;

public sealed record FindLoggedWorkoutByIdRequest(
    [FromRoute] Guid LoggedWorkoutId,
    [FromServices] IUseCase<FindLoggedWorkoutByIdInput, FindLoggedWorkoutByIdOutput> UseCase
);

public static class FindLoggedWorkoutByIdController
{
    public static async Task<Results<Ok<LoggedWorkout>, BadRequest>> Invoke(
        [AsParameters] FindLoggedWorkoutByIdRequest request,
        CancellationToken ct
    )
    {
        var output = await request.UseCase.Execute(new FindLoggedWorkoutByIdInput(request.LoggedWorkoutId), ct);

        return TypedResults.Ok(output.Log.ToResponse());
    }
}