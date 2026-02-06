using Bloom.Domain.Entity;

namespace Bloom.Domain.Repositories;

public interface IWorkoutTemplateRepository
{
    Task AddWorkoutTemplate(WorkoutTemplate template);
    Task DeleteWorkoutTemplate(WorkoutTemplate template);
    Task<WorkoutTemplate?> GetWorkoutTemplateById(Guid id);
    Task UpdateWorkoutTemplate(Guid templateId, string name, List<WorkoutTemplateExercise> newExercises);

}