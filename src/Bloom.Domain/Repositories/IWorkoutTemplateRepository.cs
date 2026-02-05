using Bloom.Domain.Entity;

namespace Bloom.Domain.Repositories;

public interface IWorkoutTemplateRepository
{
    Task AddWorkoutTemplate(WorkoutTemplate template);
}