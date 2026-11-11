using Bloom.Application.Contracts;
using Bloom.Application.LoggedWorkouts;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using UnitTests.Application.Exercises;
using UnitTests.Application.Mocks;
using UnitTests.Application.Shared;

namespace UnitTests.Application.LoggedWorkouts;

public sealed class ImportLoggedWorkoutsTests : ApplicationTestBase
{
    private static readonly Guid Bench = Guid.NewGuid();
    private static readonly Guid Run = Guid.NewGuid();
    private static readonly DateTime Monday = new(2026, 11, 2, 18, 30, 0, DateTimeKind.Utc);

    private static readonly List<ExerciseData> Catalog =
    [
        new() { Id = Bench, Name = "Bench Press", Type = "Strength" },
        new() { Id = Run, Name = "Treadmill Run", Type = "Cardio" },
    ];

    private static ImportWorkoutInput Push(DateTime at, string? sourceId = null, decimal weight = 80m) => new(
        "Push", at,
        [new LoggedExerciseInput(Bench, 0, [new LoggedSetInput("Strength", 0, null, null, null, 6, weight, "Kg", null)])],
        SourceId: sourceId);

    /// <param name="existing">The user's logs already in the app, built from their user id.</param>
    private async Task<ImportLoggedWorkouts> Setup(Func<Guid, IEnumerable<LoggedWorkoutData>>? existing = null, IEnumerable<ExerciseData>? catalog = null)
    {
        var user = User.Create("user@example.com", "alice", "hash", "Alice", "Smith", 72.5m, 180, 4, new DateOnly(1990, 1, 1));
        await UserRepository.Save(user);
        return new ImportLoggedWorkouts(
            UnitOfWork,
            StubCurrentUser.With(user.Id),
            new MockFindLoggedWorkoutsQuery(existing?.Invoke(user.Id.Value) ?? []),
            new MockSearchExerciseCatalogQuery(catalog ?? Catalog),
            CreateLogger<ImportLoggedWorkouts>());
    }

    [Fact]
    public async Task Execute_ShouldImportEveryNewWorkout()
    {
        var useCase = await Setup();

        var output = await useCase.Execute(new ImportLoggedWorkoutsInput([Push(Monday), Push(Monday.AddDays(2))]));

        Assert.Equal(2, output.Imported);
        Assert.Equal(0, output.Skipped);
        Assert.Empty(output.Failures);
    }

    [Fact]
    public async Task Execute_ShouldSkipWorkoutsYouAlreadyHave_SameNameSameMinute()
    {
        var useCase = await Setup(uid => [new LoggedWorkoutData { Id = Guid.NewGuid(), UserId = uid, Name = "push", LoggedAt = Monday.AddSeconds(20) }]);

        var output = await useCase.Execute(new ImportLoggedWorkoutsInput([Push(Monday)]));

        Assert.Equal(0, output.Imported);
        Assert.Equal(1, output.Skipped);
    }

    [Fact]
    public async Task Execute_ShouldSkipABackupOfYourOwnLogs_BySourceId()
    {
        var existingId = Guid.NewGuid();
        // Renamed since the backup was made, so only the id still matches.
        var useCase = await Setup(uid => [new LoggedWorkoutData { Id = existingId, UserId = uid, Name = "Chest day", LoggedAt = Monday }]);

        var output = await useCase.Execute(new ImportLoggedWorkoutsInput([Push(Monday, sourceId: existingId.ToString())]));

        Assert.Equal(1, output.Skipped);
        Assert.Equal(0, output.Imported);
    }

    [Fact]
    public async Task Execute_ImportingTheSameFileTwice_ShouldOnlyImportOnce()
    {
        var useCase = await Setup();
        var file = new ImportLoggedWorkoutsInput([Push(Monday, sourceId: "abc"), Push(Monday.AddDays(1), sourceId: "def")]);

        var first = await useCase.Execute(file);
        var second = await useCase.Execute(file);

        Assert.Equal(2, first.Imported);
        Assert.Equal(0, second.Imported);
        Assert.Equal(2, second.Skipped);
    }

    [Fact]
    public async Task Execute_ShouldKeepTwoWorkoutsFromTheFileEvenAtTheSameMinute()
    {
        var useCase = await Setup();

        var output = await useCase.Execute(new ImportLoggedWorkoutsInput([Push(Monday), Push(Monday)]));

        Assert.Equal(2, output.Imported);
        Assert.Equal(0, output.Skipped);
    }

