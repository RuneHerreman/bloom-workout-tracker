using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Users;

public sealed record UpdateLeaderboardVisibilityInput(bool ShowOnLeaderboard);

public sealed record UpdateLeaderboardVisibilityOutput;

public class UpdateLeaderboardVisibility(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ILogger<UpdateLeaderboardVisibility> logger
) : IUseCase<UpdateLeaderboardVisibilityInput, UpdateLeaderboardVisibilityOutput>
{
    public async Task<UpdateLeaderboardVisibilityOutput> Execute(UpdateLeaderboardVisibilityInput input, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;
        logger.LogInformation("Updating leaderboard visibility | Id: {UserId} | Show: {Show}", userId, input.ShowOnLeaderboard);

        var userRepo = uow.Repo<IUserRepository>();
        var user = await userRepo.ById(userId);

        if (!user.HasValue)
            throw new UserNotFoundException($"User not found | Id: {userId.Value}");

        user.Value.SetLeaderboardVisibility(input.ShowOnLeaderboard);

        await userRepo.Save(user.Value);
        await uow.Do(ct);

        return new UpdateLeaderboardVisibilityOutput();
    }
}
