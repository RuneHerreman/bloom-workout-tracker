using Bloom.Domain.Strava;
using Bloom.Domain.Users;

namespace Bloom.Application.Contracts.Ports;

public interface IStravaActivityImporter
{
    Task<int> ImportAll(StravaConnection connection, UserId userId, DateTime? after = null, CancellationToken ct = default);
    Task<bool> ImportSingle(StravaConnection connection, UserId userId, long activityId, CancellationToken ct = default);
}
