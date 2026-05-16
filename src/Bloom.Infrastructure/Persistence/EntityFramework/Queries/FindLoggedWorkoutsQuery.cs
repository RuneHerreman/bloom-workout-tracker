using System.Linq.Expressions;
using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Ports;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Queries;

public class FindLoggedWorkoutsQuery(QueryDbContext context) : IFindLoggedWorkoutsQuery
{
    public async Task<IReadOnlyList<LoggedWorkoutData>> Fetch(
        Expression<Func<LoggedWorkoutData, bool>> filter,
        CancellationToken ct = default
    )
    {
        return await context.LoggedWorkouts
            .Where(filter)
            .OrderByDescending(l => l.LoggedAt)
            .ToListAsync(ct);
    }
}