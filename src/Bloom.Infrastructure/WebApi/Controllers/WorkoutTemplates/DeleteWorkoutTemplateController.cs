using Bloom.Application.Contracts.Ports;
using Bloom.Application.WorkoutTemplates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.WorkoutTemplates;

public sealed record DeleteWorkoutTemplateRequest(
    [FromRoute] Guid TemplateId,
    [FromServices] IUseCase<DeleteWorkoutTemplateInput> UseCase
);

public static class DeleteWorkoutTemplateController
{
    public static async Task<Results<NoContent, BadRequest>> Invoke(
        [AsParameters] DeleteWorkoutTemplateRequest request
    )
    {
        await request.UseCase.Execute(new DeleteWorkoutTemplateInput(
            request.TemplateId
        ));

        return TypedResults.NoContent();
    }
}
