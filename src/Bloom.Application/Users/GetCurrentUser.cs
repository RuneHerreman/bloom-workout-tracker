using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Data.Filters;
using Bloom.Application.Contracts.Ports;
using Bloom.Shared.Exceptions;

namespace Bloom.Application.Users;

public sealed record GetCurrentUserInput;

public sealed record GetCurrentUserOutput(UserData User);

public class GetCurrentUser(
    ICurrentUser currentUser,
    IFindUsersQuery query
) : IUseCase<GetCurrentUserInput, GetCurrentUserOutput>
{
    public async Task<GetCurrentUserOutput> Execute(GetCurrentUserInput input)
    {
        var users = await query.Fetch(UserDataFilters.ById(currentUser.UserId));

        var user = users.FirstOrDefault()
            ?? throw new UserNotFoundException($"User not found | Id: {currentUser.UserId.Value}");

        return new GetCurrentUserOutput(user);
    }
}
