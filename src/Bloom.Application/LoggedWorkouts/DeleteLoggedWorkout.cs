using Bloom.Application.Contracts.Ports;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.Shared;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.LoggedWorkouts;

public sealed record DeleteLoggedWorkoutInput(Guid LoggedWorkoutId);

public class DeleteLoggedWorkout(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ILogger<DeleteLoggedWorkout> logger
) : IUseCase<DeleteLoggedWorkoutInput>
{
    public async Task Execute(DeleteLoggedWorkoutInput input, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;
        logger.LogInformation("Deleting LoggedWorkout | Id: {Id} - User: {UserId}", input.LoggedWorkoutId, userId);

        var logRepo = uow.Repo<ILoggedWorkoutRepository>();
        var log = await logRepo.ById(EntityId.New<LoggedWorkoutId>(input.LoggedWorkoutId));

        if (!log.HasValue)
            throw new LoggedWorkoutNotFoundException($"LoggedWorkout not found | Id: {input.LoggedWorkoutId}");

        if (log.Value.UserId != userId)
            throw new LoggedWorkoutAccessDeniedException($"User {userId} does not own LoggedWorkout {input.LoggedWorkoutId}");

        await logRepo.Remove(log.Value);
        await uow.Do(ct);

        logger.LogInformation("LoggedWorkout deleted | Id: {Id} - User: {UserId}", input.LoggedWorkoutId, userId);
    }
}
