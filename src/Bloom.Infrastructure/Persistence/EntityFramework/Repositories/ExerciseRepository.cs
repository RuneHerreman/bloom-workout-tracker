using Bloom.Domain.Exercises;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Repositories;

public sealed class ExerciseRepository(BloomDbContext context)
    : EfCoreGenericRepository<Exercise, ExerciseId>(context), IExerciseRepository
{
}