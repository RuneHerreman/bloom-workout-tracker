using Bloom.Application.Contracts.Ports;
using Bloom.Application.LoggedWorkouts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.LoggedWorkouts;

public sealed record GetLoggedExercisePrsRequest(
    [FromQuery] string? Name,
    [FromQuery] string[]? TargetMuscleGroups,
    [FromQuery] string[]? ExerciseTypes,
    [FromServices] IUseCase<GetLoggedExercisePrsInput, GetLoggedExercisePrsOutput> UseCase
);

public sealed record ExercisePrResponse(
    Guid ExerciseId,
    string ExerciseName,
    string ExerciseType,
    IReadOnlyList<string> TargetMuscles,
    decimal Weight,
    string WeightUnit
);

public static class GetLoggedExercisePrsController
{
    public static async Task<Results<Ok<List<ExercisePrResponse>>, BadRequest>> Invoke(
        [AsParameters] GetLoggedExercisePrsRequest request,
        CancellationToken ct
    )
    {
        var output = await request.UseCase.Execute(new GetLoggedExercisePrsInput(
            request.Name,
            request.TargetMuscleGroups,
            request.ExerciseTypes
        ), ct);

        return TypedResults.Ok(
            output.Prs
                .Select(pr => new ExercisePrResponse(
                    pr.ExerciseId,
                    pr.ExerciseName,
                    pr.ExerciseType,
                    pr.TargetMuscles,
                    pr.Weight,
                    pr.WeightUnit
                ))
                .ToList()
        );
    }
}
