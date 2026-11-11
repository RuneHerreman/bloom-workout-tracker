using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Body;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Body;

public sealed record DeleteBodyMeasurementInput(DateOnly Date);

/// <summary>
/// Removes a day from the body log. When it was the latest day, the user's current
/// weight falls back to the day before it; deleting the only entry keeps the weight as is.
/// </summary>
public class DeleteBodyMeasurement(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ILogger<DeleteBodyMeasurement> logger
) : IUseCase<DeleteBodyMeasurementInput>
{
    public async Task Execute(DeleteBodyMeasurementInput input, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;
        var repo = uow.Repo<IBodyMeasurementRepository>();
        var entry = await repo.OnDate(userId, input.Date, ct)
            ?? throw new BodyMeasurementNotFoundException($"No body log entry | User: {userId.Value} - Date: {input.Date}");

        var all = await repo.ForUser(userId, ct: ct);
        var wasLatest = all.Count > 0 && all[^1].Date == entry.Date;
        await repo.Remove(entry);

        if (wasLatest)
        {
            var newLatest = all.LastOrDefault(m => m.Date != entry.Date);
            if (newLatest is not null)
            {
                var userRepo = uow.Repo<IUserRepository>();
                var user = await userRepo.ById(userId);
                if (user.HasValue)
                {
                    user.Value.SyncWeight(newLatest.WeightKg);
                    await userRepo.Save(user.Value);
                }
            }
        }

        await uow.Do(ct);
        logger.LogInformation("Body measurement deleted | User: {UserId} - Date: {Date}", userId, input.Date);
    }
}
