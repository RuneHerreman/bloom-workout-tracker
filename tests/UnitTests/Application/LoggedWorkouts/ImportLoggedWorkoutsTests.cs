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
        // A cardio set needs a distance.
        var badRun = new ImportWorkoutInput("Run", Monday.AddDays(4),
            [new LoggedExerciseInput(Run, 0, [new LoggedSetInput("Cardio", 0, TimeSpan.FromMinutes(20), null, null, null, null, null, null)])]);

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
    public async Task Execute_ShouldReturnCreatedIds_AndUndoShouldRemoveExactlyThose()
    {
        var useCase = await Setup();
        var output = await useCase.Execute(new ImportLoggedWorkoutsInput([Push(Monday), Push(Monday.AddDays(1))]));
        Assert.Equal(2, output.ImportedIds.Count);

        var owner = (await LoggedWorkoutRepository.ById(LogId(output.ImportedIds[0]))).Value.UserId;
        var undo = new UndoImportedWorkouts(UnitOfWork, StubCurrentUser.With(owner), CreateLogger<UndoImportedWorkouts>());
        var result = await undo.Execute(new UndoImportedWorkoutsInput([.. output.ImportedIds]));

        Assert.Equal(2, result.Deleted);
        foreach (var id in output.ImportedIds)
            Assert.False((await LoggedWorkoutRepository.ById(LogId(id))).HasValue);
    }

    [Fact]
    public async Task Undo_ShouldIgnoreWorkoutsOfOtherUsers()
    {
        var useCase = await Setup();
        var output = await useCase.Execute(new ImportLoggedWorkoutsInput([Push(Monday)]));

        var undo = new UndoImportedWorkouts(UnitOfWork, StubCurrentUser.Random(), CreateLogger<UndoImportedWorkouts>());
        var result = await undo.Execute(new UndoImportedWorkoutsInput([.. output.ImportedIds]));

        Assert.Equal(0, result.Deleted);
        Assert.True((await LoggedWorkoutRepository.ById(LogId(output.ImportedIds[0]))).HasValue);
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
