using Bloom.Application.Contracts.Data.Filters;
using Bloom.Application.Contracts.Ports;

namespace Bloom.Application.LoggedWorkouts;

public sealed record GetExercisePrInput(Guid ExerciseId);

public sealed record GetExercisePrOutput(Guid ExerciseId, decimal? Weight, string? WeightUnit);

public class GetExercisePr(
    ICurrentUser currentUser,
    IFindLoggedWorkoutsQuery query
) : IUseCase<GetExercisePrInput, GetExercisePrOutput>
{
    public async Task<GetExercisePrOutput> Execute(GetExercisePrInput input)
    {
        var logs = await query.Fetch(LoggedWorkoutDataFilters.ByProperty(currentUser.UserId.Value));

        var bestSet = logs
            .SelectMany(l => l.LoggedExercises)
            .Where(e => e.ExerciseId == input.ExerciseId)
            .SelectMany(e => e.Sets)
            .Where(s => s.Weight != null)
            .MaxBy(s => s.Weight!.Value);

        return new GetExercisePrOutput(
            input.ExerciseId,
            bestSet?.Weight?.Value,
            bestSet?.Weight?.Unit.ToString()
        );
    }
}
