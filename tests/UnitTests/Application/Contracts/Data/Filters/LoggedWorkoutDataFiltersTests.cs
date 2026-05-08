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
