using Bloom.Domain.Shared;

namespace Bloom.Domain.Users;

public interface IUserRepository: IRepository<User, UserId>
{
    Task<User?> GetUserByEmail(string email);
}