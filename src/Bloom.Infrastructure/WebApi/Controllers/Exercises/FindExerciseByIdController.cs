using Bloom.Application.Contracts.Ports;
using Bloom.Application.Exercises;
using Bloom.Infrastructure.WebApi.Controllers.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Exercises;

public sealed record FindExerciseByIdRequest(
    [FromRoute] Guid ExerciseId,
    [FromServices] IUseCase<FindExerciseByIdInput, FindExerciseByIdOutput> UseCase
);

public static class FindExerciseByIdController
{
    public static async Task<Results<Ok<Exercise>, BadRequest>> Invoke(
        [AsParameters] FindExerciseByIdRequest request,
        CancellationToken ct
    )
    {
        var exercise = await request.UseCase.Execute(new FindExerciseByIdInput(request.ExerciseId), ct);

        return TypedResults.Ok(exercise.Exercise.ToResponse());
    }
}