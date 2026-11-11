using Bloom.Domain.Body;
using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Imports;
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

/// <param name="FileName">Shown in the import history.</param>
/// <param name="CreatedExerciseIds">Custom exercises the client created for this import, so undo can remove them.</param>
/// <param name="CreatedTemplateIds">Templates the client created for this import, so undo can remove them.</param>
/// <summary>One day of the body log in an import (a Bloom backup's Body sheet, or a scale app's export).</summary>
public sealed record ImportBodyInput(DateOnly Date, decimal WeightKg, decimal? BodyFatPct = null);

/// <param name="Body">Body log days to add alongside the workouts; days already in the log are skipped.</param>
public sealed record ImportLoggedWorkoutsInput(
    List<ImportWorkoutInput> Workouts,
    string? FileName = null,
    List<Guid>? CreatedExerciseIds = null,
    List<Guid>? CreatedTemplateIds = null,
    List<ImportBodyInput>? Body = null
);

public sealed record ImportFailure(int Index, string Name, DateTime LoggedAt, string Reason);

public sealed record ImportBodyFailure(DateOnly Date, string Reason);

/// <param name="Skipped">Days already in the body log (or repeated in the file).</param>
/// <param name="DroppedBodyFat">Days whose body fat reading was out of range: the weight was kept.</param>
public sealed record ImportBodyOutput(int Imported, int Skipped, int DroppedBodyFat, IReadOnlyList<ImportBodyFailure> Failures)
{
    public static readonly ImportBodyOutput None = new(0, 0, 0, []);
}

/// <param name="ImportedIds">Ids of the workouts this import created, so the import can be undone.</param>
/// <param name="BatchId">The import in the history (null when nothing was added).</param>
public sealed record ImportLoggedWorkoutsOutput(int Imported, int Skipped, IReadOnlyList<ImportFailure> Failures, IReadOnlyList<Guid> ImportedIds, Guid? BatchId = null, ImportBodyOutput? Body = null);

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
    /// <summary>Daily weigh-ins for over 50 years.</summary>
    public const int MaxBodyEntries = 20000;
    private const string ExternalIdPrefix = "import:";

    public async Task<ImportLoggedWorkoutsOutput> Execute(ImportLoggedWorkoutsInput input, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;
        if (input.Workouts.Count > MaxWorkouts)
            throw new ArgumentException($"An import can hold at most {MaxWorkouts} workouts.");
        if ((input.Body?.Count ?? 0) > MaxBodyEntries)
            throw new ArgumentException($"An import can hold at most {MaxBodyEntries} body log days.");

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

        var (body, bodyIds) = input.Body is { Count: > 0 }
            ? await ImportBody(userId, input.Body, ct)
            : (ImportBodyOutput.None, new List<Guid>());

        // Record the import so it can be undone later from the history.
        var createdExercises = input.CreatedExerciseIds ?? [];
        var createdTemplates = input.CreatedTemplateIds ?? [];
        ImportBatch? batch = null;
        if (importedIds.Count > 0 || createdExercises.Count > 0 || createdTemplates.Count > 0 || bodyIds.Count > 0)
        {
            batch = ImportBatch.Create(userId, input.FileName, importedIds, createdExercises, createdTemplates, bodyIds);
            await uow.Repo<IImportBatchRepository>().Save(batch);
        }

        if (batch is not null) await uow.Do(ct);

        logger.LogInformation(
            "Import done | User: {UserId} - Imported: {Imported}, Skipped: {Skipped}, Failed: {Failed}, Body days: {BodyImported} (skipped {BodySkipped})",
            userId, imported, skipped, failures.Count, body.Imported, body.Skipped);

        return new ImportLoggedWorkoutsOutput(imported, skipped, failures, importedIds, batch?.Id.Value, body);
    }

    /// <summary>
    /// Adds the file's body log days. A day already in the log keeps what is there (an
    /// import never overwrites a weigh-in); a day repeated in the file counts once, the
    /// first time. An out-of-range body fat reading is dropped and the weight kept.
    /// When the newest imported day is newer than the log's, it becomes the current weight.
    /// </summary>
    private async Task<(ImportBodyOutput Output, List<Guid> Ids)> ImportBody(UserId userId, List<ImportBodyInput> days, CancellationToken ct)
    {
        var repo = uow.Repo<IBodyMeasurementRepository>();
        var existing = await repo.ForUser(userId, ct: ct);
        var taken = existing.Select(m => m.Date).ToHashSet();
        var latest = existing.Count > 0 ? existing[^1] : null;
        var lastAllowed = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);

        var ids = new List<Guid>();
        var failures = new List<ImportBodyFailure>();
        int skipped = 0, droppedBodyFat = 0;
        BodyMeasurement? newest = null;

        foreach (var d in days)
        {
            if (taken.Contains(d.Date)) { skipped++; continue; }
            if (d.Date > lastAllowed) { failures.Add(new(d.Date, "The date is in the future.")); continue; }
            if (!BodyMeasurement.IsValidWeight(d.WeightKg))
            {
                failures.Add(new(d.Date, $"Weight must be between {BodyMeasurement.MinWeightKg:0} and {BodyMeasurement.MaxWeightKg:0} kg."));
                continue;
            }
            var bodyFat = d.BodyFatPct;
            if (bodyFat is { } bf && !BodyMeasurement.IsValidBodyFat(bf)) { bodyFat = null; droppedBodyFat++; }

            var entry = BodyMeasurement.Create(userId, d.Date, d.WeightKg, bodyFat, BodyMeasurementSource.Import);
            await repo.Save(entry);
            taken.Add(d.Date);
            ids.Add(entry.Id.Value);
            if (newest is null || entry.Date > newest.Date) newest = entry;
        }

        if (newest is not null && (latest is null || newest.Date > latest.Date))
        {
            var userRepo = uow.Repo<IUserRepository>();
            var user = await userRepo.ById(userId);
            if (user.HasValue)
            {
                user.Value.SyncWeight(newest.WeightKg);
                await userRepo.Save(user.Value);
            }
        }

        return (new ImportBodyOutput(ids.Count, skipped, droppedBodyFat, failures), ids);
    }

    /// <summary>Same workout = same name (case-insensitive) at the same minute.</summary>
    private static string Key(string name, DateTime loggedAt)
    {
        var utc = loggedAt.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(loggedAt, DateTimeKind.Utc) : loggedAt.ToUniversalTime();
        return $"{utc:yyyy-MM-ddTHH:mm}|{name.Trim().ToLowerInvariant()}";
    }
}
