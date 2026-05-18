using Bloom.Application.Contracts.Ports;
using Bloom.Application.Strava;
using Bloom.Infrastructure.Strava;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Bloom.Infrastructure.WebApi.Controllers.Strava;

public sealed record StravaCallbackRequest(
    [FromQuery] string Code,
    [FromQuery] string State,
    [FromQuery] string? Error,
    [FromServices] StravaApiClient ApiClient,
    [FromServices] IUseCase<ConnectStravaInput, ConnectStravaOutput> UseCase,
    [FromServices] ICurrentUser CurrentUser,
    [FromServices] IOptions<StravaOptions> Options
);

public static class StravaCallbackController
{
    public static async Task<Results<RedirectHttpResult, BadRequest<string>>> Invoke(
        [AsParameters] StravaCallbackRequest request,
        CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(request.Error))
            return TypedResults.BadRequest($"Strava authorization denied: {request.Error}");

        // Verify state matches the current user (CSRF protection)
        try
        {
            var stateBytes = Convert.FromBase64String(request.State);
            var stateUserId = new Guid(stateBytes);
            if (stateUserId != request.CurrentUser.UserId.Value)
                return TypedResults.BadRequest("Invalid state parameter");
        }
        catch
        {
            return TypedResults.BadRequest("Invalid state parameter");
        }

        var tokenResponse = await request.ApiClient.ExchangeCode(request.Code, ct);
        var athlete = tokenResponse.Athlete ?? throw new InvalidOperationException("No athlete data in Strava response");
        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(tokenResponse.ExpiresAt).UtcDateTime;

        await request.UseCase.Execute(new ConnectStravaInput(
            athlete.Id,
            tokenResponse.AccessToken,
            tokenResponse.RefreshToken,
            expiresAt,
            $"{athlete.FirstName} {athlete.LastName}".Trim()
        ), ct);

        var frontendUrl = request.Options.Value.FrontendUrl.TrimEnd('/');
        return TypedResults.Redirect($"{frontendUrl}/profile?strava=connected");
    }
}
