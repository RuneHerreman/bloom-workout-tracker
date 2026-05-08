using System.Linq.Expressions;
using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Ports;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Queries;

public class FindUsersQuery(QueryDbContext context) : IFindUsersQuery
{
    public async Task<IReadOnlyList<UserData>> Fetch(Expression<Func<UserData, bool>> filter)
    {
        return await context.Users
            .Where(filter)
            .ToListAsync();
    }
}
