using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.Users;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Repositories;

public class LoggedWorkoutRepository(DomainDbContext context) : EfCoreGenericRepository<LoggedWorkout, LoggedWorkoutId>(context), ILoggedWorkoutRepository
{
    public async Task<IReadOnlySet<string>> GetExistingExternalIds(UserId userId, IEnumerable<string> externalIds, CancellationToken ct = default)
    {
        var ids = externalIds.ToList();
        var existing = await _context.LoggedWorkouts
            .Where(lw => lw.UserId == userId && lw.ExternalId != null && ids.Contains(lw.ExternalId))
            .Select(lw => lw.ExternalId!)
            .ToListAsync(ct);
        return existing.ToHashSet();
    }
}