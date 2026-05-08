using System.Linq.Expressions;
using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Ports;

namespace UnitTests.Application.Mocks;

public sealed class MockSearchExerciseCatalogQuery(IEnumerable<ExerciseData> data) : ISearchExerciseCatalogQuery
{
    private readonly List<ExerciseData> _data = data.ToList();

    public Task<IReadOnlyList<ExerciseData>> Fetch(Expression<Func<ExerciseData, bool>> filter)
    {
        IReadOnlyList<ExerciseData> filtered = _data.AsQueryable().Where(filter).ToList();
        return Task.FromResult(filtered);
    }
}
