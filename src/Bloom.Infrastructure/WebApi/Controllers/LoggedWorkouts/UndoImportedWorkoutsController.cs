using System.ComponentModel.DataAnnotations;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.LoggedWorkouts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.LoggedWorkouts;

public sealed record UndoImportedWorkoutsRequest(
    [FromBody] UndoImportedWorkoutsBody Body,
    [FromServices] IUseCase<UndoImportedWorkoutsInput, UndoImportedWorkoutsOutput> UseCase
);

public sealed record UndoImportedWorkoutsBody([Required, MaxLength(ImportLoggedWorkouts.MaxWorkouts)] List<Guid> LoggedWorkoutIds);

public sealed record UndoImportedWorkoutsResponse(int Deleted);

public static class UndoImportedWorkoutsController
{
    public static async Task<Ok<UndoImportedWorkoutsResponse>> Invoke(
        [AsParameters] UndoImportedWorkoutsRequest request,
        CancellationToken ct
    )
    {
        var output = await request.UseCase.Execute(new UndoImportedWorkoutsInput(request.Body.LoggedWorkoutIds), ct);
        return TypedResults.Ok(new UndoImportedWorkoutsResponse(output.Deleted));
    }
}
