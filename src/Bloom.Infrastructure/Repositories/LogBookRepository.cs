using Bloom.Domain.Entity.Logs;
using Bloom.Domain.Repositories;
using Bloom.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Repositories;

public class LogBookRepository : ILogBookRepository
{
    private readonly BloomDbContext _context;
    public LogBookRepository(BloomDbContext context) => _context = context;
    
    public async Task<List<LoggedWorkout>> GetAllUserWorkoutsAsync(Guid userId, CancellationToken ct)
    {
        return await _context.LoggedWorkouts
            .Where(w => w.UserId == userId)
            .ToListAsync(ct);
    }
}