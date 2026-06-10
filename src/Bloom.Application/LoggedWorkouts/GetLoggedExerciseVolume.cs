using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Data.Filters;
using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Exercises.Enums;

namespace Bloom.Application.LoggedWorkouts;

public sealed record GetLoggedExerciseVolumeInput(
    string? Name,
    IReadOnlyList<string>? TargetMuscleGroups,
    IReadOnlyList<string>? ExerciseTypes,
    int? FromYear,
    int? FromMonth,
    int? ToYear,
    int? ToMonth
);

public sealed record MonthlyMaxWeightData(int Year, int Month, decimal MaxWeight, string WeightUnit);

public sealed record ExerciseVolumeData(
    Guid ExerciseId,
    string ExerciseName,
    string ExerciseType,
    IReadOnlyList<string> TargetMuscles,
    IReadOnlyList<MonthlyMaxWeightData> MonthlyVolume
);

public sealed record GetLoggedExerciseVolumeOutput(IReadOnlyList<ExerciseVolumeData> Exercises);

public class GetLoggedExerciseVolume(
    ICurrentUser currentUser,
    IFindLoggedWorkoutsQuery logsQuery,
    ISearchExerciseCatalogQuery exercisesQuery
) : IUseCase<GetLoggedExerciseVolumeInput, GetLoggedExerciseVolumeOutput>
{
    public async Task<GetLoggedExerciseVolumeOutput> Execute(GetLoggedExerciseVolumeInput input, CancellationToken ct = default)
    {
        var logs = await logsQuery.Fetch(LoggedWorkoutDataFilters.ByProperty(currentUser.UserId.Value), ct);

        int fromOrdinal = input is { FromYear: not null, FromMonth: not null }
            ? input.FromYear.Value * 12 + input.FromMonth.Value
            : 0;
        int toOrdinal = input is { ToYear: not null, ToMonth: not null }
            ? input.ToYear.Value * 12 + input.ToMonth.Value
            : int.MaxValue;

        var volumeByExercise = logs
            .SelectMany(l => l.LoggedExercises
                .SelectMany(e => e.Sets
                    .Where(s => s.Weight != null)
                    .Select(s => new { l.LoggedAt, e.ExerciseId, s.Weight })))
            .Where(x => x.LoggedAt.Year * 12 + x.LoggedAt.Month >= fromOrdinal &&
                        x.LoggedAt.Year * 12 + x.LoggedAt.Month <= toOrdinal)
            .GroupBy(x => x.ExerciseId)
            .ToDictionary(
                g => g.Key,
                g => g
                    .GroupBy(x => new { x.LoggedAt.Year, x.LoggedAt.Month })
                    .Select(mg =>
                    {
                        var best = mg.MaxBy(x => x.Weight!.Value)!;
                        return new MonthlyMaxWeightData(mg.Key.Year, mg.Key.Month, best.Weight!.Value, best.Weight.Unit.ToString());
                    })
                    .OrderBy(m => m.Year).ThenBy(m => m.Month)
                    .ToList() as IReadOnlyList<MonthlyMaxWeightData>
            );

        if (volumeByExercise.Count == 0)
            return new GetLoggedExerciseVolumeOutput([]);

        var exercises = await exercisesQuery.Fetch(
            ExerciseDataFilters.ByProperty(
                name: input.Name,
                muscleGroups: MapMuscleGroups(input.TargetMuscleGroups),
                types: MapExerciseTypes(input.ExerciseTypes),
                userId: currentUser.UserId.Value
            ), ct
        );

        var result = exercises
            .Where(e => volumeByExercise.ContainsKey(e.Id))
            .Select(e => new ExerciseVolumeData(
                e.Id,
                e.Name,
                e.Type,
                e.TargetMuscles.Select(m => m.Value).ToList(),
                volumeByExercise[e.Id]
            ))
            .ToList();

        return new GetLoggedExerciseVolumeOutput(result);
    }

    private static IReadOnlyList<TargetMuscleData>? MapMuscleGroups(IReadOnlyList<string>? muscleGroups) =>
        muscleGroups?.Select(mg => new TargetMuscleData(mg)).ToList();

    private static IReadOnlyList<ExerciseType>? MapExerciseTypes(IReadOnlyList<string>? types)
    {
        if (types is null || types.Count == 0) return null;
        return types
            .Select(t => (parsed: Enum.TryParse<ExerciseType>(t, ignoreCase: true, out var result), type: result))
            .Where(x => x.parsed)
            .Select(x => x.type)
            .ToList();
    }
}
