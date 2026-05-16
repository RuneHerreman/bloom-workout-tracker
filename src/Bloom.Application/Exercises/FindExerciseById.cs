using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Data.Filters;
using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Exercises;
using Bloom.Domain.Shared;
using Bloom.Shared.Exceptions;

namespace Bloom.Application.Exercises;

public sealed record FindExerciseByIdInput(Guid ExerciseId);

public sealed record FindExerciseByIdOutput(ExerciseData Exercise);

public class FindExerciseById(
    ISearchExerciseCatalogQuery searchExerciseCatalogQuery
): IUseCase<FindExerciseByIdInput, FindExerciseByIdOutput>
{
    public async Task<FindExerciseByIdOutput> Execute(FindExerciseByIdInput input, CancellationToken ct = default)
    {
        var exercises = await searchExerciseCatalogQuery.Fetch(
            ExerciseDataFilters.ById(EntityId.New<ExerciseId>(input.ExerciseId)), ct
        );
        
        var result = exercises.FirstOrDefault() 
            ?? throw new ExerciseNotFoundException(input.ExerciseId);

        return new FindExerciseByIdOutput(result);
    }
}