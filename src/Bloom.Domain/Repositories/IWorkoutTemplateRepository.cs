using Bloom.Domain.Entity;

namespace Bloom.Domain.Repositories;

public interface IWorkoutTemplateRepository
{
    Task AddWorkoutTemplate(WorkoutTemplate template);
    Task DeleteWorkoutTemplate(WorkoutTemplate template);
    Task DeleteWorkoutTemplateExercises(List<WorkoutTemplateExercise> exercises);
    Task<WorkoutTemplate?> GetWorkoutTemplateById(Guid id);
    
    Task UpdateWorkoutTemplate(WorkoutTemplate template);
    
}