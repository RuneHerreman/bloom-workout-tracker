using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Data.Filters;
using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Exercises.Enums;

namespace Bloom.Application.LoggedWorkouts;

public sealed record GetLoggedExercisePrsInput(
    string? Name,
    IReadOnlyList<string>? TargetMuscleGroups,
    IReadOnlyList<string>? ExerciseTypes
);

public sealed record ExercisePrData(
    Guid ExerciseId,
    string ExerciseName,
    string ExerciseType,
    IReadOnlyList<string> TargetMuscles,
    decimal Weight,
    string WeightUnit
);

public sealed record GetLoggedExercisePrsOutput(IReadOnlyList<ExercisePrData> Prs);

public class GetLoggedExercisePrs(
    ICurrentUser currentUser,
    IFindLoggedWorkoutsQuery logsQuery,
    ISearchExerciseCatalogQuery exercisesQuery
) : IUseCase<GetLoggedExercisePrsInput, GetLoggedExercisePrsOutput>
{
    public async Task<GetLoggedExercisePrsOutput> Execute(GetLoggedExercisePrsInput input, CancellationToken ct = default)
    {
        var logs = await logsQuery.Fetch(LoggedWorkoutDataFilters.ByProperty(currentUser.UserId.Value), ct);

        var prByExercise = logs
            .SelectMany(l => l.LoggedExercises)
            .SelectMany(e => e.Sets
                .Where(s => s.Weight != null)
                .Select(s => new { e.ExerciseId, s.Weight }))
            .GroupBy(x => x.ExerciseId)
            .ToDictionary(g => g.Key, g => g.MaxBy(x => x.Weight!.Value)!.Weight!);

        if (prByExercise.Count == 0)
            return new GetLoggedExercisePrsOutput([]);

        var exercises = await exercisesQuery.Fetch(
            ExerciseDataFilters.ByProperty(
                name: input.Name,
                muscleGroups: MapMuscleGroups(input.TargetMuscleGroups),
                types: MapExerciseTypes(input.ExerciseTypes)
            ), ct
        );

        var prs = exercises
            .Where(e => prByExercise.ContainsKey(e.Id))
            .Select(e => new ExercisePrData(
                e.Id,
                e.Name,
                e.Type,
                e.TargetMuscles.Select(m => m.Value).ToList(),
                prByExercise[e.Id].Value,
                prByExercise[e.Id].Unit.ToString()
            ))
            .ToList();

        return new GetLoggedExercisePrsOutput(prs);
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
