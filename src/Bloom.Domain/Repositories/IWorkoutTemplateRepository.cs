using Bloom.Domain.Entity;

namespace Bloom.Domain.Repositories;

public interface IWorkoutTemplateRepository
{
    Task AddWorkoutTemplate(WorkoutTemplate template);
    Task DeleteWorkoutTemplate(WorkoutTemplate template);
    Task<WorkoutTemplate?> GetWorkoutTemplateById(Guid id, Guid userId);
    Task UpdateWorkoutTemplate(Guid templateId, string name, List<WorkoutTemplateExercise> newExercises, Guid userId);

}