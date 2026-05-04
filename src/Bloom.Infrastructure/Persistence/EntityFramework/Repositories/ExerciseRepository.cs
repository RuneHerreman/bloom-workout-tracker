using Bloom.Domain.Exercises;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Repositories;

public class ExerciseRepository(DomainDbContext context) : EfCoreGenericRepository<Exercise, ExerciseId>(context), IExerciseRepository
{
    
}