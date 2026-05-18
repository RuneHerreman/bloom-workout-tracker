using Bloom.Application.Contracts.Ports;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Strava;

public sealed record GetStravaAuthUrlRequest(
    [FromServices] IStravaClient StravaClient,
    [FromServices] ICurrentUser CurrentUser,
    [FromServices] IDataProtectionProvider DataProtectionProvider
);

public sealed record GetStravaAuthUrlResponse(string Url);

public static class GetStravaAuthUrlController
{
    public static Ok<GetStravaAuthUrlResponse> Invoke([AsParameters] GetStravaAuthUrlRequest request)
    {
        var protector = request.DataProtectionProvider
            .CreateProtector("Strava.OAuth.State")
            .ToTimeLimitedDataProtector();

        var state = protector.Protect(
            request.CurrentUser.UserId.Value.ToString(),
            lifetime: TimeSpan.FromMinutes(10));

        var url = request.StravaClient.BuildAuthUrl(state);
        return TypedResults.Ok(new GetStravaAuthUrlResponse(url));
    }
}
