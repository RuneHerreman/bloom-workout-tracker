
using System.Linq.Expressions;
using Bloom.Domain.Users;

namespace Bloom.Application.Contracts.Data.Filters;

public static class UserDataFilters
{
    /// <summary>Users who opted in to the leaderboard, plus <paramref name="currentUserId"/> so they always see their own standing.</summary>
    public static Expression<Func<UserData, bool>> OnLeaderboardFor(UserId currentUserId)
    {
        var id = currentUserId.Value;
        return user => user.ShowOnLeaderboard || user.Id == id;
    }

    public static Expression<Func<UserData, bool>> ById(UserId inputId)
    {
        if (inputId.Value == Guid.Empty)
            return user => false;

        return user => user.Id == inputId.Value;
    }
}
