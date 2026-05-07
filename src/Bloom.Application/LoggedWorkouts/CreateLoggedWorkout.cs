using Bloom.Application.Contracts.Ports;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.LoggedWorkouts;

public sealed record CreateLoggedWorkoutInput(
    Guid UserId,
    List<LoggedExerciseInput> Exercises
);

public sealed record CreateLoggedWorkoutOutput(Guid LoggedWorkoutId);

public class CreateLoggedWorkout(
    IUnitOfWork uow,
    ILogger<CreateLoggedWorkout> logger
) : IUseCase<CreateLoggedWorkoutInput, CreateLoggedWorkoutOutput>
{
    public async Task<CreateLoggedWorkoutOutput> Execute(CreateLoggedWorkoutInput input)
    {
        logger.LogInformation($"Creating LoggedWorkout | User: {input.UserId}");

        var logRepo = uow.Repo<ILoggedWorkoutRepository>();
        var userExists = await uow.Repo<IUserRepository>().Exists(EntityId.New<UserId>(input.UserId));

        if (!userExists)
            throw new UserNotFoundException($"User not found | Id: {input.UserId}");

        var exercises = input.Exercises.Select(e => e.ToLoggedExercise()).ToList();

        var log = LoggedWorkout.Create(
            EntityId.New<UserId>(input.UserId),
            exercises
        );

        await logRepo.Save(log);
        await uow.Do();

        logger.LogInformation($"LoggedWorkout created | Id: {log.Id} - User: {input.UserId}");

        return new CreateLoggedWorkoutOutput(log.Id.Value);
    }
}