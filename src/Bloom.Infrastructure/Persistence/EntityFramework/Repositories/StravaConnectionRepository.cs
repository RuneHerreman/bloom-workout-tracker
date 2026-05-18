using Aornis;
using Bloom.Domain.Strava;
using Bloom.Domain.Users;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Repositories;

public class StravaConnectionRepository(DomainDbContext context)
    : EfCoreGenericRepository<StravaConnection, StravaConnectionId>(context), IStravaConnectionRepository
{
    public async Task<Optional<StravaConnection>> ByUserId(UserId userId, CancellationToken ct = default)
    {
        var conn = await _context.StravaConnections
            .FirstOrDefaultAsync(sc => sc.UserId == userId, ct);
        return Optional.Of(conn);
    }

    public async Task<Optional<StravaConnection>> ByStravaAthleteId(long stravaAthleteId, CancellationToken ct = default)
    {
        var conn = await _context.StravaConnections
            .FirstOrDefaultAsync(sc => sc.StravaAthleteId == stravaAthleteId, ct);
        return Optional.Of(conn);
    }
}
