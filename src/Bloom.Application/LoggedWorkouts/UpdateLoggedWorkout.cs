using Bloom.Application.Contracts.Ports;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.Shared;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.LoggedWorkouts;

public sealed record UpdateLoggedWorkoutInput(
    Guid LoggedWorkoutId,
    DateTime LoggedAt,
    List<LoggedExerciseInput> Exercises
);

public sealed record UpdateLoggedWorkoutOutput(Guid LoggedWorkoutId);

public class UpdateLoggedWorkout(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ILogger<UpdateLoggedWorkout> logger
) : IUseCase<UpdateLoggedWorkoutInput, UpdateLoggedWorkoutOutput>
{
    public async Task<UpdateLoggedWorkoutOutput> Execute(UpdateLoggedWorkoutInput input)
    {
        var userId = currentUser.UserId;
        logger.LogInformation("Updating LoggedWorkout | Id: {Id} - User: {UserId}", input.LoggedWorkoutId, userId);

        var logRepo = uow.Repo<ILoggedWorkoutRepository>();
        var log = await logRepo.ById(EntityId.New<LoggedWorkoutId>(input.LoggedWorkoutId));

        if (!log.HasValue)
            throw new LoggedWorkoutNotFoundException($"LoggedWorkout not found | Id: {input.LoggedWorkoutId}");

        if (log.Value.UserId != userId)
            throw new LoggedWorkoutAccessDeniedException($"User {userId} does not own LoggedWorkout {input.LoggedWorkoutId}");

        var exercises = input.Exercises.Select(e => e.ToLoggedExercise()).ToList();
        log.Value.Update(input.LoggedAt, exercises);

        await logRepo.Save(log.Value);
        await uow.Do();

        logger.LogInformation("LoggedWorkout updated | Id: {Id} - User: {UserId}", log.Value.Id, userId);

        return new UpdateLoggedWorkoutOutput(log.Value.Id.Value);
    }
}
