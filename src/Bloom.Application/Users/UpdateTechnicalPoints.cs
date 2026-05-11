using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Users;

public sealed record UpdateTechnicalPointsInput(string? TechnicalPoints);

public sealed record UpdateTechnicalPointsOutput;

public class UpdateTechnicalPoints(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ILogger<UpdateTechnicalPoints> logger
) : IUseCase<UpdateTechnicalPointsInput, UpdateTechnicalPointsOutput>
{
    public async Task<UpdateTechnicalPointsOutput> Execute(UpdateTechnicalPointsInput input)
    {
        var userId = currentUser.UserId;
        logger.LogInformation("Updating technical points | Id: {UserId}", userId);

        var userRepo = uow.Repo<IUserRepository>();
        var user = await userRepo.ById(userId);

        if (!user.HasValue)
            throw new UserNotFoundException($"User not found | Id: {userId.Value}");

        user.Value.UpdateTechnicalPoints(input.TechnicalPoints);

        await userRepo.Save(user.Value);
        await uow.Do();

        logger.LogInformation("Technical points updated | Id: {UserId}", user.Value.Id);

        return new UpdateTechnicalPointsOutput();
    }
}
