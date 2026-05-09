using Bloom.Application.Contracts.Ports;
using Bloom.Application.LoggedWorkouts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Exercises;

public sealed record GetExercisePrRequest(
    [FromRoute] Guid ExerciseId,
    [FromServices] IUseCase<GetExercisePrInput, GetExercisePrOutput> UseCase
);

public sealed record ExercisePrResponse(Guid ExerciseId, decimal? Weight, string? WeightUnit);

public static class GetExercisePrController
{
    public static async Task<Results<Ok<ExercisePrResponse>, BadRequest>> Invoke(
        [AsParameters] GetExercisePrRequest request
    )
    {
        var output = await request.UseCase.Execute(new GetExercisePrInput(request.ExerciseId));
        return TypedResults.Ok(new ExercisePrResponse(output.ExerciseId, output.Weight, output.WeightUnit));
    }
}
