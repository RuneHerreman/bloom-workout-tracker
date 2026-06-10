using Aornis;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;

namespace Bloom.Domain.Exercises;

public interface IExerciseRepository : IRepository<Exercise, ExerciseId>
{
    Task<Optional<Exercise>> ByName(string name, CancellationToken ct = default);

    /// <summary>
    /// Finds an exercise by name that is visible to the given user: either a global
    /// catalog exercise or one the user owns. Used to enforce per-user name uniqueness
    /// without colliding across different users' custom exercises.
    /// </summary>
    Task<Optional<Exercise>> ByNameForUser(string name, UserId ownerUserId, CancellationToken ct = default);
}