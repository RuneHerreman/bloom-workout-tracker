using System.ComponentModel.DataAnnotations;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.Exercises;
using Bloom.Infrastructure.WebApi.Controllers.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Exercises;

public sealed record SearchExerciseCatalogRequest(
    [FromQuery] string? Name,
    [FromQuery] string[]? TargetMuscleGroups,
    [FromQuery] string[]? ExerciseTypes,
    [FromServices] IUseCase<SearchExerciseCatalogInput, SearchExerciseCatalogOutput> UseCase
);

public static class SearchExerciseCatalogController
{
    public static async Task<Results<Ok<List<Exercise>>, BadRequest>> Invoke(
        [AsParameters] SearchExerciseCatalogRequest request,
        CancellationToken ct
    )
    {
        var output = await request.UseCase.Execute(new SearchExerciseCatalogInput(
            request.Name,
            request.TargetMuscleGroups,
            request.ExerciseTypes
        ), ct);
        
        return TypedResults.Ok(
            output.Exercises
                .Select(e => e.ToResponse())
                .ToList()
        );
    }
}
