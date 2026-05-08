using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Domain.Users.ValueObjects;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Users;

public sealed record UpdateUserInfoInput(
    Guid UserId,
    string Email,
    string Username,
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
        var requestedUserId = EntityId.New<UserId>(input.UserId);
        logger.LogInformation("Updating User | Id: {UserId}", input.UserId);

        if (currentUser.UserId != requestedUserId)
            throw new UserAccessDeniedException(
                $"User {currentUser.UserId.Value} cannot update user {input.UserId}");

        var userRepo = uow.Repo<IUserRepository>();
        var user = await userRepo.ById(requestedUserId);

        if (!user.HasValue)
            throw new UserNotFoundException($"User not found | Id: {input.UserId}");

        var newEmail = Email.Create(input.Email);
        if (user.Value.Email != newEmail && await userRepo.ExistsByEmail(newEmail))
            throw new UserAlreadyExistsException($"User already exists | Email: {newEmail.Value}");

        user.Value.UpdateInfo(
            input.Email,
            input.Username,
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
