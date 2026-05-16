using Bloom.Application.Contracts.Ports;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.LoggedWorkouts;

public sealed record CreateLoggedWorkoutInput(
    string Name,
    List<LoggedExerciseInput> Exercises,
    string? Note = null,
    DateTime? LoggedAt = null
);

public sealed record CreateLoggedWorkoutOutput(Guid LoggedWorkoutId);

public class CreateLoggedWorkout(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ILogger<CreateLoggedWorkout> logger
) : IUseCase<CreateLoggedWorkoutInput, CreateLoggedWorkoutOutput>
{
    public async Task<CreateLoggedWorkoutOutput> Execute(CreateLoggedWorkoutInput input, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;
        logger.LogInformation("Creating LoggedWorkout | User: {UserId}", userId);

        var logRepo = uow.Repo<ILoggedWorkoutRepository>();
        var userExists = await uow.Repo<IUserRepository>().Exists(userId);

        if (!userExists)
            throw new UserNotFoundException($"User not found | Id: {userId}");

        var exercises = input.Exercises.Select(e => e.ToLoggedExercise()).ToList();

        var log = LoggedWorkout.Create(
            userId,
            input.Name,
            exercises,
            input.Note,
            input.LoggedAt
        );

        await logRepo.Save(log);
        await uow.Do(ct);

        logger.LogInformation("LoggedWorkout created | Id: {LogId} - User: {UserId}", log.Id, userId);

        return new CreateLoggedWorkoutOutput(log.Id.Value);
    }
}
