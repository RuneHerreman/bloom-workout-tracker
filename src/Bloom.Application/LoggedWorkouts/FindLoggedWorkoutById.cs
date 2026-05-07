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
    IFindLoggedWorkoutsQuery query
) : IUseCase<FindLoggedWorkoutByIdInput, FindLoggedWorkoutByIdOutput>
{
    public async Task<FindLoggedWorkoutByIdOutput> Execute(FindLoggedWorkoutByIdInput input)
    {
        var logs = await query.Fetch(
            LoggedWorkoutDataFilters.ById(EntityId.New<LoggedWorkoutId>(input.LoggedWorkoutId))
        );

        var result = logs.FirstOrDefault()
            ?? throw new LoggedWorkoutNotFoundException($"LoggedWorkout not found | Id: {input.LoggedWorkoutId}");

        return new FindLoggedWorkoutByIdOutput(result);
    }
}