using Bloom.Application.Contracts.Ports;
using Bloom.Application.Exercises;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Exercises;

public sealed record CreateCustomExerciseRequest(
    [FromBody] CustomExerciseBody Body,
    [FromServices] IUseCase<CreateCustomExerciseInput, CreateCustomExerciseOutput> UseCase
);

public sealed record CreateCustomExerciseResponse(Guid ExerciseId);

public static class CreateCustomExerciseController
{
    public static async Task<Results<Ok<CreateCustomExerciseResponse>, BadRequest>> Invoke(
        [AsParameters] CreateCustomExerciseRequest request,
        CancellationToken ct
    )
    {
        var output = await request.UseCase.Execute(new CreateCustomExerciseInput(
            request.Body.Name,
            request.Body.Description,
            request.Body.Type,
            request.Body.TargetMuscles
        ), ct);

        return TypedResults.Ok(new CreateCustomExerciseResponse(output.ExerciseId));
    }
}
