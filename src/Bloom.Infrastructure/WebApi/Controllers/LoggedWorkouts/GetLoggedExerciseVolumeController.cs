using System.ComponentModel.DataAnnotations;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.LoggedWorkouts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.LoggedWorkouts;

public sealed record GetLoggedExerciseVolumeRequest(
    [FromQuery, MaxLength(200)] string? Name,
    [FromQuery] string[]? TargetMuscleGroups,
    [FromQuery] string[]? ExerciseTypes,
    [FromQuery, Range(1900, 2200)] int? FromYear,
    [FromQuery, Range(1, 12)] int? FromMonth,
    [FromQuery, Range(1900, 2200)] int? ToYear,
    [FromQuery, Range(1, 12)] int? ToMonth,
    [FromServices] IUseCase<GetLoggedExerciseVolumeInput, GetLoggedExerciseVolumeOutput> UseCase
);

public sealed record MonthlyVolumeResponse(int Year, int Month, decimal MaxWeight, string WeightUnit);

public sealed record ExerciseVolumeResponse(
    Guid ExerciseId,
    string ExerciseName,
    string ExerciseType,
    IReadOnlyList<string> TargetMuscles,
    IReadOnlyList<MonthlyVolumeResponse> MonthlyVolume
);

public static class GetLoggedExerciseVolumeController
{
    public static async Task<Results<Ok<List<ExerciseVolumeResponse>>, BadRequest>> Invoke(
        [AsParameters] GetLoggedExerciseVolumeRequest request,
        CancellationToken ct
    )
    {
        var output = await request.UseCase.Execute(new GetLoggedExerciseVolumeInput(
            request.Name,
            request.TargetMuscleGroups,
            request.ExerciseTypes,
            request.FromYear,
            request.FromMonth,
            request.ToYear,
            request.ToMonth
        ), ct);

        return TypedResults.Ok(
            output.Exercises
                .Select(e => new ExerciseVolumeResponse(
                    e.ExerciseId,
                    e.ExerciseName,
                    e.ExerciseType,
                    e.TargetMuscles,
                    e.MonthlyVolume
                        .Select(m => new MonthlyVolumeResponse(m.Year, m.Month, m.MaxWeight, m.WeightUnit))
                        .ToList()
                ))
                .ToList()
        );
    }
}
