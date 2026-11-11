using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Exercises;
using Bloom.Domain.Imports;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.Shared;
using Bloom.Domain.WorkoutTemplates;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.LoggedWorkouts;

public sealed record UndoImportBatchInput(Guid ImportBatchId);

/// <param name="KeptExercises">New exercises from the import that stay because other workouts or templates now use them.</param>
public sealed record UndoImportBatchOutput(int Workouts, int Templates, int Exercises, int KeptExercises);

/// <summary>
/// Undoes one import from the history: removes the workouts and templates it
/// added, and the exercises it created unless something outside the import now
/// uses them. Workouts already deleted by hand are skipped. Undoing twice is
/// harmless: the second call changes nothing.
/// </summary>
public class UndoImportBatch(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    IFindLoggedWorkoutsQuery logsQuery,
    IFindWorkoutTemplatesQuery templatesQuery,
    ILogger<UndoImportBatch> logger
) : IUseCase<UndoImportBatchInput, UndoImportBatchOutput>
{
    public async Task<UndoImportBatchOutput> Execute(UndoImportBatchInput input, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;
        var batches = uow.Repo<IImportBatchRepository>();
        var found = await batches.ById(EntityId.New<ImportBatchId>(input.ImportBatchId));
        if (!found.HasValue || found.Value.UserId != userId)
            throw new ImportBatchNotFoundException($"Import not found | Id: {input.ImportBatchId}");

        var batch = found.Value;
        if (batch.IsUndone) return new UndoImportBatchOutput(0, 0, 0, 0);

        var workoutRepo = uow.Repo<ILoggedWorkoutRepository>();
        var workouts = 0;
        foreach (var id in batch.WorkoutIds)
        {
            var log = await workoutRepo.ById(EntityId.New<LoggedWorkoutId>(id));
            if (!log.HasValue || log.Value.UserId != userId) continue;
            await workoutRepo.Remove(log.Value);
            workouts++;
        }

        var templateRepo = uow.Repo<IWorkoutTemplateRepository>();
        var templates = 0;
        foreach (var id in batch.TemplateIds)
        {
            var template = await templateRepo.ById(EntityId.New<WorkoutTemplateId>(id));
            if (!template.HasValue || template.Value.UserId != userId) continue;
            await templateRepo.Remove(template.Value);
            templates++;
        }

        // An exercise created by the import may since have been used elsewhere: keep those.
        var uid = userId.Value;
        var batchWorkouts = batch.WorkoutIds.ToHashSet();
        var batchTemplates = batch.TemplateIds.ToHashSet();
        var usedByLogs = (await logsQuery.Fetch(l => l.UserId == uid, ct))
            .Where(l => !batchWorkouts.Contains(l.Id))
            .SelectMany(l => l.LoggedExercises.Select(e => e.ExerciseId));
        var usedByTemplates = (await templatesQuery.Fetch(t => t.UserId == uid, ct))
            .Where(t => !batchTemplates.Contains(t.Id))
            .SelectMany(t => t.TemplateExercises.Select(e => e.ExerciseId));
        var stillUsed = usedByLogs.Concat(usedByTemplates).ToHashSet();

        var exerciseRepo = uow.Repo<IExerciseRepository>();
        int exercises = 0, kept = 0;
        foreach (var id in batch.ExerciseIds)
        {
            var exercise = await exerciseRepo.ById(EntityId.New<ExerciseId>(id));
            if (!exercise.HasValue || exercise.Value.OwnerUserId != userId) continue;
            if (stillUsed.Contains(id)) { kept++; continue; }
            await exerciseRepo.Remove(exercise.Value);
            exercises++;
        }

        batch.MarkUndone();
        await batches.Save(batch);
        await uow.Do(ct);

        logger.LogInformation(
            "Import undone | Batch: {BatchId} - User: {UserId} - Workouts: {Workouts}, Templates: {Templates}, Exercises: {Exercises} (kept {Kept})",
            batch.Id, userId, workouts, templates, exercises, kept);
        return new UndoImportBatchOutput(workouts, templates, exercises, kept);
    }
}
