using Bloom.Application.Common;
using Bloom.Application.Common.Security;
using Bloom.Domain.Entity;
using Bloom.Domain.Repositories;
using MediatR;

namespace Bloom.Application.Commands;

public record RegisterUserCommand(
    string Email,
    string Name,
    string Password,
    decimal Height,
    decimal Weight,
    int ActiveDays
) : IRequest<Result<Guid>>;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    private readonly IUserRepository _userRepository;

    public RegisterUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken ct)
    {
        // Check if user already exists.
        var existingUser = await _userRepository.GetUserByEmail(command.Email, ct);
        
        if (existingUser != null)
        {
            return Result<Guid>.Failure("User with this email already exists.");
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
        return Result<Guid>.Success(user.Id);
    }
}