using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Users;

public sealed record ChangeUserPasswordInput(
    string OldPassword,
    string NewPassword,
    bool Persistent = false
);

/// <summary>A fresh token for the caller: the password change voids every older one.</summary>
public sealed record ChangeUserPasswordOutput(string Token);

public class ChangeUserPassword(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    IPasswordHasher passwordHasher,
    ITokenIssuer tokenIssuer,
    ILogger<ChangeUserPassword> logger
) : IUseCase<ChangeUserPasswordInput, ChangeUserPasswordOutput>
{
    public async Task<ChangeUserPasswordOutput> Execute(ChangeUserPasswordInput input, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;
        logger.LogInformation("Changing password | UserId: {UserId}", userId);

        var userRepo = uow.Repo<IUserRepository>();
        var user = await userRepo.ById(userId);

        if (!user.HasValue)
            throw new UserNotFoundException($"User not found | Id: {userId.Value}");

        if (!passwordHasher.VerifyHashedPassword(user.Value.HashedPassword.Value, input.OldPassword))
            throw new InvalidCredentialsException("Current password is incorrect.");

        var newHashedPassword = passwordHasher.HashPassword(input.NewPassword);
        user.Value.ChangePassword(newHashedPassword);

        await userRepo.Save(user.Value);
        await uow.Do(ct);

        logger.LogInformation("Password changed | UserId: {UserId}", userId);

        return new ChangeUserPasswordOutput(tokenIssuer.Issue(SessionTicket.For(user.Value, input.Persistent)));
    }
}
