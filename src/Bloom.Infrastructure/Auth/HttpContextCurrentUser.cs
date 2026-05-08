using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Microsoft.AspNetCore.Http;

namespace Bloom.Infrastructure.Auth;

public sealed class HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public UserId UserId
    {
        get
        {
            var principal = httpContextAccessor.HttpContext?.User
                ?? throw new InvalidOperationException("No HTTP context available; request is not authenticated.");

            var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("Authenticated principal has no subject claim.");

            if (!Guid.TryParse(sub, out var id))
                throw new InvalidOperationException($"Subject claim '{sub}' is not a valid Guid.");

            return EntityId.New<UserId>(id);
        }
    }
}
