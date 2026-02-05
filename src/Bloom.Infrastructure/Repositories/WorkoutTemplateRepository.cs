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
}