using Bloom.Domain.Entity;
using Bloom.Domain.Repositories;
using Bloom.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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

    public async Task<WorkoutTemplate?> GetWorkoutTemplateById(Guid id, Guid userId)
    {
        if (id == Guid.Empty)
            return null;
        
        return await _context.WorkoutTemplates
            .Where(t => t.Id == id && t.UserId == userId)
            .SingleOrDefaultAsync();
    }

    public async Task UpdateWorkoutTemplate(Guid templateId, string name,
        List<WorkoutTemplateExercise> newExercises, Guid userId)
    {
        var template = await GetWorkoutTemplateById(templateId, userId);
        if (template == null)
            throw new InvalidOperationException("Template not found or does not belong to you");

        var existingExercises = await _context.WorkoutTemplateExercises
            .Where(e => e.WorkoutTemplateId == templateId)
            .Include(e => e.Sets)
            .ToListAsync();
        _context.RemoveRange(existingExercises);

        template.Name = name;

        await _context.WorkoutTemplateExercises.AddRangeAsync(newExercises);
        await _context.SaveChangesAsync();
    }
}