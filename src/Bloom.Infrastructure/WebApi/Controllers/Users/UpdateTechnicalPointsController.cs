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

public static class UpdateTechnicalPointsController
{
    public static async Task<NoContent> Invoke(
        [AsParameters] UpdateTechnicalPointsRequest request
    )
    {
        await request.UseCase.Execute(new UpdateTechnicalPointsInput(
            request.Body.TechnicalPoints
        ));

        return TypedResults.NoContent();
    }
}
