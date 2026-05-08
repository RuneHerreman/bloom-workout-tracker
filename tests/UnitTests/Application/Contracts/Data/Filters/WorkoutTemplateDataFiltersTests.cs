using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Data.Filters;
using Bloom.Domain.Shared;
using Bloom.Domain.WorkoutTemplates;

namespace UnitTests.Application.Contracts.Data.Filters;

public sealed class WorkoutTemplateDataFiltersTests
{
    [Fact]
    public void ByProperty_NoName_ShouldFilterByUserId()
    {
        Guid userA = Guid.NewGuid();
        Guid userB = Guid.NewGuid();
        var data = new List<WorkoutTemplateData>
        {
            new() { Id = Guid.NewGuid(), UserId = userA, Name = "Push" },
            new() { Id = Guid.NewGuid(), UserId = userB, Name = "Pull" }
        };

        var filter = WorkoutTemplateDataFilters.ByProperty(userA, null);
        var result = data.AsQueryable().Where(filter).ToList();

        Assert.Single(result);
    }

    [Fact]
    public void ByProperty_WithName_ShouldFilterByUserAndName()
    {
        Guid userA = Guid.NewGuid();
        var data = new List<WorkoutTemplateData>
        {
            new() { Id = Guid.NewGuid(), UserId = userA, Name = "Push Day" },
            new() { Id = Guid.NewGuid(), UserId = userA, Name = "Pull Day" }
        };

        var filter = WorkoutTemplateDataFilters.ByProperty(userA, "push");
        var result = data.AsQueryable().Where(filter).ToList();

        Assert.Single(result);
        Assert.Equal("Push Day", result[0].Name);
    }

    [Fact]
    public void ByProperty_WithWhitespaceName_ShouldFallBackToUserOnly()
    {
        Guid userA = Guid.NewGuid();
        var data = new List<WorkoutTemplateData>
        {
            new() { Id = Guid.NewGuid(), UserId = userA, Name = "Push" },
            new() { Id = Guid.NewGuid(), UserId = userA, Name = "Pull" }
        };

        var filter = WorkoutTemplateDataFilters.ByProperty(userA, "   ");
        var result = data.AsQueryable().Where(filter).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ById_WithValidId_ShouldFilter()
    {
        Guid id = Guid.NewGuid();
        var data = new List<WorkoutTemplateData>
        {
            new() { Id = id },
            new() { Id = Guid.NewGuid() }
        };

        var filter = WorkoutTemplateDataFilters.ById(EntityId.New<WorkoutTemplateId>(id));
        var result = data.AsQueryable().Where(filter).ToList();

        Assert.Single(result);
    }

    [Fact]
    public void ById_WithEmptyGuid_ShouldReturnNothing()
    {
        var data = new List<WorkoutTemplateData> { new() { Id = Guid.NewGuid() } };

        var filter = WorkoutTemplateDataFilters.ById(EntityId.New<WorkoutTemplateId>(Guid.Empty));
        var result = data.AsQueryable().Where(filter).ToList();

        Assert.Empty(result);
    }
}
