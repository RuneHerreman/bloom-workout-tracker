using System.Linq.Expressions;
using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Ports;

namespace UnitTests.Application.Mocks;

public sealed class MockFindWorkoutTemplatesQuery(IEnumerable<WorkoutTemplateData> data) : IFindWorkoutTemplatesQuery
{
    private readonly List<WorkoutTemplateData> _data = data.ToList();

    public Task<IReadOnlyList<WorkoutTemplateData>> Fetch(Expression<Func<WorkoutTemplateData, bool>> filter)
    {
        IReadOnlyList<WorkoutTemplateData> filtered = _data.AsQueryable().Where(filter).ToList();
        return Task.FromResult(filtered);
    }
}
