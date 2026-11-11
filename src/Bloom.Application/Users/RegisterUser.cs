using Bloom.Domain.Body;
using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Users;
using Bloom.Domain.Users.ValueObjects;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Users;

public sealed record RegisterUserInput(
    string Email,
    string Username,
    string Password,
    string FirstName,
    string LastName,
    decimal Weight,
    int Height,
    int ActiveDays,
    DateOnly BirthDate
);

public sealed record RegisterUserOutput(string Token);

public class RegisterUser(
    IUnitOfWork uow,
    IPasswordHasher passwordHasher,
    ITokenIssuer tokenIssuer,
    ILogger<RegisterUser> logger
) : IUseCase<RegisterUserInput, RegisterUserOutput>
{
    public async Task<RegisterUserOutput> Execute(RegisterUserInput input, CancellationToken ct = default)
    {
        logger.LogInformation("Registering user | Email: {Email}", input.Email);

        var userRepo = uow.Repo<IUserRepository>();
        var email = Email.Create(input.Email);

        if (await userRepo.ExistsByEmail(email))
            throw new UserAlreadyExistsException("An account with this email already exists.");

        var hashedPassword = passwordHasher.HashPassword(input.Password);

        var user = User.Create(
            email.Value,
            input.Username,
            hashedPassword,
            input.FirstName,
            input.LastName,
            input.Weight,
            input.Height,
            input.ActiveDays,
            input.BirthDate
        );

        await userRepo.Save(user);

        // The weight given at signup is the first day of the body log.
        await uow.Repo<IBodyMeasurementRepository>().Save(BodyMeasurement.Create(
            user.Id, DateOnly.FromDateTime(DateTime.UtcNow), user.Weight, null, BodyMeasurementSource.Signup));

        await uow.Do(ct);

        var token = tokenIssuer.Issue(SessionTicket.For(user, persistent: false));

        logger.LogInformation("User registered | Id: {UserId}", user.Id);

        return new RegisterUserOutput(token);
    }
}
