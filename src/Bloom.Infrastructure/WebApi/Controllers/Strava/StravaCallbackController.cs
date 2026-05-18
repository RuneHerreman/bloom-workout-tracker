using Bloom.Application.Contracts.Ports;
using Bloom.Application.Strava;
using Bloom.Infrastructure.Strava;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Bloom.Infrastructure.WebApi.Controllers.Strava;

public sealed record StravaCallbackRequest(
    [FromQuery] string Code,
    [FromQuery] string State,
    [FromQuery] string? Error,
    [FromServices] IUseCase<ConnectStravaInput, ConnectStravaOutput> UseCase,
    [FromServices] ICurrentUser CurrentUser,
    [FromServices] IOptions<StravaOptions> Options,
    [FromServices] IDataProtectionProvider DataProtectionProvider
);

public static class StravaCallbackController
{
    public static async Task<Results<RedirectHttpResult, BadRequest<string>>> Invoke(
        [AsParameters] StravaCallbackRequest request,
        CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(request.Error))
            return TypedResults.BadRequest($"Strava authorization denied: {request.Error}");

        try
        {
            var protector = request.DataProtectionProvider
                .CreateProtector("Strava.OAuth.State")
                .ToTimeLimitedDataProtector();

            var stateUserId = new Guid(protector.Unprotect(request.State));
            if (stateUserId != request.CurrentUser.UserId.Value)
                return TypedResults.BadRequest("Invalid state parameter");
        }
        catch
        {
            return TypedResults.BadRequest("Invalid state parameter");
        }

        await request.UseCase.Execute(new ConnectStravaInput(request.Code), ct);

        var frontendUrl = request.Options.Value.FrontendUrl.TrimEnd('/');
        return TypedResults.Redirect($"{frontendUrl}/profile?strava=connected");
    }
}
