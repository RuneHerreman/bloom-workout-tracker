using Bloom.Application.Contracts.Ports;
using Bloom.Application.LoggedWorkouts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.LoggedWorkouts;

public sealed record ListImportBatchesRequest(
    [FromServices] IUseCase<ListImportBatchesInput, ListImportBatchesOutput> UseCase
);

public sealed record UndoImportBatchRequest(
    [FromRoute] Guid ImportBatchId,
    [FromServices] IUseCase<UndoImportBatchInput, UndoImportBatchOutput> UseCase
);

public static class ListImportBatchesController
{
    public static async Task<Ok<IReadOnlyList<ImportBatchSummary>>> Invoke([AsParameters] ListImportBatchesRequest request, CancellationToken ct)
    {
        var output = await request.UseCase.Execute(new ListImportBatchesInput(), ct);
        return TypedResults.Ok(output.Imports);
    }
}

public static class UndoImportBatchController
{
    public static async Task<Ok<UndoImportBatchOutput>> Invoke([AsParameters] UndoImportBatchRequest request, CancellationToken ct)
    {
        var output = await request.UseCase.Execute(new UndoImportBatchInput(request.ImportBatchId), ct);
        return TypedResults.Ok(output);
    }
}
