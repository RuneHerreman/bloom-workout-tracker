using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Data.Filters;
using Bloom.Application.Contracts.Ports;

namespace Bloom.Application.LoggedWorkouts;

public sealed record FindUserLoggedWorkoutsInput(
    string? Name = null,
    DateTime? From = null,
    DateTime? To = null,
    string? Gear = null
);

public sealed record FindUserLoggedWorkoutsOutput(IReadOnlyList<LoggedWorkoutData> Logs);

public class FindUserLoggedWorkouts(
    ICurrentUser currentUser,
    IFindLoggedWorkoutsQuery query
) : IUseCase<FindUserLoggedWorkoutsInput, FindUserLoggedWorkoutsOutput>
{
    public async Task<FindUserLoggedWorkoutsOutput> Execute(FindUserLoggedWorkoutsInput input, CancellationToken ct = default)
    {
        var logs = await query.Fetch(
            LoggedWorkoutDataFilters.ByProperty(
                currentUser.UserId.Value,
                input.Name,
                input.From,
                input.To,
                input.Gear
            ), ct
        );

        return new FindUserLoggedWorkoutsOutput(logs);
    }
}
