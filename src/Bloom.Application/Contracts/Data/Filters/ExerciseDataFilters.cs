using System.Linq.Expressions;
using Bloom.Domain.Exercises;
using Bloom.Domain.Exercises.Enums;

namespace Bloom.Application.Contracts.Data.Filters;

public static class ExerciseDataFilters
{
    public static Expression<Func<ExerciseData, bool>> ByProperty(
        string? name,
        IReadOnlyList<TargetMuscleData>? muscleGroups,
        IReadOnlyList<ExerciseType>? types,
        Guid userId)
    {
        var cleanName = string.IsNullOrWhiteSpace(name) ? null : name.ToLower();
        var cleanMuscleGroups = muscleGroups is { Count: > 0 }
            // An area matches exercises tagged with it or with any muscle in it.
            ? muscleGroups
                .SelectMany(mg => MuscleTree.Areas
                    .Where(a => a.Area.Equals(mg.Value.Trim(), StringComparison.OrdinalIgnoreCase))
                    .SelectMany(a => a.Muscles)
                    .Append(MuscleTree.Canonical(mg.Value) ?? mg.Value)
                    .Append(mg.Value.Trim()))
                .Select(m => m.ToLower())
                .Distinct()
                .ToList()
            : null;
        var cleanTypeStrings = types is { Count: > 0 }
            ? types.Select(t => t.ToString()).ToList()
            : null;

        return exercise =>
            (exercise.OwnerUserId == null || exercise.OwnerUserId == userId) &&
            (cleanName == null || exercise.Name.ToLower().Contains(cleanName)) &&
            (cleanMuscleGroups == null || exercise.TargetMuscles.Any(mg => cleanMuscleGroups.Contains(mg.Value.ToLower()))) &&
            (cleanTypeStrings == null || cleanTypeStrings.Contains(exercise.Type));
    }

    public static Expression<Func<ExerciseData, bool>> ById(ExerciseId inputId, Guid userId)
    {
        if (inputId.Value == Guid.Empty)
            return exercise => false;

        return exercise =>
            exercise.Id == inputId.Value &&
            (exercise.OwnerUserId == null || exercise.OwnerUserId == userId);
    }
}