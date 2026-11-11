using System.ComponentModel.DataAnnotations;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.LoggedWorkouts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.LoggedWorkouts;

public sealed record ImportLoggedWorkoutsRequest(
    [FromBody] ImportLoggedWorkoutsBody Body,
    [FromServices] IUseCase<ImportLoggedWorkoutsInput, ImportLoggedWorkoutsOutput> UseCase
);

public sealed record ImportLoggedWorkoutsBody(
    [Required, MaxLength(ImportLoggedWorkouts.MaxWorkouts)] List<ImportWorkoutBody> Workouts,
    [MaxLength(200)] string? FileName = null,
    List<Guid>? CreatedExerciseIds = null,
    List<Guid>? CreatedTemplateIds = null
);

/// <summary>
/// One imported workout. Sets are validated by the domain rather than by attributes,
/// so values the app itself stores (e.g. 0 kg for bodyweight) round-trip, and a bad
/// workout is reported on its own instead of rejecting the whole file.
/// </summary>
public sealed record ImportWorkoutBody(
    [Required, MinLength(1), MaxLength(200)] string Name,
    [Required] DateTime LoggedAt,
    [Required] List<ImportExerciseBody> Exercises,
    [MaxLength(2000)] string? Note = null,
    [MaxLength(100)] string? SourceId = null
);

public sealed record ImportExerciseBody(
    [Required] Guid ExerciseId,
    int Order,
    [Required] List<ImportSetBody> Sets,
    string? GpxData = null,
    [MaxLength(2000)] string? Note = null,
    List<string>? Gear = null
);

public sealed record ImportSetBody(
    [Required] string Type,
    int Order,
    TimeSpan? Duration,
    decimal? Distance,
    string? DistanceUnit,
    int? Reps,
    decimal? Weight,
    string? WeightUnit,
    int? Rir,
    string? Marker = null
);

public sealed record ImportFailureResponse(int Index, string Name, DateTime LoggedAt, string Reason);

public sealed record ImportLoggedWorkoutsResponse(int Imported, int Skipped, IReadOnlyList<ImportFailureResponse> Failures, IReadOnlyList<Guid> ImportedIds, Guid? BatchId);

public static class ImportLoggedWorkoutsController
{
    public static async Task<Ok<ImportLoggedWorkoutsResponse>> Invoke(
        [AsParameters] ImportLoggedWorkoutsRequest request,
        CancellationToken ct
    )
    {
        var input = new ImportLoggedWorkoutsInput(request.Body.Workouts.Select(w => new ImportWorkoutInput(
            w.Name,
            w.LoggedAt,
            w.Exercises.Select(e => new LoggedExerciseInput(
                e.ExerciseId,
                e.Order,
                e.Sets.Select(s => new LoggedSetInput(s.Type, s.Order, s.Duration, s.Distance, s.DistanceUnit, s.Reps, s.Weight, s.WeightUnit, s.Rir, s.Marker)).ToList(),
                e.GpxData,
                e.Note,
                e.Gear
            )).ToList(),
            w.Note,
            w.SourceId
        )).ToList(), request.Body.FileName, request.Body.CreatedExerciseIds, request.Body.CreatedTemplateIds);

        var output = await request.UseCase.Execute(input, ct);

        return TypedResults.Ok(new ImportLoggedWorkoutsResponse(
            output.Imported,
            output.Skipped,
            output.Failures.Select(f => new ImportFailureResponse(f.Index, f.Name, f.LoggedAt, f.Reason)).ToList(),
            output.ImportedIds,
            output.BatchId
        ));
    }
}
