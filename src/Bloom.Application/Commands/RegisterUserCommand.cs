using Bloom.Application.Common;
using Bloom.Application.Common.Behaviours;
using Bloom.Application.Common.Security;
using Bloom.Domain.Entity;
using Bloom.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Commands;

public record RegisterUserCommand(
    string Email,
    string Name,
    string Password,
    decimal Height,
    decimal Weight,
    int ActiveDays
) : IRequest<Result<string>>;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<string>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly ILogger<RegisterUserHandler> _logger;
    
    public RegisterUserHandler(
        IUserRepository userRepository,
        IJwtTokenGenerator tokenGenerator, ILogger<RegisterUserHandler> logger)
    {
        _userRepository = userRepository;
        _tokenGenerator = tokenGenerator;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(RegisterUserCommand command, CancellationToken ct)
    {
        // Check if user already exists.
        var existingUser = await _userRepository.GetUserByEmail(command.Email, ct);
        
        
        if (existingUser != null)
        {
            return Result<string>.Failure("User with this email already exists.");
        }
        
        var user = new User(
            Guid.NewGuid(),
            command.Email,
            command.Name,
            Hashing.Hash(command.Password),
            command.Height,
            command.Weight,
            command.ActiveDays
        );
        
        await _userRepository.RegisterUser(user, ct);
        var token = _tokenGenerator.GenerateToken(user);
        _logger.LogInformation("User registered: {user}", user.Email);
        return Result<string>.Success(token);
    }
}