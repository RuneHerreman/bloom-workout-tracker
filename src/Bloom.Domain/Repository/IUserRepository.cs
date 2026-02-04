using Bloom.Domain.Entity;

namespace Bloom.Domain.Repository;

public interface IUserRepository
{
    Task<User> GetUser(Guid id);
    Task RegisterUser(User user);
    Task LoginUser(string email, string password);
}