using System.Linq.Expressions;
using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Ports;
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
        var useCase = new FindUserLoggedWorkouts(
            StubCurrentUser.With(userA),
            new MockFindLoggedWorkoutsQuery(data)
        );

        var output = await useCase.Execute(new FindUserLoggedWorkoutsInput());

        Assert.Equal(2, output.Logs.Count);
        Assert.All(output.Logs, l => Assert.Equal(userA, l.UserId));
    }
}

public sealed class MockFindLoggedWorkoutsQuery(IEnumerable<LoggedWorkoutData> data) : IFindLoggedWorkoutsQuery
{
    private readonly List<LoggedWorkoutData> _data = data.ToList();

    public Task<IReadOnlyList<LoggedWorkoutData>> Fetch(Expression<Func<LoggedWorkoutData, bool>> filter)
    {
        IReadOnlyList<LoggedWorkoutData> filtered = _data.AsQueryable().Where(filter).ToList();
        return Task.FromResult(filtered);
    }
}
