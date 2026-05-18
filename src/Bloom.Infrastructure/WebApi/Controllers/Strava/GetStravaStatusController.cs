using Bloom.Application.Contracts.Ports;
using Bloom.Application.Strava;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Strava;

public sealed record GetStravaStatusRequest(
    [FromServices] IUseCase<GetStravaStatusInput, GetStravaStatusOutput> UseCase
);

public sealed record GetStravaStatusResponse(bool Connected, string? AthleteName, DateTime? ConnectedAt);

public static class GetStravaStatusController
{
    public static async Task<Ok<GetStravaStatusResponse>> Invoke(
        [AsParameters] GetStravaStatusRequest request,
        CancellationToken ct)
    {
        var output = await request.UseCase.Execute(new GetStravaStatusInput(), ct);
        return TypedResults.Ok(new GetStravaStatusResponse(output.Connected, output.AthleteName, output.ConnectedAt));
    }
}
