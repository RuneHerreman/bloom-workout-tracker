using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Ports;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.LoggedWorkouts;

public sealed record ImportWorkoutInput(
    string Name,
    DateTime LoggedAt,
    List<LoggedExerciseInput> Exercises,
    string? Note = null,
    /// <summary>Id of the workout in the file it came from (e.g. a Bloom backup), used to skip it on a re-import.</summary>
    string? SourceId = null
);

public sealed record ImportLoggedWorkoutsInput(List<ImportWorkoutInput> Workouts);

public sealed record ImportFailure(int Index, string Name, DateTime LoggedAt, string Reason);

/// <param name="ImportedIds">Ids of the workouts this import created, so the import can be undone.</param>
public sealed record ImportLoggedWorkoutsOutput(int Imported, int Skipped, IReadOnlyList<ImportFailure> Failures, IReadOnlyList<Guid> ImportedIds);

/// <summary>
/// Imports many workouts at once (from a Bloom export or another app's file) in a
/// single save. Workouts you already have are skipped, so importing the same file
/// twice is harmless (two workouts inside the file are never merged): a workout counts as already there when its source id matches
/// a log of yours or an earlier import, or when you have a log with the same name
/// at the same minute. Workouts that can't be built (unknown exercise, invalid set)
/// are reported individually instead of failing the whole import.
/// </summary>
public class ImportLoggedWorkouts(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    IFindLoggedWorkoutsQuery logsQuery,
    ISearchExerciseCatalogQuery exerciseQuery,
    ILogger<ImportLoggedWorkouts> logger
) : IUseCase<ImportLoggedWorkoutsInput, ImportLoggedWorkoutsOutput>
{
    public const int MaxWorkouts = 5000;
    private const string ExternalIdPrefix = "import:";

    public async Task<ImportLoggedWorkoutsOutput> Execute(ImportLoggedWorkoutsInput input, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;
        if (input.Workouts.Count > MaxWorkouts)
            throw new ArgumentException($"An import can hold at most {MaxWorkouts} workouts.");

        if (!await uow.Repo<IUserRepository>().Exists(userId))
            throw new UserNotFoundException($"User not found | Id: {userId}");

        logger.LogInformation("Importing {Count} workouts | User: {UserId}", input.Workouts.Count, userId);

        var uid = userId.Value;
        var existing = await logsQuery.Fetch(l => l.UserId == uid, ct);
        var existingKeys = existing.Select(l => Key(l.Name, l.LoggedAt)).ToHashSet();
        var existingIds = existing.Select(l => l.Id.ToString()).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var logRepo = uow.Repo<ILoggedWorkoutRepository>();
        var externalIds = input.Workouts
            .Where(w => !string.IsNullOrWhiteSpace(w.SourceId))
            .Select(w => ExternalIdPrefix + w.SourceId!.Trim())
            .Distinct()
            .ToList();
        var importedBefore = externalIds.Count > 0
            ? await logRepo.GetExistingExternalIds(userId, externalIds, ct)
            : new HashSet<string>();

        var visibleExercises = (await exerciseQuery.Fetch(e => e.OwnerUserId == null || e.OwnerUserId == uid, ct))
            .Select(e => e.Id)
            .ToHashSet();

        var imported = 0;
        var skipped = 0;
        var failures = new List<ImportFailure>();
        var importedIds = new List<Guid>();

        for (var i = 0; i < input.Workouts.Count; i++)
        {
            var w = input.Workouts[i];
            var loggedAt = DateTime.SpecifyKind(w.LoggedAt, w.LoggedAt.Kind == DateTimeKind.Unspecified ? DateTimeKind.Utc : w.LoggedAt.Kind).ToUniversalTime();
            var key = Key(w.Name, loggedAt);
            var sourceId = string.IsNullOrWhiteSpace(w.SourceId) ? null : w.SourceId.Trim();

            if (existingKeys.Contains(key)
                || (sourceId is not null && (existingIds.Contains(sourceId) || importedBefore.Contains(ExternalIdPrefix + sourceId))))
            {
                skipped++;
                continue;
            }

            var unknown = w.Exercises.FirstOrDefault(e => !visibleExercises.Contains(e.ExerciseId));
            if (unknown is not null)
            {
                failures.Add(new ImportFailure(i, w.Name, loggedAt, $"Unknown exercise id {unknown.ExerciseId}"));
                continue;
            }

            try
            {
                var log = LoggedWorkout.Create(
                    userId,
                    w.Name,
                    w.Exercises.Select(e => e.ToLoggedExercise()).ToList(),
                    w.Note,
                    loggedAt,
                    sourceId is null ? null : ExternalIdPrefix + sourceId
                );
                await logRepo.Save(log);
                importedIds.Add(log.Id.Value);
                imported++;
            }
            // The domain rejects invalid data with these (e.g. a cardio set without a distance).
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or FormatException or BloomGeneralException)
            {
                failures.Add(new ImportFailure(i, w.Name, loggedAt, ex.Message));
            }
        }

        if (imported > 0) await uow.Do(ct);

        logger.LogInformation(
            "Import done | User: {UserId} - Imported: {Imported}, Skipped: {Skipped}, Failed: {Failed}",
            userId, imported, skipped, failures.Count);

        return new ImportLoggedWorkoutsOutput(imported, skipped, failures, importedIds);
    }

    /// <summary>Same workout = same name (case-insensitive) at the same minute.</summary>
    private static string Key(string name, DateTime loggedAt)
    {
        var utc = loggedAt.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(loggedAt, DateTimeKind.Utc) : loggedAt.ToUniversalTime();
        return $"{utc:yyyy-MM-ddTHH:mm}|{name.Trim().ToLowerInvariant()}";
    }
}