    [Fact]
    public async Task Execute_ShouldReportUnknownExercisesWithoutFailingTheRest()
    {
        var useCase = await Setup();
        var unknown = new ImportWorkoutInput("Mystery", Monday.AddDays(3),
            [new LoggedExerciseInput(Guid.NewGuid(), 0, [new LoggedSetInput("Strength", 0, null, null, null, 5, 50m, "Kg", null)])]);

        var output = await useCase.Execute(new ImportLoggedWorkoutsInput([Push(Monday), unknown]));

        Assert.Equal(1, output.Imported);
        var failure = Assert.Single(output.Failures);
        Assert.Equal(1, failure.Index);
        Assert.Contains("Unknown exercise", failure.Reason);
    }

    [Fact]
    public async Task Execute_ShouldReportInvalidSetsWithoutFailingTheRest()
    {
        var useCase = await Setup();
        // A strength set needs reps.
        var badRun = new ImportWorkoutInput("Bench", Monday.AddDays(4),
            [new LoggedExerciseInput(Bench, 0, [new LoggedSetInput("Strength", 0, null, null, null, null, 60m, "Kg", null)])]);

        var output = await useCase.Execute(new ImportLoggedWorkoutsInput([badRun, Push(Monday)]));

        Assert.Equal(1, output.Imported);
        Assert.Equal(0, Assert.Single(output.Failures).Index);
    }

    [Fact]
    public async Task Execute_ShouldKeepBodyweightSetsAtZeroKg()
    {
        var useCase = await Setup();

        var output = await useCase.Execute(new ImportLoggedWorkoutsInput([Push(Monday, weight: 0m)]));

        Assert.Equal(1, output.Imported);
    }

    [Fact]
    public async Task Execute_ShouldNotUseAnotherUsersCustomExercise()
    {
        var foreign = new ExerciseData { Id = Guid.NewGuid(), Name = "Their Curl", Type = "Strength", OwnerUserId = Guid.NewGuid() };
        var useCase = await Setup(catalog: [.. Catalog, foreign]);
        var workout = new ImportWorkoutInput("Arms", Monday,
            [new LoggedExerciseInput(foreign.Id, 0, [new LoggedSetInput("Strength", 0, null, null, null, 8, 20m, "Kg", null)])]);

        var output = await useCase.Execute(new ImportLoggedWorkoutsInput([workout]));

        Assert.Equal(0, output.Imported);
        Assert.Single(output.Failures);
    }

    [Fact]
    public async Task Execute_ShouldRecordTheImportInTheHistory()
    {
        var useCase = await Setup();
        var exerciseId = Guid.NewGuid();
        var output = await useCase.Execute(new ImportLoggedWorkoutsInput([Push(Monday), Push(Monday.AddDays(1))], "my-log.csv", [exerciseId], []));

        Assert.NotNull(output.BatchId);
        var batch = (await ImportBatchRepository.ById(Bloom.Domain.Shared.EntityId.New<Bloom.Domain.Imports.ImportBatchId>(output.BatchId!.Value))).Value;
        Assert.Equal("my-log.csv", batch.FileName);
        Assert.Equal(output.ImportedIds, batch.WorkoutIds);
        Assert.Equal([exerciseId], batch.ExerciseIds);

        var list = await new ListImportBatches(UnitOfWork, StubCurrentUser.With(batch.UserId)).Execute(new ListImportBatchesInput());
        var summary = Assert.Single(list.Imports);
        Assert.Equal(2, summary.Workouts);
        Assert.Null(summary.UndoneAt);
    }

    [Fact]
    public async Task Execute_WhenNothingIsAdded_ShouldNotRecordAnImport()
    {
        var useCase = await Setup(uid => [new LoggedWorkoutData { Id = Guid.NewGuid(), UserId = uid, Name = "Push", LoggedAt = Monday }]);

        var output = await useCase.Execute(new ImportLoggedWorkoutsInput([Push(Monday)], "again.csv"));

        Assert.Null(output.BatchId);
    }

