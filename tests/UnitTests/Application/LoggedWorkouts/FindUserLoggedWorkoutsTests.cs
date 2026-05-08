using Bloom.Application.Contracts;
using Bloom.Application.LoggedWorkouts;
using UnitTests.Application.Mocks;

namespace UnitTests.Application.LoggedWorkouts;

public sealed class FindUserLoggedWorkoutsTests
{
    [Fact]
    public async Task Execute_ShouldReturnLogsBelongingToUser()
    {
        Guid userA = Guid.NewGuid();
        Guid userB = Guid.NewGuid();
        var data = new List<LoggedWorkoutData>
        {
            new() { Id = Guid.NewGuid(), UserId = userA },
            new() { Id = Guid.NewGuid(), UserId = userA },
            new() { Id = Guid.NewGuid(), UserId = userB }
        };
        var useCase = new FindUserLoggedWorkouts(new MockFindLoggedWorkoutsQuery(data));

        var output = await useCase.Execute(new FindUserLoggedWorkoutsInput(userA));

        Assert.Equal(2, output.Logs.Count);
        Assert.All(output.Logs, l => Assert.Equal(userA, l.UserId));
    }
}
