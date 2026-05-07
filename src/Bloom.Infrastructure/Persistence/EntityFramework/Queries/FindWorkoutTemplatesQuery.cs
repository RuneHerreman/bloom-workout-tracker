using System.Linq.Expressions;
using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Ports;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Queries;

public class FindWorkoutTemplatesQuery(DomainDbContext context) : IFindWorkoutTemplatesQuery
{
    public async Task<IReadOnlyList<WorkoutTemplateData>> Fetch(
        Expression<Func<WorkoutTemplateData, bool>> filter
    )
    {
        var templates = await context.WorkoutTemplates
            .AsNoTracking()
            .ToListAsync();

        var data = templates.Select(t => new WorkoutTemplateData
        {
            Id = t.Id.Value,
            UserId = t.UserId.Value,
            Name = t.Name.Value,
            Exercises = t.TemplateExercises.Select(e => new TemplateExerciseData
            {
                ExerciseId = e.ExerciseId.Value,
                Order = e.Order,
                Sets = e.Sets.Select(s => new PlannedSetData
                {
                    Type = s.Type.ToString(),
                    Order = s.Order,
                    Reps = s.Reps?.Value,
                    Duration = s.Duration?.Value,
                    Distance = s.Distance?.Value,
                    DistanceUnit = s.Distance?.Unit.ToString()
                }).ToList()
            }).ToList()
        });

        return data.AsQueryable().Where(filter).ToList();
    }
}