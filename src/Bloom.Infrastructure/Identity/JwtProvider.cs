using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Users;
using Microsoft.IdentityModel.Tokens;

namespace Bloom.Infrastructure.Identity;

public static class JwtProvider
{
    public static string GenerateToken(string userId, string email)
    {
        // 2. Set up claims
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email)
        };

        // 3. Get key phrase from config/env
        string secretKey = Environment.GetEnvironmentVariable("Jwt__Key") 
            ?? throw new InvalidOperationException("JWT Secret Key is missing from configuration.");
            
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        string issuer = Environment.GetEnvironmentVariable("Jwt__Issuer") 
            ?? "bloom.workout";
            
        string audience = Environment.GetEnvironmentVariable("Jwt__Audience") 
            ?? "users";

        // 4. Create Token
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
