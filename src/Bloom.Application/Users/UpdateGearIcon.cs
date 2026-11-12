using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Users;

/// <param name="Kind">One of <see cref="GearKinds.All"/>, or null to go back to the icon guessed from the name.</param>
public sealed record UpdateGearIconInput(string Name, string? Kind);

public sealed record UpdateGearIconOutput;

/// <summary>Sets or clears the icon the user picked for one of their gear items.</summary>
public class UpdateGearIcon(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ILogger<UpdateGearIcon> logger
) : IUseCase<UpdateGearIconInput, UpdateGearIconOutput>
{
    public async Task<UpdateGearIconOutput> Execute(UpdateGearIconInput input, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;
        if (input.Kind is not null && !GearKinds.All.Contains(input.Kind))
            throw new BloomGeneralException($"“{input.Kind}” isn't a gear icon.");

        var userRepo = uow.Repo<IUserRepository>();
        var user = await userRepo.ById(userId);
        if (!user.HasValue)
            throw new UserNotFoundException($"User not found | Id: {userId.Value}");

        if (!user.Value.SetGearIcon(input.Name, input.Kind))
            throw new BloomGeneralException($"“{input.Name}” isn't in your gear.");

        await userRepo.Save(user.Value);
        await uow.Do(ct);
        logger.LogInformation("Gear icon updated | Id: {UserId}", userId);
        return new UpdateGearIconOutput();
    }
}
