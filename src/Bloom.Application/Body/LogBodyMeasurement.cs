using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Body;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Body;

/// <param name="BodyFatPct">A new body fat reading, or null to keep the day's existing one.</param>
/// <param name="ClearBodyFat">Remove the day's body fat reading (ignored when <paramref name="BodyFatPct"/> is given).</param>
public sealed record LogBodyMeasurementInput(DateOnly Date, decimal WeightKg, decimal? BodyFatPct = null, bool ClearBodyFat = false);

public sealed record LogBodyMeasurementOutput(BodyMeasurementData Entry, bool Created);

/// <summary>
/// Logs a weigh-in for a day: creates the day's entry, or updates it when there is one
/// (re-weighing replaces the weight; a body fat reading is only replaced by a new one).
/// When it's the latest day in the log, it becomes the user's current weight.
/// </summary>
public class LogBodyMeasurement(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ILogger<LogBodyMeasurement> logger
) : IUseCase<LogBodyMeasurementInput, LogBodyMeasurementOutput>
{
    public async Task<LogBodyMeasurementOutput> Execute(LogBodyMeasurementInput input, CancellationToken ct = default)
    {
        BodyLog.Validate(input.Date, input.WeightKg, input.BodyFatPct);

        var userId = currentUser.UserId;
        var userRepo = uow.Repo<IUserRepository>();
        var user = await userRepo.ById(userId);
        if (!user.HasValue)
            throw new UserNotFoundException($"User not found | Id: {userId.Value}");

        var repo = uow.Repo<IBodyMeasurementRepository>();
        var latest = await repo.Latest(userId, ct);
        var existing = await repo.OnDate(userId, input.Date, ct);

        BodyMeasurement entry;
        if (existing is null)
        {
            entry = BodyMeasurement.Create(userId, input.Date, input.WeightKg, input.BodyFatPct, BodyMeasurementSource.Manual);
        }
        else
        {
            entry = existing;
            entry.Update(input.WeightKg, input.BodyFatPct);
            if (input.BodyFatPct is null && input.ClearBodyFat) entry.ClearBodyFat();
        }
        await repo.Save(entry);

        if (latest is null || input.Date >= latest.Date)
        {
            user.Value.SyncWeight(entry.WeightKg);
            await userRepo.Save(user.Value);
        }

        await uow.Do(ct);

        logger.LogInformation("Body measurement {Action} | User: {UserId} - Date: {Date}",
            existing is null ? "logged" : "updated", userId, input.Date);
        return new LogBodyMeasurementOutput(entry.ToData(), existing is null);
    }
}
