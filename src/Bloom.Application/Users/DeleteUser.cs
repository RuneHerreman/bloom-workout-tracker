using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Users;

public sealed record DeleteUserInput(Guid UserId);

public class DeleteUser(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ILogger<DeleteUser> logger
) : IUseCase<DeleteUserInput>
{
    public async Task Execute(DeleteUserInput input)
    {
        var requestedUserId = EntityId.New<UserId>(input.UserId);
        logger.LogInformation("Deleting User | Id: {UserId}", input.UserId);

        if (currentUser.UserId != requestedUserId)
            throw new UserAccessDeniedException(
                $"User {currentUser.UserId.Value} cannot delete user {input.UserId}");

        var userRepo = uow.Repo<IUserRepository>();
        var user = await userRepo.ById(requestedUserId);

        if (!user.HasValue)
            throw new UserNotFoundException($"User not found | Id: {input.UserId}");

        await userRepo.Remove(user.Value);
        await uow.Do();

        logger.LogInformation("User deleted | Id: {UserId}", input.UserId);
    }
}
