using Bloom.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Repositories;

public sealed class UserRepository(BloomDbContext context)
    :  EfCoreGenericRepository<User, UserId>(context), IUserRepository
{
    public Task<User?> GetUserByEmail(string email)
    {
        return _context.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
    }
}