using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Users;
using Bloom.Domain.Users.ValueObjects;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Users;

public sealed record UpdateUserInfoInput(
    string Email,
    string Username,
    string FirstName,
    string LastName,
    decimal Weight,
    int Height,
    int ActiveDays
);

public sealed record UpdateUserInfoOutput(Guid UserId);

public class UpdateUserInfo(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ILogger<UpdateUserInfo> logger
) : IUseCase<UpdateUserInfoInput, UpdateUserInfoOutput>
{
    public async Task<UpdateUserInfoOutput> Execute(UpdateUserInfoInput input)
    {
        var userId = currentUser.UserId;
        logger.LogInformation("Updating User | Id: {UserId}", userId);

        var userRepo = uow.Repo<IUserRepository>();
        var user = await userRepo.ById(userId);

        if (!user.HasValue)
            throw new UserNotFoundException($"User not found | Id: {userId.Value}");

        var newEmail = Email.Create(input.Email);
        if (user.Value.Email != newEmail && await userRepo.ExistsByEmail(newEmail))
            throw new UserAlreadyExistsException($"User already exists | Email: {newEmail.Value}");

        user.Value.UpdateInfo(
            input.Email,
            input.Username,
            input.FirstName,
            input.LastName,
            input.Weight,
            input.Height,
            input.ActiveDays
        );

        await userRepo.Save(user.Value);
        await uow.Do();

        logger.LogInformation("User updated | Id: {UserId}", user.Value.Id);

        return new UpdateUserInfoOutput(user.Value.Id.Value);
    }
}
