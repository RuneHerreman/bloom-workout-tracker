using Bloom.Domain.Body;
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
    int ActiveDays,
    DateOnly BirthDate
);

public sealed record UpdateUserInfoOutput(Guid UserId);

public class UpdateUserInfo(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ILogger<UpdateUserInfo> logger
) : IUseCase<UpdateUserInfoInput, UpdateUserInfoOutput>
{
    public async Task<UpdateUserInfoOutput> Execute(UpdateUserInfoInput input, CancellationToken ct = default)
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

        var weightChanged = user.Value.Weight != input.Weight;

        user.Value.UpdateInfo(
            input.Email,
            input.Username,
            input.FirstName,
            input.LastName,
            input.Weight,
            input.Height,
            input.ActiveDays,
            input.BirthDate
        );

        await userRepo.Save(user.Value);

        // A new weight in the profile is a weigh-in today, so the body log stays the source of truth.
        if (weightChanged && BodyMeasurement.IsValidWeight(input.Weight))
        {
            var bodyRepo = uow.Repo<IBodyMeasurementRepository>();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var entry = await bodyRepo.OnDate(userId, today, ct);
            if (entry is null)
                entry = BodyMeasurement.Create(userId, today, input.Weight, null, BodyMeasurementSource.Manual);
            else
                entry.Update(input.Weight, null);
            await bodyRepo.Save(entry);
        }

        await uow.Do(ct);

        logger.LogInformation("User updated | Id: {UserId}", user.Value.Id);

        return new UpdateUserInfoOutput(user.Value.Id.Value);
    }
}
