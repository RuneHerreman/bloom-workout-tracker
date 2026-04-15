using Bloom.Application.Contracts.Data.Templates;
using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Queries;

public class GetAllUserTemplatesQuery(
    BloomDbContext context    
): IGetAllUserTemplatesQuery
{
    public async Task<List<WorkoutTemplateData>> Fetch(UserId userId)
    {
        var templates = await context.WorkoutTemplates
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .Select(t => new
            {
                TemplateId = t.Id.Value,
                t.Name,
                Exercises = t.Exercises
                    .OrderBy(e => e.Order)
                    .Select(e => new
                    {
                        ExerciseId = e.ExerciseId.Value,
                        ExerciseName = context.Exercises
                            .Where(ex => ex.Id == e.ExerciseId)
                            .Select(ex => ex.Name)
                            .FirstOrDefault() ?? string.Empty,
                        e.Order,
                        StrengthSets = e.StrengthSets
                            .OrderBy(s => s.SetOrder)
                            .Select(s => new
                            {
                                s.SetOrder,
                                s.Reps,
                                s.RIR
                            })
                            .ToList(),
                        CardioSets = e.CardioSets
                            .Select(s => new
                            {
                                s.Duration,
                                s.Distance
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .ToListAsync();

        return templates
            .Select(t => new WorkoutTemplateData(
                t.TemplateId,
                t.Name,
                t.Exercises.Select(e =>
                {
                    var sets = e.StrengthSets
                        .Select(s => new TemplateExerciseSetData(
                            s.SetOrder,
                            s.Reps,
                            s.RIR
                        ))
                        .Concat(e.CardioSets.Select((s, index) => new TemplateExerciseSetData(
                            SetOrder: index,
                            Duration: s.Duration,
                            Distance: s.Distance
                        )))
                        .ToList();

                    return new WorkoutTemplateExerciseData(
                        e.ExerciseId,
                        e.ExerciseName,
                        e.Order,
                        sets
                    );
                }).ToList()
            ))
            .ToList();
    }
}