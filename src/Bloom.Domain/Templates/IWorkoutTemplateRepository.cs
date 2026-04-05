using Bloom.Domain.Shared;
using Bloom.Domain.Users;

namespace Bloom.Domain.Templates;

public interface IWorkoutTemplateRepository: IRepository<WorkoutTemplate, WorkoutTemplateId>
{
    Task AddWorkoutTemplate(WorkoutTemplate template);
    Task DeleteWorkoutTemplate(WorkoutTemplate template);
    Task<WorkoutTemplate?> GetWorkoutTemplateById(WorkoutTemplateId id, UserId userId);
    Task UpdateWorkoutTemplate(WorkoutTemplateId templateId, string name, List<WorkoutTemplateExercise> newExercises, UserId userId);
}