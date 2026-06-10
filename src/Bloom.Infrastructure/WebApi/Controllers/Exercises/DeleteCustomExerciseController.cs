using Bloom.Application.Contracts.Ports;
using Bloom.Application.Exercises;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Exercises;

public sealed record DeleteCustomExerciseRequest(
    [FromRoute] Guid ExerciseId,
    [FromServices] IUseCase<DeleteCustomExerciseInput> UseCase
);

public static class DeleteCustomExerciseController
{
    public static async Task<Results<NoContent, BadRequest>> Invoke(
        [AsParameters] DeleteCustomExerciseRequest request,
        CancellationToken ct
    )
    {
        await request.UseCase.Execute(new DeleteCustomExerciseInput(
            request.ExerciseId
        ), ct);

        return TypedResults.NoContent();
    }
}
