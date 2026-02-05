using Bloom.Domain.Entity;
using Bloom.Domain.Repositories;
using Bloom.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Repositories;

public class ExerciseRepository : IExerciseRepository
{
    private readonly BloomDbContext _context;
    
    public ExerciseRepository(BloomDbContext context) => _context = context;
    
    public async Task<List<Exercise>> GetByIdsAsync(List<Guid> exerciseIds, CancellationToken ct)
    {
        return await _context.Exercises.Where(e => exerciseIds.Contains(e.Id)).ToListAsync(ct);
    }
}