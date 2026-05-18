using Bloom.Domain.Shared;
using Bloom.Domain.Users;

namespace Bloom.Domain.LoggedWorkouts;

public interface ILoggedWorkoutRepository: IRepository<LoggedWorkout, LoggedWorkoutId>
{
    Task<bool> ExistsWithExternalId(UserId userId, string externalId, CancellationToken ct = default);
}