using Bloom.Application.Contracts.Ports;
using Bloom.Infrastructure.Strava;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Strava;

public sealed record GetStravaAuthUrlRequest(
    [FromServices] StravaApiClient ApiClient,
    [FromServices] ICurrentUser CurrentUser
);

public sealed record GetStravaAuthUrlResponse(string Url);

public static class GetStravaAuthUrlController
{
    public static Ok<GetStravaAuthUrlResponse> Invoke([AsParameters] GetStravaAuthUrlRequest request)
    {
        var state = Convert.ToBase64String(request.CurrentUser.UserId.Value.ToByteArray());
        var url = request.ApiClient.BuildAuthUrl(state);
        return TypedResults.Ok(new GetStravaAuthUrlResponse(url));
    }
}
