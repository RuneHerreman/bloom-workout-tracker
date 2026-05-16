using Aornis;
using Bloom.Domain.Users;
using Bloom.Domain.Users.ValueObjects;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Repositories;

public class UserRepository(DomainDbContext context) : EfCoreGenericRepository<User, UserId>(context), IUserRepository
{
    public Task<bool> ExistsByEmail(Email email)
    {
        return _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<Optional<User>> ByEmail(Email email)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
        return Optional.Of(user);
    }
}
