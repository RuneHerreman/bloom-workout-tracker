using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Users;

public sealed record UpdateTechnicalPointsRequest(
    [FromBody] UpdateTechnicalPointsBody Body,
    [FromServices] IUseCase<UpdateTechnicalPointsInput, UpdateTechnicalPointsOutput> UseCase
);

public sealed record UpdateTechnicalPointsBody(string? TechnicalPoints);

public sealed record UpdateTechnicalPointsResponse(Guid UserId);

public static class UpdateTechnicalPointsController
{
    public static async Task<Results<Ok<UpdateTechnicalPointsResponse>, BadRequest>> Invoke(
        [AsParameters] UpdateTechnicalPointsRequest request
    )
    {
        var output = await request.UseCase.Execute(new UpdateTechnicalPointsInput(
            request.Body.TechnicalPoints
        ));

        return TypedResults.Ok(new UpdateTechnicalPointsResponse(output.UserId));
    }
}
