using Bloom.Domain.WorkoutTemplates;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Repositories;

public class WorkoutTemplateRepository(DomainDbContext context) : EfCoreGenericRepository<WorkoutTemplate, WorkoutTemplateId>(context), IWorkoutTemplateRepository
{
    
}