    [Fact]
    public async Task UndoBatch_ShouldRemoveWorkoutsAndUnusedNewExercises_KeepingOnesUsedElsewhere()
    {
        var useCase = await Setup();
        var output = await useCase.Execute(new ImportLoggedWorkoutsInput([Push(Monday), Push(Monday.AddDays(1))], "my-log.csv"));
        var userId = (await LoggedWorkoutRepository.ById(LogId(output.ImportedIds[0]))).Value.UserId;

        var unused = Bloom.Domain.Exercises.Exercise.CreateCustom(userId, "Morning jog", "Imported", Bloom.Domain.Exercises.Enums.ExerciseType.Cardio, []);
        var usedLater = Bloom.Domain.Exercises.Exercise.CreateCustom(userId, "Sled drag", "Imported", Bloom.Domain.Exercises.Enums.ExerciseType.Strength, []);
        await ExerciseRepository.Save(unused);
        await ExerciseRepository.Save(usedLater);
        var batch = Bloom.Domain.Imports.ImportBatch.Create(userId, "my-log.csv", output.ImportedIds, [unused.Id.Value, usedLater.Id.Value], []);
        await ImportBatchRepository.Save(batch);

        // A workout logged by hand after the import uses "Sled drag".
        var later = new LoggedWorkoutData
        {
            Id = Guid.NewGuid(), UserId = userId.Value, Name = "Legs", LoggedAt = Monday.AddDays(7),
            LoggedExercises = [new LoggedExerciseData { ExerciseId = usedLater.Id.Value }],
        };
        var undo = new UndoImportBatch(UnitOfWork, StubCurrentUser.With(userId),
            new MockFindLoggedWorkoutsQuery([later]), new UnitTests.Application.WorkoutTemplates.MockFindWorkoutTemplatesQuery([]),
            CreateLogger<UndoImportBatch>());

        var result = await undo.Execute(new UndoImportBatchInput(batch.Id.Value));

        Assert.Equal(new UndoImportBatchOutput(2, 0, 1, 1), result);
        foreach (var id in output.ImportedIds) Assert.False((await LoggedWorkoutRepository.ById(LogId(id))).HasValue);
        Assert.False((await ExerciseRepository.ById(unused.Id)).HasValue);
        Assert.True((await ExerciseRepository.ById(usedLater.Id)).HasValue);
        Assert.True(batch.IsUndone);

        // Undoing again changes nothing.
        Assert.Equal(new UndoImportBatchOutput(0, 0, 0, 0), await undo.Execute(new UndoImportBatchInput(batch.Id.Value)));
    }

    [Fact]
    public async Task UndoBatch_OfAnotherUser_ShouldBeNotFound()
    {
        var useCase = await Setup();
        var output = await useCase.Execute(new ImportLoggedWorkoutsInput([Push(Monday)], "mine.csv"));

        var undo = new UndoImportBatch(UnitOfWork, StubCurrentUser.Random(),
            new MockFindLoggedWorkoutsQuery([]), new UnitTests.Application.WorkoutTemplates.MockFindWorkoutTemplatesQuery([]),
            CreateLogger<UndoImportBatch>());

        await Assert.ThrowsAsync<ImportBatchNotFoundException>(() => undo.Execute(new UndoImportBatchInput(output.BatchId!.Value)));
        Assert.True((await LoggedWorkoutRepository.ById(LogId(output.ImportedIds[0]))).HasValue);
    }

    [Fact]
    public async Task UndoBatch_ShouldRemoveItsBodyEntries_AndCurrentWeightFallsBack()
    {
        var user = User.Create("body@example.com", "bea", "hash", "Bea", "Smith", 70m, 170, 3, new DateOnly(1992, 3, 3));
        await UserRepository.Save(user);
        var own = Bloom.Domain.Body.BodyMeasurement.Create(user.Id, new DateOnly(2026, 10, 1), 71m, null, Bloom.Domain.Body.BodyMeasurementSource.Manual);
        var imported = Bloom.Domain.Body.BodyMeasurement.Create(user.Id, new DateOnly(2026, 11, 1), 69m, 20m, Bloom.Domain.Body.BodyMeasurementSource.Import);
        await BodyMeasurementRepository.Save(own);
        await BodyMeasurementRepository.Save(imported);
        user.SyncWeight(69m);
        var batch = Bloom.Domain.Imports.ImportBatch.Create(user.Id, "scale.csv", [], [], [], [imported.Id.Value]);
        await ImportBatchRepository.Save(batch);

        var undo = new UndoImportBatch(UnitOfWork, StubCurrentUser.With(user.Id),
            new MockFindLoggedWorkoutsQuery([]), new UnitTests.Application.WorkoutTemplates.MockFindWorkoutTemplatesQuery([]),
            CreateLogger<UndoImportBatch>());
        var result = await undo.Execute(new UndoImportBatchInput(batch.Id.Value));

        Assert.Equal(1, result.BodyEntries);
        Assert.Equal([own.Id], (await BodyMeasurementRepository.ForUser(user.Id)).Select(m => m.Id));
        Assert.Equal(71m, (await UserRepository.ById(user.Id)).Value.Weight);
    }

    private static Bloom.Domain.LoggedWorkouts.LoggedWorkoutId LogId(Guid id) =>
        Bloom.Domain.Shared.EntityId.New<Bloom.Domain.LoggedWorkouts.LoggedWorkoutId>(id);

    [Fact]
    public async Task Execute_WithMissingUser_ShouldThrow()
    {
        var useCase = new ImportLoggedWorkouts(UnitOfWork, StubCurrentUser.Random(),
            new MockFindLoggedWorkoutsQuery([]), new MockSearchExerciseCatalogQuery(Catalog), CreateLogger<ImportLoggedWorkouts>());

        await Assert.ThrowsAsync<UserNotFoundException>(() => useCase.Execute(new ImportLoggedWorkoutsInput([Push(Monday)])));
    }
}
