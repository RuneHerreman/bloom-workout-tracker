using Bloom.Application.Contracts.Data.Filters;
using Bloom.Application.Contracts.Ports;

namespace Bloom.Application.LoggedWorkouts;

public sealed record GetExerciseVolumeOverTimeInput(Guid ExerciseId);

public sealed record MonthlyMaxWeightData(int Year, int Month, decimal MaxWeight, string WeightUnit);

public sealed record GetExerciseVolumeOverTimeOutput(
    Guid ExerciseId,
    IReadOnlyList<MonthlyMaxWeightData> MonthlyVolume
);

public class GetExerciseVolumeOverTime(
    ICurrentUser currentUser,
    IFindLoggedWorkoutsQuery query
) : IUseCase<GetExerciseVolumeOverTimeInput, GetExerciseVolumeOverTimeOutput>
{
    public async Task<GetExerciseVolumeOverTimeOutput> Execute(GetExerciseVolumeOverTimeInput input)
    {
        var logs = await query.Fetch(LoggedWorkoutDataFilters.ByProperty(currentUser.UserId.Value));

        var monthlyVolume = logs
            .SelectMany(l => l.LoggedExercises
                .Where(e => e.ExerciseId == input.ExerciseId)
                .SelectMany(e => e.Sets)
                .Where(s => s.Weight != null)
                .Select(s => new { l.LoggedAt, s.Weight }))
            .GroupBy(x => new { x.LoggedAt.Year, x.LoggedAt.Month })
            .Select(g =>
            {
                var best = g.MaxBy(x => x.Weight!.Value)!;
                return new MonthlyMaxWeightData(
                    g.Key.Year,
                    g.Key.Month,
                    best.Weight!.Value,
                    best.Weight.Unit.ToString()
                );
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToList();

        return new GetExerciseVolumeOverTimeOutput(input.ExerciseId, monthlyVolume);
    }
}
