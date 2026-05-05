using System.Linq.Expressions;
using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Ports;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Queries;

public class SearchExerciseCatalogQuery(QueryDbContext context): ISearchExerciseCatalogQuery
{
    public async Task<IReadOnlyList<ExerciseData>> Fetch(Expression<Func<ExerciseData, bool>> filter)
    {
        return await context.Exercises
            .Where(filter)
            .OrderBy(e => e.TargetMuscles)
            .ToListAsync();
    }
}