using Bloom.Application.Contracts.Ports;
using Bloom.Application.Strava;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Strava;

public sealed record SyncStravaRequest(
    [FromServices] IUseCase<SyncStravaActivitiesInput, SyncStravaActivitiesOutput> UseCase
);

public sealed record SyncStravaResponse(int Imported);

public static class SyncStravaController
{
    public static async Task<Ok<SyncStravaResponse>> Invoke(
        [AsParameters] SyncStravaRequest request,
        CancellationToken ct)
    {
        var output = await request.UseCase.Execute(new SyncStravaActivitiesInput(), ct);
        return TypedResults.Ok(new SyncStravaResponse(output.Imported));
    }
}
