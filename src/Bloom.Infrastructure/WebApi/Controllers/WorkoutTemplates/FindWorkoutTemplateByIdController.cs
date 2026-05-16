using Bloom.Application.Contracts.Ports;
using Bloom.Application.WorkoutTemplates;
using Bloom.Infrastructure.WebApi.Controllers.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.WorkoutTemplates;

public sealed record FindWorkoutTemplateByIdRequest(
    [FromRoute] Guid TemplateId,
    [FromServices] IUseCase<FindWorkoutTemplateByIdInput, FindWorkoutTemplateByIdOutput> UseCase
);

public static class FindWorkoutTemplateByIdController
{
    public static async Task<Results<Ok<WorkoutTemplate>, BadRequest>> Invoke(
        [AsParameters] FindWorkoutTemplateByIdRequest request,
        CancellationToken ct
    )
    {
        var output = await request.UseCase.Execute(new FindWorkoutTemplateByIdInput(request.TemplateId), ct);

        return TypedResults.Ok(output.Template.ToResponse());
    }
}