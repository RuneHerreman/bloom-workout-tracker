using System.Security.Claims;
using System.Text;
using Bloom.Application.Common;
using Bloom.Domain.Entity;
using Bloom.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Bloom.Application.Common.Behaviours;

namespace Bloom.Application.Commands;
public record LoginCommand(string Email, string Password) : IRequest<Result<string>>;
public class LoginHandler : IRequestHandler<LoginCommand, Result<string>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _tokenGenerator;


    public LoginHandler(
        IUserRepository userRepository,
        IJwtTokenGenerator tokenGenerator
    )
    {
        _userRepository = userRepository;
        _tokenGenerator = tokenGenerator;
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
        var token = _tokenGenerator.GenerateToken(user);
        return Result<string>.Success(token);
    }
}