using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Data.Filters;
using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Exercises.Enums;

namespace Bloom.Application.Exercises;

public sealed record SearchExerciseCatalogInput(
    string? Name,
    IReadOnlyList<string>? TargetMuscleGroups,
    IReadOnlyList<string>? ExerciseTypes
);

public sealed record SearchExerciseCatalogOutput(IReadOnlyList<ExerciseData> Exercises);

public class SearchExerciseCatalog(
    ISearchExerciseCatalogQuery searchExerciseCatalogQuery  
): IUseCase<SearchExerciseCatalogInput, SearchExerciseCatalogOutput>
{
    public async Task<SearchExerciseCatalogOutput> Execute(SearchExerciseCatalogInput input)
    {
        var exercises = await searchExerciseCatalogQuery.Fetch(
            ExerciseDataFilters.ByProperty(
                name: input.Name,
                muscleGroups: MapTargetMuscleGroups(input.TargetMuscleGroups),
                types: MapExerciseTypes(input.ExerciseTypes)
            )
        );
        
        return new SearchExerciseCatalogOutput(exercises);
    }

    private static IReadOnlyList<TargetMuscleData>? MapTargetMuscleGroups(IReadOnlyList<string>? muscleGroups)
    {
        return muscleGroups?
            .Select(mg => new TargetMuscleData(mg))
            .ToList();
    }

    private static IReadOnlyList<ExerciseType>? MapExerciseTypes(IReadOnlyList<string>? exerciseTypes)
    {
        if (exerciseTypes is null || exerciseTypes.Count == 0)
            return null;

        return exerciseTypes
            .Select(et => (parsed: Enum.TryParse<ExerciseType>(et, ignoreCase: true, out var result), type: result))
            .Where(x => x.parsed)
            .Select(x => x.type)
            .ToList();
    }
}