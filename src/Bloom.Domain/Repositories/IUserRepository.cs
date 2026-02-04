using Bloom.Domain.Entity;

namespace Bloom.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByEmail(string email, CancellationToken ct);
    Task RegisterUser(User user, CancellationToken ct);
    Task LoginUser(string email, string password);
}