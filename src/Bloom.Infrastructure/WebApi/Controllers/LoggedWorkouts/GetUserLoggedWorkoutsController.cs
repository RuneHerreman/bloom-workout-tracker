using Bloom.Application.Contracts.Ports;
using Bloom.Application.LoggedWorkouts;
using Bloom.Infrastructure.WebApi.Controllers.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.LoggedWorkouts;

public sealed record GetUserLoggedWorkoutsRequest(
    [FromQuery] Guid UserId,
    [FromServices] IUseCase<FindUserLoggedWorkoutsInput, FindUserLoggedWorkoutsOutput> UseCase
);

public static class GetUserLoggedWorkoutsController
{
    public static async Task<Results<Ok<List<LoggedWorkout>>, BadRequest>> Invoke(
        [AsParameters] GetUserLoggedWorkoutsRequest request
    )
    {
        var output = await request.UseCase.Execute(new FindUserLoggedWorkoutsInput(request.UserId));

        return TypedResults.Ok(
            output.Logs
                .Select(l => l.ToResponse())
                .ToList()
        );
    }
}