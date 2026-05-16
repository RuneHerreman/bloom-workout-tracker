using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Users;

public sealed record DeleteUserInput;

public class DeleteUser(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ILogger<DeleteUser> logger
) : IUseCase<DeleteUserInput>
{
    public async Task Execute(DeleteUserInput input, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;
        logger.LogInformation("Deleting User | Id: {UserId}", userId);

        var userRepo = uow.Repo<IUserRepository>();
        var user = await userRepo.ById(userId);

        if (!user.HasValue)
            throw new UserNotFoundException($"User not found | Id: {userId.Value}");

        await userRepo.Remove(user.Value);
        await uow.Do(ct);

        logger.LogInformation("User deleted | Id: {UserId}", userId);
    }
}
