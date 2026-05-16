using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Data.Filters;
using Bloom.Application.Contracts.Ports;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.Shared;
using Bloom.Shared.Exceptions;

namespace Bloom.Application.LoggedWorkouts;

public sealed record FindLoggedWorkoutByIdInput(Guid LoggedWorkoutId);

public sealed record FindLoggedWorkoutByIdOutput(LoggedWorkoutData Log);

public class FindLoggedWorkoutById(
    ICurrentUser currentUser,
    IFindLoggedWorkoutsQuery query
) : IUseCase<FindLoggedWorkoutByIdInput, FindLoggedWorkoutByIdOutput>
{
    public async Task<FindLoggedWorkoutByIdOutput> Execute(FindLoggedWorkoutByIdInput input, CancellationToken ct = default)
    {
        var logs = await query.Fetch(
            LoggedWorkoutDataFilters.ById(EntityId.New<LoggedWorkoutId>(input.LoggedWorkoutId)), ct
        );

        var result = logs.FirstOrDefault()
            ?? throw new LoggedWorkoutNotFoundException($"LoggedWorkout not found | Id: {input.LoggedWorkoutId}");

        if (result.UserId != currentUser.UserId.Value)
            throw new LoggedWorkoutAccessDeniedException($"User {currentUser.UserId.Value} does not own log {input.LoggedWorkoutId}");

        return new FindLoggedWorkoutByIdOutput(result);
    }
}