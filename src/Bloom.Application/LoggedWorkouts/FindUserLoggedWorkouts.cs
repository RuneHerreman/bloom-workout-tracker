using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Data.Filters;
using Bloom.Application.Contracts.Ports;

namespace Bloom.Application.LoggedWorkouts;

public sealed record FindUserLoggedWorkoutsInput;

public sealed record FindUserLoggedWorkoutsOutput(IReadOnlyList<LoggedWorkoutData> Logs);

public class FindUserLoggedWorkouts(
    ICurrentUser currentUser,
    IFindLoggedWorkoutsQuery query
) : IUseCase<FindUserLoggedWorkoutsInput, FindUserLoggedWorkoutsOutput>
{
    public async Task<FindUserLoggedWorkoutsOutput> Execute(FindUserLoggedWorkoutsInput input, CancellationToken ct = default)
    {
        var logs = await query.Fetch(
            LoggedWorkoutDataFilters.ByProperty(currentUser.UserId.Value), ct
        );

        return new FindUserLoggedWorkoutsOutput(logs);
    }
}
