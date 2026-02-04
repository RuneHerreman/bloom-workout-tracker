using System.Security.Claims;
using System.Text;
using Bloom.Application.Common;
using Bloom.Domain.Entity;
using Bloom.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Bloom.Application.Commands;
public record LoginCommand(string Email, string Password) : IRequest<Result<string>>;
public class LoginHandler : IRequestHandler<LoginCommand, Result<string>>
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public LoginHandler(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<Result<string>> Handle(LoginCommand command, CancellationToken ct)
    {
        // Find user
        var user = await _userRepository.GetUserByEmail(command.Email, ct);
        if (user == null)
            return Result<string>.Failure("Invalid email or password");

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash))
            return Result<string>.Failure("Invalid email or password");

        // Generate JWT
        var token = GenerateJwtToken(user);
        return Result<string>.Success(token);
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}