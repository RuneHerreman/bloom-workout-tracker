using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Data.Filters;
using Bloom.Application.Contracts.Ports;

namespace Bloom.Application.LoggedWorkouts;

public sealed record FindUserLoggedWorkoutsInput(Guid UserId);

public sealed record FindUserLoggedWorkoutsOutput(IReadOnlyList<LoggedWorkoutData> Logs);

public class FindUserLoggedWorkouts(
    IFindLoggedWorkoutsQuery query
) : IUseCase<FindUserLoggedWorkoutsInput, FindUserLoggedWorkoutsOutput>
{
    public async Task<FindUserLoggedWorkoutsOutput> Execute(FindUserLoggedWorkoutsInput input)
    {
        var logs = await query.Fetch(
            LoggedWorkoutDataFilters.ByProperty(input.UserId)
        );

        return new FindUserLoggedWorkoutsOutput(logs);
    }
}