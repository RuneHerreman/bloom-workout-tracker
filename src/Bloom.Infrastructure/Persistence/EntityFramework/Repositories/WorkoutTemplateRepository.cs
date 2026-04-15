using Bloom.Domain.Templates;
using Bloom.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Repositories;

public sealed class WorkoutTemplateRepository(BloomDbContext context)
    : EfCoreGenericRepository<WorkoutTemplate, WorkoutTemplateId>(context), IWorkoutTemplateRepository
{
    public Task AddWorkoutTemplate(WorkoutTemplate template) => Save(template);

    public Task DeleteWorkoutTemplate(WorkoutTemplate template) => Remove(template);

    public Task<WorkoutTemplate?> GetWorkoutTemplateById(WorkoutTemplateId id, UserId userId)
    {
        return _context.WorkoutTemplates
            .Include(t => t.Exercises)
            .ThenInclude(e => e.StrengthSets)
            .Include(t => t.Exercises)
            .ThenInclude(e => e.CardioSets)
            .FirstOrDefaultAsync(t => t.Id.Equals(id) && t.UserId.Equals(userId));
    }

    public async Task UpdateWorkoutTemplate(
        WorkoutTemplateId templateId,
        string name,
        List<WorkoutTemplateExercise> newExercises,
        UserId userId)
    {
        var template = await GetWorkoutTemplateById(templateId, userId);
        if (template is null)
            throw new InvalidOperationException("Workout template not found.");

        template.UpdateName(name);
        // Replace child collection to match requested exercise list.
        template.Exercises.Clear();
        template.Exercises.AddRange(newExercises);
    }
}