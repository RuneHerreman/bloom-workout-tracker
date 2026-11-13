using Bloom.Application.Contracts.Ports;
using Bloom.Application.Exercises;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Exercises;

public sealed record UpdateCustomExerciseRequest(
    [FromRoute] Guid ExerciseId,
    [FromBody] CustomExerciseBody Body,
    [FromServices] IUseCase<UpdateCustomExerciseInput, UpdateCustomExerciseOutput> UseCase
);

public sealed record UpdateCustomExerciseResponse(Guid ExerciseId);

public static class UpdateCustomExerciseController
{
    public static async Task<Results<Ok<UpdateCustomExerciseResponse>, BadRequest>> Invoke(
        [AsParameters] UpdateCustomExerciseRequest request,
        CancellationToken ct
    )
    {
        var output = await request.UseCase.Execute(new UpdateCustomExerciseInput(
            request.ExerciseId,
            request.Body.Name,
            request.Body.Description,
            request.Body.Type,
            request.Body.TargetMuscles
        ), ct);

        return TypedResults.Ok(new UpdateCustomExerciseResponse(output.ExerciseId));
    }
}
