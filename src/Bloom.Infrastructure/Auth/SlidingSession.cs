using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace Bloom.Infrastructure.Auth;

public static class SessionClaims
{
    public const string Stamp = "sstamp";
    public const string Persistent = "persist";

    public static bool IsPersistent(ClaimsPrincipal principal) =>
        principal.FindFirstValue(Persistent) == "true";
}

/// <summary>
/// Sliding session: once a valid token is past <see cref="JwtOptions.RenewAfter"/>,
/// the request carrying it gets a fresh token and cookie. Active users never
/// expire; idle ones do after the token lifetime. Renewal re-checks the user,
/// so a password change or account deletion ends every older session at its
/// next renewal.
/// </summary>
public static class SlidingSession
{
    /// <summary>
    /// What to renew with when the token is due, or null while it is still
    /// fresh. Throws <see cref="UnauthenticatedException"/> for a due token
    /// without the session claims.
    /// </summary>
    public static RenewSessionInput? DueRenewal(ClaimsPrincipal principal, DateTimeOffset now, JwtOptions options)
    {
        var iat = principal.FindFirstValue(JwtRegisteredClaimNames.Iat);
        if (iat is null || !long.TryParse(iat, out var issuedAtSeconds))
            throw new UnauthenticatedException("Token has no issue time.");

        if (now - DateTimeOffset.FromUnixTimeSeconds(issuedAtSeconds) < options.RenewAfter)
            return null;

        var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var stamp = principal.FindFirstValue(SessionClaims.Stamp);

        if (!Guid.TryParse(sub, out var userId) || stamp is null)
            throw new UnauthenticatedException("Token lacks session claims.");

        return new RenewSessionInput(EntityId.New<UserId>(userId), stamp, SessionClaims.IsPersistent(principal));
    }

    public static async Task OnTokenValidated(TokenValidatedContext context, JwtOptions options)
    {
        if (context.Principal is null) return;

        var services = context.HttpContext.RequestServices;
        var now = (services.GetService<TimeProvider>() ?? TimeProvider.System).GetUtcNow();

        try
        {
            var renewal = DueRenewal(context.Principal, now, options);
            if (renewal is null) return;

            var useCase = services.GetRequiredService<IUseCase<RenewSessionInput, RenewSessionOutput>>();
            var output = await useCase.Execute(renewal, context.HttpContext.RequestAborted);

            // Only cookie sessions slide; a bearer header (API docs) keeps its own expiry.
            if (context.Request.Cookies.ContainsKey(options.CookieName))
                AuthCookie.Append(context.HttpContext, options, output.Token, renewal.Persistent, now);
        }
        catch (UnauthenticatedException ex)
        {
            AuthCookie.Delete(context.HttpContext, options);
            context.Fail(ex.Message);
        }
    }
}
