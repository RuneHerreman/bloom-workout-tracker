using Bloom.Domain.Users;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Repositories;

public class UserRepository(DomainDbContext context) : EfCoreGenericRepository<User, UserId>(context), IUserRepository
{
    
}