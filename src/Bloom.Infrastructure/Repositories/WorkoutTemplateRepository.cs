using Bloom.Domain.Templates;
using Bloom.Domain.Users;
using Bloom.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Repositories;

public class WorkoutTemplateRepository : IWorkoutTemplateRepository
{
    public Task<bool> Exists(WorkoutTemplateId id)
    {
        throw new NotImplementedException();
    }

    public Task<WorkoutTemplate> ById(WorkoutTemplateId id)
    {
        throw new NotImplementedException();
    }

    public Task Save(WorkoutTemplate aggregateRoot)
    {
        throw new NotImplementedException();
    }

    public Task Remove(WorkoutTemplate aggregateRoot)
    {
        throw new NotImplementedException();
    }

    public Task AddWorkoutTemplate(WorkoutTemplate template)
    {
        throw new NotImplementedException();
    }

    public Task DeleteWorkoutTemplate(WorkoutTemplate template)
    {
        throw new NotImplementedException();
    }

    public Task<WorkoutTemplate?> GetWorkoutTemplateById(WorkoutTemplateId id, UserId userId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateWorkoutTemplate(WorkoutTemplateId templateId, string name, List<WorkoutTemplateExercise> newExercises, UserId userId)
    {
        throw new NotImplementedException();
    }
}