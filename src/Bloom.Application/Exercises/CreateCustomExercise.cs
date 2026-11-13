using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Exercises;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Exercises;

public sealed record CreateCustomExerciseInput(
    string Name,
    string Description,
    string Type,
    List<string> TargetMuscles
);

public sealed record CreateCustomExerciseOutput(Guid ExerciseId);

public class CreateCustomExercise(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ILogger<CreateCustomExercise> logger
) : IUseCase<CreateCustomExerciseInput, CreateCustomExerciseOutput>
{
    public async Task<CreateCustomExerciseOutput> Execute(CreateCustomExerciseInput input, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;
        logger.LogInformation("Creating custom Exercise | User: {UserId}", userId);

        var exerciseRepo = uow.Repo<IExerciseRepository>();
        var userExists = await uow.Repo<IUserRepository>().Exists(userId);

        if (!userExists)
            throw new UserNotFoundException($"User not found | Id: {userId}");

        // Per-user uniqueness: block shadowing a global catalog name or reusing one of
        // your own custom names. Two different users may share the same custom name.
        var existing = await exerciseRepo.ByNameForUser(input.Name, userId, ct);
        if (existing.HasValue)
            throw new ExerciseAlreadyExistsException(input.Name);

        var exercise = Exercise.CreateCustom(
            userId,
            input.Name,
            input.Description,
            Enum.Parse<ExerciseType>(input.Type, ignoreCase: true),
            input.TargetMuscles
        );

        await exerciseRepo.Save(exercise);
        await uow.Do(ct);

        logger.LogInformation("Custom Exercise created | Id: {ExerciseId} - User: {UserId}", exercise.Id, userId);

        return new CreateCustomExerciseOutput(exercise.Id.Value);
    }
}
