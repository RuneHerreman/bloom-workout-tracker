using Aornis;
using Bloom.Domain.Shared;
using Bloom.Domain.Users.ValueObjects;

namespace Bloom.Domain.Users;

public interface IUserRepository: IRepository<User, UserId>
{
    Task<bool> ExistsByEmail(Email email);
    Task<Optional<User>> ByEmail(Email email);
}
