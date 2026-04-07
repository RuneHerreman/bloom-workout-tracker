using Bloom.Domain.Exercises;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Repositories;

public class ExerciseRepository : IExerciseRepository
{
    public Task<bool> Exists(ExerciseId id)
    {
        throw new NotImplementedException();
    }

    public Task<Exercise> ById(ExerciseId id)
    {
        throw new NotImplementedException();
    }

    public Task Save(Exercise aggregateRoot)
    {
        throw new NotImplementedException();
    }

    public Task Remove(Exercise aggregateRoot)
    {
        throw new NotImplementedException();
    }
}