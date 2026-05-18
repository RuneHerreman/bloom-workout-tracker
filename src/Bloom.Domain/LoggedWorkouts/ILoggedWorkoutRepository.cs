using Bloom.Domain.Shared;
using Bloom.Domain.Users;

namespace Bloom.Domain.LoggedWorkouts;

public interface ILoggedWorkoutRepository: IRepository<LoggedWorkout, LoggedWorkoutId>
{
    Task<IReadOnlySet<string>> GetExistingExternalIds(UserId userId, IEnumerable<string> externalIds, CancellationToken ct = default);
}