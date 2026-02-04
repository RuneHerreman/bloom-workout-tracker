using Bloom.Domain.Entity;
using Bloom.Domain.Repositories;
using Bloom.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly BloomDbContext _context;
    
    public UserRepository(BloomDbContext context) => _context = context;
    
    public async Task<User?> GetUserByEmail(string email, CancellationToken ct)
    {
        return await _context.Users.SingleOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task RegisterUser(User user, CancellationToken ct)
    {
        await _context.Users.AddAsync(user, ct);
        await _context.SaveChangesAsync(ct);
    }
}