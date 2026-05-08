using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Users;
using Bloom.Domain.Users.ValueObjects;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Users;

public sealed record RegisterUserInput(
    string Email,
    string Username,
    string Password
);

public sealed record RegisterUserOutput(Guid UserId, string Token);

public class RegisterUser(
    IUnitOfWork uow,
    IPasswordHasher passwordHasher,
    ITokenIssuer tokenIssuer,
    ILogger<RegisterUser> logger
) : IUseCase<RegisterUserInput, RegisterUserOutput>
{
    public async Task<RegisterUserOutput> Execute(RegisterUserInput input)
    {
        logger.LogInformation("Registering user | Email: {Email}", input.Email);

        var userRepo = uow.Repo<IUserRepository>();
        var email = Email.Create(input.Email);

        if (await userRepo.ExistsByEmail(email))
            throw new UserAlreadyExistsException($"User already exists | Email: {email.Value}");

        var hashedPassword = passwordHasher.HashPassword(input.Password);

        var user = User.Create(
            email.Value,
            input.Username,
            hashedPassword
        );

        await userRepo.Save(user);
        await uow.Do();

        var token = tokenIssuer.Issue(user.Id, user.Email, user.Username);

        logger.LogInformation("User registered | Id: {UserId}", user.Id);

        return new RegisterUserOutput(user.Id.Value, token);
    }
}
