using System.Linq.Expressions;
using Bloom.Domain.Exercises;
using Bloom.Domain.Exercises.Enums;

namespace Bloom.Application.Contracts.Filters;

public static class ExerciseDataFilters
{
    public static Expression<Func<ExerciseData, bool>> ByProperty(
        string? name,
        IReadOnlyList<TargetMuscleData>? muscleGroups,
        IReadOnlyList<ExerciseType>? types)
    {
        var cleanName = string.IsNullOrWhiteSpace(name) ? null : name.ToLower();
        var cleanMuscleGroups = muscleGroups is { Count: > 0 }
            ? muscleGroups.Select(mg => mg.Value.ToLower()).ToList()
            : null;
        var cleanTypeStrings = types is { Count: > 0 }
            ? types.Select(t => t.ToString()).ToList()
            : null;

        return exercise =>
            (cleanName == null || exercise.Name.ToLower().Contains(cleanName)) &&
            (cleanMuscleGroups == null || exercise.TargetMuscles.Any(mg => cleanMuscleGroups.Contains(mg.Value.ToLower()))) &&
            (cleanTypeStrings == null || cleanTypeStrings.Contains(exercise.Type));
    }

    public static Expression<Func<ExerciseData, bool>> ById(ExerciseId inputId)
    {
        if (inputId.Value == Guid.Empty)
            return exercise => false;

        return exercise => exercise.Id == inputId.Value;
    }
}