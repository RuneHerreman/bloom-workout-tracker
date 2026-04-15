using System.Security.Claims;
using Bloom.Application.Contracts.Data.Templates;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.Templates;
using Bloom.Domain.Templates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Templates;

public sealed record CreateTemplateRequest(
    string Name,
    List<WorkoutTemplateExerciseData> Exercises
);

public class CreateTemplateController
{
    public static async Task<Results<Ok<Guid>, BadRequest<string>, UnauthorizedHttpResult>> Invoke(
        ClaimsPrincipal user,
        [FromBody] CreateTemplateRequest input,
        [FromServices] IUseCase<CreateWorkoutTemplateInput, WorkoutTemplateId> createTemplateUseCase
    )
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? user.FindFirstValue("sub");

        if (!Guid.TryParse(userIdClaim, out var userId))
            return TypedResults.Unauthorized();

        var result = await createTemplateUseCase.Execute(new CreateWorkoutTemplateInput(
            userId,    
            input.Name,
            input.Exercises
        ));
        
        return TypedResults.Ok(result.Value);
    }
}