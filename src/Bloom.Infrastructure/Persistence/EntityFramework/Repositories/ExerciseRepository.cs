using Bloom.Domain.Exercises;
using Bloom.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Repositories;

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