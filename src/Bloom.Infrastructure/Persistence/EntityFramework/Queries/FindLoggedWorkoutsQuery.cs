using System.Linq.Expressions;
using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Ports;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Queries;

public class FindLoggedWorkoutsQuery(DomainDbContext context) : IFindLoggedWorkoutsQuery
{
    public async Task<IReadOnlyList<LoggedWorkoutData>> Fetch(
        Expression<Func<LoggedWorkoutData, bool>> filter
    )
    {
        var logs = await context.LoggedWorkouts
            .AsNoTracking()
            .ToListAsync();

        var data = logs.Select(l => new LoggedWorkoutData
        {
            Id = l.Id.Value,
            UserId = l.UserId.Value,
            LoggedAt = l.LoggedAt,
            Exercises = l.LoggedExercises.Select(e => new LoggedExerciseData
            {
                ExerciseId = e.ExerciseId.Value,
                Order = e.Order,
                Sets = e.Sets.Select(s => new LoggedSetData
                {
                    Type = s.Type.ToString(),
                    Order = s.Order,
                    Duration = s.Duration?.Value,
                    Distance = s.Distance?.Value,
                    DistanceUnit = s.Distance?.Unit.ToString(),
                    Reps = s.Reps?.Value,
                    Weight = s.Weight?.Value,
                    WeightUnit = s.Weight?.Unit.ToString(),
                    Rir = s.Rir?.Value
                }).ToList()
            }).ToList()
        });

        return data.AsQueryable().Where(filter).ToList();
    }
}