
using System.Linq.Expressions;
using Bloom.Domain.Users;

namespace Bloom.Application.Contracts.Data.Filters;

public static class UserDataFilters
{
    public static Expression<Func<UserData, bool>> ById(UserId inputId)
    {
        if (inputId.Value == Guid.Empty)
            return user => false;

        return user => user.Id == inputId.Value;
    }
}
