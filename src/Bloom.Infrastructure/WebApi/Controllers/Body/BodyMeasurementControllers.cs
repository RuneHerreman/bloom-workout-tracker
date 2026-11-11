using Bloom.Application.Body;
using Bloom.Application.Contracts.Ports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Body;

public sealed record ListBodyMeasurementsRequest(
    [FromQuery] DateOnly? From,
    [FromQuery] DateOnly? To,
    [FromServices] IUseCase<ListBodyMeasurementsInput, ListBodyMeasurementsOutput> UseCase
);

/// <param name="BodyFatPct">A new body fat reading; leave out to keep the day's existing one.</param>
/// <param name="ClearBodyFat">Remove the day's body fat reading.</param>
public sealed record LogBodyMeasurementBody(decimal WeightKg, decimal? BodyFatPct = null, bool ClearBodyFat = false);

public sealed record LogBodyMeasurementRequest(
    [FromRoute] DateOnly Date,
    [FromBody] LogBodyMeasurementBody Body,
    [FromServices] IUseCase<LogBodyMeasurementInput, LogBodyMeasurementOutput> UseCase
);

public sealed record DeleteBodyMeasurementRequest(
    [FromRoute] DateOnly Date,
    [FromServices] IUseCase<DeleteBodyMeasurementInput> UseCase
);

public static class ListBodyMeasurementsController
{
    public static async Task<Ok<IReadOnlyList<BodyMeasurementData>>> Invoke([AsParameters] ListBodyMeasurementsRequest request, CancellationToken ct)
    {
        var output = await request.UseCase.Execute(new ListBodyMeasurementsInput(request.From, request.To), ct);
        return TypedResults.Ok(output.Entries);
    }
}

public static class LogBodyMeasurementController
{
    public static async Task<Ok<BodyMeasurementData>> Invoke([AsParameters] LogBodyMeasurementRequest request, CancellationToken ct)
    {
        var output = await request.UseCase.Execute(new LogBodyMeasurementInput(
            request.Date, request.Body.WeightKg, request.Body.BodyFatPct, request.Body.ClearBodyFat), ct);
        return TypedResults.Ok(output.Entry);
    }
}

public static class DeleteBodyMeasurementController
{
    public static async Task<NoContent> Invoke([AsParameters] DeleteBodyMeasurementRequest request, CancellationToken ct)
    {
        await request.UseCase.Execute(new DeleteBodyMeasurementInput(request.Date), ct);
        return TypedResults.NoContent();
    }
}
