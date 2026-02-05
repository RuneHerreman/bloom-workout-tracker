using Bloom.Domain.Entity;

namespace Bloom.Domain.Repositories;

public interface IExerciseRepository
{
    Task<List<Exercise>> GetByIdsAsync(List<Guid> exerciseIds, CancellationToken ct);
}