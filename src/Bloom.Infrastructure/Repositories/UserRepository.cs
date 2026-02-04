using Bloom.Domain.Entity;
using Bloom.Domain.Repositories;
using Bloom.Infrastructure.Persistence;

namespace Bloom.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly BloomDbContext _context;
    
    public UserRepository(BloomDbContext context) => _context = context;
    
    public Task<User> GetUserByEmail(string email, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task RegisterUser(User user, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task LoginUser(string email, string password)
    {
        throw new NotImplementedException();
    }
}