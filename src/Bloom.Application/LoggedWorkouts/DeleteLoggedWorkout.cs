using Bloom.Application.Contracts.Ports;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.Shared;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.LoggedWorkouts;

public sealed record DeleteLoggedWorkoutInput(Guid LoggedWorkoutId, Guid UserId);

public class DeleteLoggedWorkout(
    IUnitOfWork uow,
    ILogger<DeleteLoggedWorkout> logger
) : IUseCase<DeleteLoggedWorkoutInput>
{
    public async Task Execute(DeleteLoggedWorkoutInput input)
    {
        logger.LogInformation($"Deleting LoggedWorkout | Id: {input.LoggedWorkoutId} - User: {input.UserId}");

        var logRepo = uow.Repo<ILoggedWorkoutRepository>();
        var log = await logRepo.ById(EntityId.New<LoggedWorkoutId>(input.LoggedWorkoutId));

        if (!log.HasValue)
            throw new LoggedWorkoutNotFoundException($"LoggedWorkout not found | Id: {input.LoggedWorkoutId}");

        if (log.Value.UserId.Value != input.UserId)
            throw new LoggedWorkoutAccessDeniedException($"User {input.UserId} does not own LoggedWorkout {input.LoggedWorkoutId}");

        await logRepo.Remove(log.Value);
        await uow.Do();

        logger.LogInformation($"LoggedWorkout deleted | Id: {input.LoggedWorkoutId} - User: {input.UserId}");
    }
}