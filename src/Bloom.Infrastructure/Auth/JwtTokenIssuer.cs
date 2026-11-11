using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bloom.Application.Contracts.Ports;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Bloom.Infrastructure.Auth;

public sealed class JwtTokenIssuer(IOptions<JwtOptions> options, TimeProvider clock) : ITokenIssuer
{
    private readonly JwtOptions _options = options.Value;

    public string Issue(SessionTicket ticket)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = clock.GetUtcNow();

        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, ticket.UserId.Value.ToString()),
            new(JwtRegisteredClaimNames.Email, ticket.Email.Value),
            new(JwtRegisteredClaimNames.Name, ticket.Username.Value),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(SessionClaims.Stamp, ticket.Stamp),
            new(SessionClaims.Persistent, ticket.Persistent ? "true" : "false")
        ];

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: (now + _options.LifetimeFor(ticket.Persistent)).UtcDateTime,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
