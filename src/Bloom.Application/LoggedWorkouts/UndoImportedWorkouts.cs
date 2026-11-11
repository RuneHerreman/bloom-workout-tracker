using Bloom.Application.Contracts.Ports;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.LoggedWorkouts;

public sealed record UndoImportedWorkoutsInput(List<Guid> LoggedWorkoutIds);

public sealed record UndoImportedWorkoutsOutput(int Deleted);

/// <summary>
/// Removes the workouts an import created (the ids it returned), in one save.
/// Ids that are already gone or belong to someone else are ignored, so undo is
/// safe to retry.
/// </summary>
public class UndoImportedWorkouts(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ILogger<UndoImportedWorkouts> logger
) : IUseCase<UndoImportedWorkoutsInput, UndoImportedWorkoutsOutput>
{
    public async Task<UndoImportedWorkoutsOutput> Execute(UndoImportedWorkoutsInput input, CancellationToken ct = default)
    {
        if (input.LoggedWorkoutIds.Count > ImportLoggedWorkouts.MaxWorkouts)
            throw new ArgumentException($"At most {ImportLoggedWorkouts.MaxWorkouts} workouts can be removed at once.");

        var userId = currentUser.UserId;
        var repo = uow.Repo<ILoggedWorkoutRepository>();
        var deleted = 0;
        foreach (var id in input.LoggedWorkoutIds.Distinct())
        {
            var log = await repo.ById(EntityId.New<LoggedWorkoutId>(id));
            if (!log.HasValue || log.Value.UserId != userId) continue;
            await repo.Remove(log.Value);
            deleted++;
        }
        if (deleted > 0) await uow.Do(ct);

        logger.LogInformation("Import undone | User: {UserId} - Deleted: {Deleted} of {Requested}", userId, deleted, input.LoggedWorkoutIds.Count);
        return new UndoImportedWorkoutsOutput(deleted);
    }
}
