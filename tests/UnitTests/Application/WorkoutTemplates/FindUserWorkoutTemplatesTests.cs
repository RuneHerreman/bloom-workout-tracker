using Bloom.Application.Contracts;
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
        var useCase = new FindUserWorkoutTemplates(new MockFindWorkoutTemplatesQuery(Sample(userA, userB)));

        var output = await useCase.Execute(new FindUserWorkoutTemplatesInput(userA, null));

        Assert.Equal(2, output.Templates.Count);
    }

    [Fact]
    public async Task Execute_WithNameFilter_ShouldFilter()
    {
        Guid userA = Guid.NewGuid();
        Guid userB = Guid.NewGuid();
        var useCase = new FindUserWorkoutTemplates(new MockFindWorkoutTemplatesQuery(Sample(userA, userB)));

        var output = await useCase.Execute(new FindUserWorkoutTemplatesInput(userA, "push"));

        Assert.Single(output.Templates);
        Assert.Equal("Push Day", output.Templates[0].Name);
    }
}
