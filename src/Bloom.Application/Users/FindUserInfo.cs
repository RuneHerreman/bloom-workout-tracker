using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Data.Filters;
using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;

namespace Bloom.Application.Users;

public sealed record FindUserInfoInput(Guid UserId);

public sealed record FindUserInfoOutput(UserData User);

public class FindUserInfo(
    ICurrentUser currentUser,
    IFindUsersQuery query
) : IUseCase<FindUserInfoInput, FindUserInfoOutput>
{
    public async Task<FindUserInfoOutput> Execute(FindUserInfoInput input)
    {
        var requestedUserId = EntityId.New<UserId>(input.UserId);

        if (currentUser.UserId != requestedUserId)
            throw new UserAccessDeniedException(
                $"User {currentUser.UserId.Value} cannot access user {input.UserId}");

        var users = await query.Fetch(UserDataFilters.ById(requestedUserId));

        var result = users.FirstOrDefault()
            ?? throw new UserNotFoundException($"User not found | Id: {input.UserId}");

        return new FindUserInfoOutput(result);
    }
}
