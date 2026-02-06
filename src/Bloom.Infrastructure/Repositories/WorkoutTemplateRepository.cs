using Bloom.Domain.Entity;
using Bloom.Domain.Repositories;
using Bloom.Infrastructure.Persistence;

namespace Bloom.Infrastructure.Repositories;

public class WorkoutTemplateRepository : IWorkoutTemplateRepository
{
    private readonly BloomDbContext _context;
    
    public WorkoutTemplateRepository(BloomDbContext context) => _context = context;
    
    public async Task AddWorkoutTemplate(WorkoutTemplate template)
    {
        await _context.WorkoutTemplates.AddAsync(template);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteWorkoutTemplate(WorkoutTemplate template)
    {
        _context.WorkoutTemplates.Remove(template);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteWorkoutTemplateExercises(List<WorkoutTemplateExercise> exercises)
    {
        _context.WorkoutTemplateExercises.RemoveRange(exercises);
        await _context.SaveChangesAsync();
    }

    public async Task<WorkoutTemplate?> GetWorkoutTemplateById(Guid id)
    {
        if (id == Guid.Empty)
            return null;
        return await _context.WorkoutTemplates.FindAsync(id);
    }

    public async Task UpdateWorkoutTemplate(WorkoutTemplate template)
    {
        await _context.SaveChangesAsync();
    }
}