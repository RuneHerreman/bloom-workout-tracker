using Bloom.Application.Contracts.Ports;
using Bloom.Application.Strava;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Strava;

public sealed record ImportStravaHistoryRequest(
    [FromServices] IUseCase<ImportAllStravaActivitiesInput, ImportAllStravaActivitiesOutput> UseCase
);

public sealed record ImportStravaHistoryResponse(int Imported);

public static class ImportStravaHistoryController
{
    public static async Task<Ok<ImportStravaHistoryResponse>> Invoke(
        [AsParameters] ImportStravaHistoryRequest request,
        CancellationToken ct)
    {
        var output = await request.UseCase.Execute(new ImportAllStravaActivitiesInput(), ct);
        return TypedResults.Ok(new ImportStravaHistoryResponse(output.Imported));
    }
}
