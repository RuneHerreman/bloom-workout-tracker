using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Exercises;
using Bloom.Domain.Shared;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Exercises;

public sealed record DeleteCustomExerciseInput(Guid ExerciseId);

public class DeleteCustomExercise(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ILogger<DeleteCustomExercise> logger
) : IUseCase<DeleteCustomExerciseInput>
{
    public async Task Execute(DeleteCustomExerciseInput input, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;
        logger.LogInformation("Deleting custom Exercise | Id: {Id} - User: {UserId}", input.ExerciseId, userId);

        var exerciseRepo = uow.Repo<IExerciseRepository>();
        var exercise = await exerciseRepo.ById(EntityId.New<ExerciseId>(input.ExerciseId));

        if (!exercise.HasValue)
            throw new ExerciseNotFoundException(input.ExerciseId);

        if (exercise.Value.OwnerUserId != userId)
            throw new ExerciseAccessDeniedException($"User {userId} does not own custom exercise {input.ExerciseId}");

        await exerciseRepo.Remove(exercise.Value);
        await uow.Do(ct);

        logger.LogInformation("Custom Exercise deleted | Id: {Id} - User: {UserId}", input.ExerciseId, userId);
    }
}
