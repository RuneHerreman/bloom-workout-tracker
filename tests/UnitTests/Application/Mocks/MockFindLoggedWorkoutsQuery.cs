using System.Linq.Expressions;
using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Ports;

namespace UnitTests.Application.Mocks;

public sealed class MockFindLoggedWorkoutsQuery(IEnumerable<LoggedWorkoutData> data) : IFindLoggedWorkoutsQuery
{
    private readonly List<LoggedWorkoutData> _data = data.ToList();

    public Task<IReadOnlyList<LoggedWorkoutData>> Fetch(Expression<Func<LoggedWorkoutData, bool>> filter)
    {
        IReadOnlyList<LoggedWorkoutData> filtered = _data.AsQueryable().Where(filter).ToList();
        return Task.FromResult(filtered);
    }
}
