using Aornis;
using Bloom.Domain.Shared;

namespace Bloom.Domain.Exercises;

public interface IExerciseRepository : IRepository<Exercise, ExerciseId>
{
    Task<Optional<Exercise>> ByName(string name, CancellationToken ct = default);
}