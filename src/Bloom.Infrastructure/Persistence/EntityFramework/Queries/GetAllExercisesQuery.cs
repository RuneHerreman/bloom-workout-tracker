using Bloom.Application.Contracts.Data;
using Bloom.Application.Contracts.Ports;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Queries;

public class GetAllExercisesQuery(
    BloomDbContext context
): IGetAllExercisesQuery
{
    public async Task<IEnumerable<ExerciseData>> Fetch()
    {
        IEnumerable<ExerciseData> exercises = await context.Exercises
            .Select(exercise => new ExerciseData(
                exercise.Id.Value,
                exercise.Name,
                exercise.Description,
                exercise.Type.ToString(),
                exercise.PrimaryMuscleGroup
            ))
            .ToListAsync();
        
        return await Task.FromResult(exercises);
    }
}