using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Users;

public sealed record UpdateUserGearInput(List<string> Gear);

public sealed record UpdateUserGearOutput;

public class UpdateUserGear(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ILogger<UpdateUserGear> logger
) : IUseCase<UpdateUserGearInput, UpdateUserGearOutput>
{
    public async Task<UpdateUserGearOutput> Execute(UpdateUserGearInput input, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;
        logger.LogInformation("Updating gear | Id: {UserId}", userId);

        var userRepo = uow.Repo<IUserRepository>();
        var user = await userRepo.ById(userId);

        if (!user.HasValue)
            throw new UserNotFoundException($"User not found | Id: {userId.Value}");

        user.Value.UpdateGear(input.Gear);

        await userRepo.Save(user.Value);
        await uow.Do(ct);

        logger.LogInformation("Gear updated | Id: {UserId}", user.Value.Id);

        return new UpdateUserGearOutput();
    }
}
