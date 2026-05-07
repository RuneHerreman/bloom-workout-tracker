using Bloom.Application.Contracts.Ports;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.Shared;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.LoggedWorkouts;

public sealed record UpdateLoggedWorkoutInput(
    Guid LoggedWorkoutId,
    Guid UserId,
    DateTime LoggedAt,
    List<LoggedExerciseInput> Exercises
);

public sealed record UpdateLoggedWorkoutOutput(Guid LoggedWorkoutId);

public class UpdateLoggedWorkout(
    IUnitOfWork uow,
    ILogger<UpdateLoggedWorkout> logger
) : IUseCase<UpdateLoggedWorkoutInput, UpdateLoggedWorkoutOutput>
{
    public async Task<UpdateLoggedWorkoutOutput> Execute(UpdateLoggedWorkoutInput input)
    {
        logger.LogInformation($"Updating LoggedWorkout | Id: {input.LoggedWorkoutId} - User: {input.UserId}");

        var logRepo = uow.Repo<ILoggedWorkoutRepository>();
        var log = await logRepo.ById(EntityId.New<LoggedWorkoutId>(input.LoggedWorkoutId));

        if (!log.HasValue)
            throw new LoggedWorkoutNotFoundException($"LoggedWorkout not found | Id: {input.LoggedWorkoutId}");

        if (log.Value.UserId.Value != input.UserId)
            throw new LoggedWorkoutAccessDeniedException($"User {input.UserId} does not own LoggedWorkout {input.LoggedWorkoutId}");

        var exercises = input.Exercises.Select(e => e.ToLoggedExercise()).ToList();
        log.Value.Update(input.LoggedAt, exercises);

        await logRepo.Save(log.Value);
        await uow.Do();

        logger.LogInformation($"LoggedWorkout updated | Id: {log.Value.Id} - User: {input.UserId}");

        return new UpdateLoggedWorkoutOutput(log.Value.Id.Value);
    }
}