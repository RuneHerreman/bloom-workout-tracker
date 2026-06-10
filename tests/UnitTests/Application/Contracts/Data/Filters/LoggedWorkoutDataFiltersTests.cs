using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Data.Filters;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.Shared;

namespace UnitTests.Application.Contracts.Data.Filters;

public sealed class LoggedWorkoutDataFiltersTests
{
    [Fact]
    public void ByProperty_ShouldFilterByUserId()
    {
        Guid userA = Guid.NewGuid();
        Guid userB = Guid.NewGuid();
        var data = new List<LoggedWorkoutData>
        {
            new() { Id = Guid.NewGuid(), UserId = userA },
            new() { Id = Guid.NewGuid(), UserId = userB }
        };

        var filter = LoggedWorkoutDataFilters.ByProperty(userA);
        var result = data.AsQueryable().Where(filter).ToList();

        Assert.Single(result);
        Assert.Equal(userA, result[0].UserId);
    }

    [Fact]
    public void ByProperty_WithName_ShouldFilterCaseInsensitive()
    {
        Guid userId = Guid.NewGuid();
        var data = new List<LoggedWorkoutData>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, Name = "Push Day" },
            new() { Id = Guid.NewGuid(), UserId = userId, Name = "Leg Day" }
        };

        var filter = LoggedWorkoutDataFilters.ByProperty(userId, name: "push");
        var result = data.AsQueryable().Where(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Push Day", result[0].Name);
    }

    [Fact]
    public void ByProperty_WithDateRange_ShouldFilterByLoggedAt()
    {
        Guid userId = Guid.NewGuid();
        var data = new List<LoggedWorkoutData>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, LoggedAt = new DateTime(2026, 1, 10) },
            new() { Id = Guid.NewGuid(), UserId = userId, LoggedAt = new DateTime(2026, 2, 15) },
            new() { Id = Guid.NewGuid(), UserId = userId, LoggedAt = new DateTime(2026, 3, 20) }
        };

        var filter = LoggedWorkoutDataFilters.ByProperty(
            userId,
            from: new DateTime(2026, 2, 1),
            to: new DateTime(2026, 2, 28));
        var result = data.AsQueryable().Where(filter).ToList();

        Assert.Single(result);
        Assert.Equal(new DateTime(2026, 2, 15), result[0].LoggedAt);
    }

    [Fact]
    public void ByProperty_WithOnlyFrom_ShouldFilterFromDateOnward()
    {
        Guid userId = Guid.NewGuid();
        var data = new List<LoggedWorkoutData>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, LoggedAt = new DateTime(2026, 1, 10) },
            new() { Id = Guid.NewGuid(), UserId = userId, LoggedAt = new DateTime(2026, 3, 20) }
        };

        var filter = LoggedWorkoutDataFilters.ByProperty(userId, from: new DateTime(2026, 2, 1));
        var result = data.AsQueryable().Where(filter).ToList();

        Assert.Single(result);
        Assert.Equal(new DateTime(2026, 3, 20), result[0].LoggedAt);
    }

    [Fact]
    public void ByProperty_WithGear_ShouldMatchLogsThatUsedThatGear()
    {
        Guid userId = Guid.NewGuid();
        var data = new List<LoggedWorkoutData>
        {
            new()
            {
                Id = Guid.NewGuid(), UserId = userId, Name = "Run",
                LoggedExercises =
                [
                    new LoggedExerciseData { ExerciseId = Guid.NewGuid(), Order = 0, Gear = ["Nike Vaporfly", "Garmin Forerunner"] }
                ]
            },
            new()
            {
                Id = Guid.NewGuid(), UserId = userId, Name = "Bench",
                LoggedExercises =
                [
                    new LoggedExerciseData { ExerciseId = Guid.NewGuid(), Order = 0, Gear = ["Lifting Belt"] }
                ]
            }
        };

        var filter = LoggedWorkoutDataFilters.ByProperty(userId, gear: "Nike Vaporfly");
        var result = data.AsQueryable().Where(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Run", result[0].Name);
    }

    [Fact]
    public void ByProperty_WithAllFilters_ShouldApplyTogether()
    {
        Guid userId = Guid.NewGuid();
        var data = new List<LoggedWorkoutData>
        {
            new()
            {
                Id = Guid.NewGuid(), UserId = userId, Name = "Morning Run",
                LoggedAt = new DateTime(2026, 2, 10),
                LoggedExercises = [new LoggedExerciseData { ExerciseId = Guid.NewGuid(), Order = 0, Gear = ["Nike Vaporfly"] }]
            },
            // Right gear + name but out of date range
            new()
            {
                Id = Guid.NewGuid(), UserId = userId, Name = "Morning Run",
                LoggedAt = new DateTime(2026, 5, 10),
                LoggedExercises = [new LoggedExerciseData { ExerciseId = Guid.NewGuid(), Order = 0, Gear = ["Nike Vaporfly"] }]
            }
        };

        var filter = LoggedWorkoutDataFilters.ByProperty(
            userId,
            name: "run",
            from: new DateTime(2026, 1, 1),
            to: new DateTime(2026, 3, 1),
            gear: "Nike Vaporfly");
        var result = data.AsQueryable().Where(filter).ToList();

        Assert.Single(result);
        Assert.Equal(new DateTime(2026, 2, 10), result[0].LoggedAt);
    }

    [Fact]
    public void ByProperty_WithNoOptionalFilters_ShouldReturnAllForUser()
    {
        Guid userId = Guid.NewGuid();
        var data = new List<LoggedWorkoutData>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, Name = "A" },
            new() { Id = Guid.NewGuid(), UserId = userId, Name = "B" },
            new() { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Name = "C" }
        };

        var filter = LoggedWorkoutDataFilters.ByProperty(userId);
        var result = data.AsQueryable().Where(filter).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ById_WithValidId_ShouldFilter()
    {
        Guid id = Guid.NewGuid();
        var data = new List<LoggedWorkoutData>
        {
            new() { Id = id },
            new() { Id = Guid.NewGuid() }
        };

        var filter = LoggedWorkoutDataFilters.ById(EntityId.New<LoggedWorkoutId>(id));
        var result = data.AsQueryable().Where(filter).ToList();

        Assert.Single(result);
    }

    [Fact]
    public void ById_WithEmptyGuid_ShouldReturnNothing()
    {
        var data = new List<LoggedWorkoutData> { new() { Id = Guid.NewGuid() } };

        var filter = LoggedWorkoutDataFilters.ById(EntityId.New<LoggedWorkoutId>(Guid.Empty));
        var result = data.AsQueryable().Where(filter).ToList();

        Assert.Empty(result);
    }
}
