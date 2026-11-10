using Bloom.Application.Contracts;
using Bloom.Application.Leaderboard;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.LoggedWorkouts.Enums;
using Bloom.Domain.LoggedWorkouts.ValueObjects;
using Bloom.Shared.Exceptions;
using UnitTests.Application.LoggedWorkouts;
using UnitTests.Application.Mocks;
using UnitTests.Application.Users;

namespace UnitTests.Application.Leaderboard;

public sealed class GetMonthlyLeaderboardTests
{
    private readonly Guid _alice = Guid.NewGuid();
    private readonly Guid _bob = Guid.NewGuid();
    private readonly Guid _bench = Guid.NewGuid();
    private readonly Guid _squat = Guid.NewGuid();

    private static readonly DateTime March = new(2026, 3, 10, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime February = new(2026, 2, 10, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime April = new(2026, 4, 2, 18, 0, 0, DateTimeKind.Utc);

    private static UserData User(Guid id, string username) => new() { Id = id, Username = username };

    private static LoggedSetData Set(int reps, decimal weight, WeightUnit unit = WeightUnit.Kg, SetMarker? marker = null) => new()
    {
        Id = Guid.NewGuid(),
        Type = ExerciseType.Strength,
        Reps = reps,
        Weight = new WeightData { Value = weight, Unit = unit },
        Marker = marker
    };

    private static LoggedWorkoutData Log(Guid userId, DateTime at, Guid exerciseId, params LoggedSetData[] sets) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        LoggedAt = at,
        Name = "Session",
        LoggedExercises = [new LoggedExerciseData { Id = Guid.NewGuid(), ExerciseId = exerciseId, Sets = sets.ToList() }]
    };

    private GetMonthlyLeaderboard BuildUseCase(Guid currentUser, IEnumerable<LoggedWorkoutData> logs, params UserData[] users) =>
        new(StubCurrentUser.With(currentUser), new MockFindUsersQuery(users), new MockFindLoggedWorkoutsQuery(logs));

    [Fact]
    public async Task Execute_ShouldSumWorkingSetVolumeAndExcludeWarmUps()
    {
        var logs = new[]
        {
            Log(_alice, March, _bench, Set(10, 50m), Set(5, 100m), Set(10, 20m, marker: SetMarker.WarmUp))
        };

        var output = await BuildUseCase(_alice, logs, User(_alice, "alice"))
            .Execute(new GetMonthlyLeaderboardInput(2026, 3));

        var entry = Assert.Single(output.Entries);
        Assert.Equal(1000m, entry.TotalVolumeKg);
        Assert.Equal(1, entry.Workouts);
        Assert.True(entry.IsCurrentUser);
    }

    [Fact]
    public async Task Execute_ShouldConvertPoundsToKilograms()
    {
        var logs = new[] { Log(_alice, March, _bench, Set(10, 100m, WeightUnit.Lbs)) };

        var output = await BuildUseCase(_alice, logs, User(_alice, "alice"))
            .Execute(new GetMonthlyLeaderboardInput(2026, 3));

        Assert.Equal(453.6m, output.Entries[0].TotalVolumeKg);
    }

    [Fact]
    public async Task Execute_ShouldOnlyCountLogsInsideTheRequestedMonth()
    {
        var logs = new[]
        {
            Log(_alice, February, _bench, Set(10, 100m)),
            Log(_alice, March, _bench, Set(1, 10m)),
            Log(_alice, April, _bench, Set(10, 100m)),
        };

        var output = await BuildUseCase(_alice, logs, User(_alice, "alice"))
            .Execute(new GetMonthlyLeaderboardInput(2026, 3));

        var entry = Assert.Single(output.Entries);
        Assert.Equal(10m, entry.TotalVolumeKg);
        Assert.Equal(1, entry.Workouts);
        Assert.Equal(1, entry.ActiveDays);
    }

    [Fact]
    public async Task Execute_ShouldCountDistinctActiveDays()
    {
        var logs = new[]
        {
            Log(_alice, March, _bench, Set(5, 50m)),
            Log(_alice, March.AddHours(2), _squat, Set(5, 50m)),
            Log(_alice, March.AddDays(1), _bench, Set(5, 50m)),
        };

        var output = await BuildUseCase(_alice, logs, User(_alice, "alice"))
            .Execute(new GetMonthlyLeaderboardInput(2026, 3));

        Assert.Equal(2, output.Entries[0].ActiveDays);
        Assert.Equal(3, output.Entries[0].Workouts);
    }

    [Fact]
    public async Task Execute_ShouldCountPrOnlyWhenBeatingEarlierHistory()
    {
        var logs = new[]
        {
            Log(_alice, February, _bench, Set(5, 100m)),
            Log(_alice, February, _squat, Set(5, 140m)),
            Log(_alice, March, _bench, Set(3, 105m)),   // beats Feb → PR
            Log(_alice, March, _squat, Set(3, 140m)),   // ties Feb → not a PR
            Log(_alice, March, Guid.NewGuid(), Set(3, 60m)), // first time ever → not a PR
        };

        var output = await BuildUseCase(_alice, logs, User(_alice, "alice"))
            .Execute(new GetMonthlyLeaderboardInput(2026, 3));

        Assert.Equal(1, output.Entries[0].NewPrs);
    }

    [Fact]
    public async Task Execute_ShouldIgnoreWarmUpsWhenDetectingPrs()
    {
        var logs = new[]
        {
            Log(_alice, February, _bench, Set(5, 100m)),
            Log(_alice, March, _bench, Set(1, 120m, marker: SetMarker.WarmUp)),
        };

        var output = await BuildUseCase(_alice, logs, User(_alice, "alice"))
            .Execute(new GetMonthlyLeaderboardInput(2026, 3));

        Assert.Equal(0, output.Entries[0].NewPrs);
    }

    [Fact]
    public async Task Execute_ShouldRankByVolumeAndIncludeInactiveUsers()
    {
        var carol = Guid.NewGuid();
        var logs = new[]
        {
            Log(_alice, March, _bench, Set(10, 50m)),
            Log(_bob, March, _bench, Set(10, 100m)),
        };

        var output = await BuildUseCase(_alice, logs, User(_alice, "alice"), User(_bob, "bob"), User(carol, "carol"))
            .Execute(new GetMonthlyLeaderboardInput(2026, 3));

        Assert.Equal(["bob", "alice", "carol"], output.Entries.Select(e => e.Username));
        Assert.Equal(0m, output.Entries[2].TotalVolumeKg);
        Assert.Single(output.Entries, e => e.IsCurrentUser);
    }

    [Theory]
    [InlineData(2026, 0)]
    [InlineData(2026, 13)]
    [InlineData(1999, 5)]
    public async Task Execute_WithInvalidPeriod_ShouldThrow(int year, int month)
    {
        var useCase = BuildUseCase(_alice, [], User(_alice, "alice"));

        await Assert.ThrowsAsync<BloomGeneralException>(
            () => useCase.Execute(new GetMonthlyLeaderboardInput(year, month)));
    }
}
