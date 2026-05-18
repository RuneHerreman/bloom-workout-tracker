using Aornis;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;

namespace Bloom.Domain.Strava;

public interface IStravaConnectionRepository : IRepository<StravaConnection, StravaConnectionId>
{
    Task<Optional<StravaConnection>> ByUserId(UserId userId, CancellationToken ct = default);
    Task<Optional<StravaConnection>> ByStravaAthleteId(long stravaAthleteId, CancellationToken ct = default);
}
