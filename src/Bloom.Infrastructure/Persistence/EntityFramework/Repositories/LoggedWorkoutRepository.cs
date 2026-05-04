using Bloom.Domain.LoggedWorkouts;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Repositories;

public class LoggedWorkoutRepository(DomainDbContext context) : EfCoreGenericRepository<LoggedWorkout, LoggedWorkoutId>(context), ILoggedWorkoutRepository
{
    
}