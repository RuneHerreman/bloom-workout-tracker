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

    public Task RegisterUser(User user, CancellationToken ct)
    {
        CreatedUsers.Add(user);
        return Task.CompletedTask;
    }

    public Task LoginUser(string email, string password)
    {
        throw new NotImplementedException();
    }
}
