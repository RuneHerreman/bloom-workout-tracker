using Bloom.Application.Common.Exceptions;
using Bloom.Application.Common.Security;
using Bloom.Domain.Entity;
using Bloom.Domain.Repositories;

namespace UnitTests.Mock;

public class MockUserRepository : IUserRepository
{
    public List<User> CreatedUsers { get; } = new();
    
    public async Task<User?> GetUserByEmail(string email, CancellationToken ct)
    {
        return await Task.FromResult(CreatedUsers.FirstOrDefault(u => u.Email == email));
    }

    public Task<User?> GetUserById(Guid userId, CancellationToken ct)
    {
        if (userId == Guid.Empty)
            return Task.FromResult<User?>(null);
        return Task.FromResult(CreatedUsers.FirstOrDefault(u => u.Id == userId));
    }

    public Task RegisterUser(User user, CancellationToken ct)
    {
        CreatedUsers.Add(user);
        return Task.CompletedTask;
    }
}
