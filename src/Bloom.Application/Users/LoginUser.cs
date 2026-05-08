using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Users;
using Bloom.Domain.Users.ValueObjects;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Users;

public sealed record LoginUserInput(
    string Email,
    string Password
);

public sealed record LoginUserOutput(
    Guid UserId,
    string Username,
    string Email,
    string Token
);

public class LoginUser(
    IUnitOfWork uow,
    IPasswordHasher passwordHasher,
    ITokenIssuer tokenIssuer,
    ILogger<LoginUser> logger
) : IUseCase<LoginUserInput, LoginUserOutput>
{
    public async Task<LoginUserOutput> Execute(LoginUserInput input)
    {
        logger.LogInformation("Login attempt | Email: {Email}", input.Email);

        var userRepo = uow.Repo<IUserRepository>();
        var email = Email.Create(input.Email);

        var maybeUser = await userRepo.ByEmail(email);

        if (!maybeUser.HasValue)
            throw new InvalidCredentialsException("Invalid email or password.");

        var user = maybeUser.Value;

        if (!passwordHasher.VerifyHashedPassword(user.HashedPassword.Value, input.Password))
            throw new InvalidCredentialsException("Invalid email or password.");

        var token = tokenIssuer.Issue(user.Id, user.Email, user.Username);

        logger.LogInformation("Login successful | Id: {UserId}", user.Id);

        return new LoginUserOutput(
            user.Id.Value,
            user.Username.Value,
            user.Email.Value,
            token
        );
    }
}
