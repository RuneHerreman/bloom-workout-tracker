using Bloom.Application.Contracts.Ports;
using Bloom.Application.Strava;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Strava;

public sealed record DisconnectStravaRequest(
    [FromServices] IUseCase<DisconnectStravaInput> UseCase
);

public static class DisconnectStravaController
{
    public static async Task<NoContent> Invoke(
        [AsParameters] DisconnectStravaRequest request,
        CancellationToken ct)
    {
        await request.UseCase.Execute(new DisconnectStravaInput(), ct);
        return TypedResults.NoContent();
    }
}
