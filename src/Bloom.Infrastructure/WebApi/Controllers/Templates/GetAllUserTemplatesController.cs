using System.Security.Claims;
using Bloom.Application.Contracts.Data.Templates;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.Templates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Templates;

public class GetAllUserTemplatesController
{
    public static async Task<Results<Ok<IReadOnlyList<WorkoutTemplateData>>, BadRequest<string>, UnauthorizedHttpResult>> Invoke(
        ClaimsPrincipal user,
        [FromServices] IUseCase<GetAllUserTemplatesInput, IReadOnlyList<WorkoutTemplateData>> getAllUserTemplatesUseCase
    )
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? user.FindFirstValue("sub");

        if (!Guid.TryParse(userIdClaim, out var userId))
            return TypedResults.Unauthorized();

        var result = await getAllUserTemplatesUseCase.Execute(new GetAllUserTemplatesInput(userId));
        
        return TypedResults.Ok(result);
    }
}