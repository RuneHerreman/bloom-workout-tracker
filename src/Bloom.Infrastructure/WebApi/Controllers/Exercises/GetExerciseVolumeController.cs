using Bloom.Application.Contracts.Ports;
using Bloom.Application.LoggedWorkouts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Exercises;

public sealed record GetExerciseVolumeRequest(
    [FromRoute] Guid ExerciseId,
    [FromServices] IUseCase<GetExerciseVolumeOverTimeInput, GetExerciseVolumeOverTimeOutput> UseCase
);

public sealed record MonthlyVolumeResponse(int Year, int Month, decimal MaxWeight, string WeightUnit);

public sealed record ExerciseVolumeResponse(Guid ExerciseId, IReadOnlyList<MonthlyVolumeResponse> MonthlyVolume);

public static class GetExerciseVolumeController
{
    public static async Task<Results<Ok<ExerciseVolumeResponse>, BadRequest>> Invoke(
        [AsParameters] GetExerciseVolumeRequest request
    )
    {
        var output = await request.UseCase.Execute(new GetExerciseVolumeOverTimeInput(request.ExerciseId));
        return TypedResults.Ok(new ExerciseVolumeResponse(
            output.ExerciseId,
            output.MonthlyVolume
                .Select(m => new MonthlyVolumeResponse(m.Year, m.Month, m.MaxWeight, m.WeightUnit))
                .ToList()
        ));
    }
}
