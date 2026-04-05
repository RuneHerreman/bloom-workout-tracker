using Bloom.Domain.Users;
using Bloom.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    public Task<bool> Exists(UserId id)
    {
        throw new NotImplementedException();
    }

    public Task<User> ById(UserId id)
    {
        throw new NotImplementedException();
    }

    public Task Save(User aggregateRoot)
    {
        throw new NotImplementedException();
    }

    public Task Remove(User aggregateRoot)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetUserByEmail(string email)
    {
        throw new NotImplementedException();
    }
}