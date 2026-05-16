using System.Linq.Expressions;
using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.WorkoutTemplates;
using UnitTests.Application.Mocks;

namespace UnitTests.Application.WorkoutTemplates;

public sealed class FindUserWorkoutTemplatesTests
{
    private static List<WorkoutTemplateData> Sample(Guid userA, Guid userB) =>
    [
        new() { Id = Guid.NewGuid(), UserId = userA, Name = "Push Day" },
        new() { Id = Guid.NewGuid(), UserId = userA, Name = "Pull Day" },
        new() { Id = Guid.NewGuid(), UserId = userB, Name = "Push Day" }
    ];

    [Fact]
    public async Task Execute_WithoutNameFilter_ShouldReturnAllUserTemplates()
    {
        Guid userA = Guid.NewGuid();
        Guid userB = Guid.NewGuid();
        var useCase = new FindUserWorkoutTemplates(
            StubCurrentUser.With(userA),
            new MockFindWorkoutTemplatesQuery(Sample(userA, userB))
        );

        var output = await useCase.Execute(new FindUserWorkoutTemplatesInput(null));

        Assert.Equal(2, output.Templates.Count);
    }

    [Fact]
    public async Task Execute_WithNameFilter_ShouldFilter()
    {
        Guid userA = Guid.NewGuid();
        Guid userB = Guid.NewGuid();
        var useCase = new FindUserWorkoutTemplates(
            StubCurrentUser.With(userA),
            new MockFindWorkoutTemplatesQuery(Sample(userA, userB))
        );

        var output = await useCase.Execute(new FindUserWorkoutTemplatesInput("push"));

        Assert.Single(output.Templates);
        Assert.Equal("Push Day", output.Templates[0].Name);
    }
}

public sealed class MockFindWorkoutTemplatesQuery(IEnumerable<WorkoutTemplateData> data) : IFindWorkoutTemplatesQuery
{
    private readonly List<WorkoutTemplateData> _data = data.ToList();

    public Task<IReadOnlyList<WorkoutTemplateData>> Fetch(Expression<Func<WorkoutTemplateData, bool>> filter, CancellationToken ct = default)
    {
        IReadOnlyList<WorkoutTemplateData> filtered = _data.AsQueryable().Where(filter).ToList();
        return Task.FromResult(filtered);
    }
}
