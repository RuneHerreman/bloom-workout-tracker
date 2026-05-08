using Bloom.Application.Contracts.Ports;
using Bloom.Application.WorkoutTemplates;
using Bloom.Infrastructure.WebApi.Controllers.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.WorkoutTemplates;

public sealed record GetUserWorkoutTemplatesRequest(
    [FromQuery] string? Name,
    [FromServices] IUseCase<FindUserWorkoutTemplatesInput, FindUserWorkoutTemplatesOutput> UseCase
);

public static class GetUserWorkoutTemplatesController
{
    public static async Task<Results<Ok<List<WorkoutTemplate>>, BadRequest>> Invoke(
        [AsParameters] GetUserWorkoutTemplatesRequest request
    )
    {
        var output = await request.UseCase.Execute(new FindUserWorkoutTemplatesInput(
            request.Name
        ));

        return TypedResults.Ok(
            output.Templates
                .Select(t => t.ToResponse())
                .ToList()
        );
    }
}
