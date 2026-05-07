using System.Linq.Expressions;
using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Ports;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Queries;

public class FindWorkoutTemplatesQuery(QueryDbContext context) : IFindWorkoutTemplatesQuery
{
    public async Task<IReadOnlyList<WorkoutTemplateData>> Fetch(
        Expression<Func<WorkoutTemplateData, bool>> filter
    )
    {
        return await context.WorkoutTemplates
            .Where(filter)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }
}