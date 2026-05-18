using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.Users;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Repositories;

public class LoggedWorkoutRepository(DomainDbContext context) : EfCoreGenericRepository<LoggedWorkout, LoggedWorkoutId>(context), ILoggedWorkoutRepository
{
    public Task<bool> ExistsWithExternalId(UserId userId, string externalId, CancellationToken ct = default)
        => _context.LoggedWorkouts
            .AnyAsync(lw => lw.UserId == userId && lw.ExternalId == externalId, ct);
}