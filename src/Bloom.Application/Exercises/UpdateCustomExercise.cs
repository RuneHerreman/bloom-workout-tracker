using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Exercises;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.Shared;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Exercises;

public sealed record UpdateCustomExerciseInput(
    Guid ExerciseId,
    string Name,
    string Description,
    string Type,
    List<string> TargetMuscles
);

public sealed record UpdateCustomExerciseOutput(Guid ExerciseId);

public class UpdateCustomExercise(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ILogger<UpdateCustomExercise> logger
) : IUseCase<UpdateCustomExerciseInput, UpdateCustomExerciseOutput>
{
    public async Task<UpdateCustomExerciseOutput> Execute(UpdateCustomExerciseInput input, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;
        logger.LogInformation("Updating custom Exercise | Id: {Id} - User: {UserId}", input.ExerciseId, userId);

        var exerciseRepo = uow.Repo<IExerciseRepository>();
        var exercise = await exerciseRepo.ById(EntityId.New<ExerciseId>(input.ExerciseId));

        if (!exercise.HasValue)
            throw new ExerciseNotFoundException(input.ExerciseId);

        if (exercise.Value.OwnerUserId != userId)
            throw new ExerciseAccessDeniedException($"User {userId} does not own custom exercise {input.ExerciseId}");

        // Per-user uniqueness: block colliding with a global catalog name or another of
        // this user's own custom exercises (renaming onto itself is allowed).
        var existing = await exerciseRepo.ByNameForUser(input.Name, userId, ct);
        if (existing.HasValue && existing.Value.Id != exercise.Value.Id)
            throw new ExerciseAlreadyExistsException(input.Name);

        exercise.Value.Update(
            input.Name,
            input.Description,
            Enum.Parse<ExerciseType>(input.Type, ignoreCase: true),
            input.TargetMuscles
        );

        await exerciseRepo.Save(exercise.Value);
        await uow.Do(ct);

        logger.LogInformation("Custom Exercise updated | Id: {Id} - User: {UserId}", exercise.Value.Id, userId);

        return new UpdateCustomExerciseOutput(exercise.Value.Id.Value);
    }
}
