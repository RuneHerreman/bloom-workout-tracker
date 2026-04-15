using Bloom.Application.Contracts.Data;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Exercises;

public class GetAllExercisesController
{
    public static async Task<Results<Ok<IEnumerable<ExerciseData>>, UnauthorizedHttpResult>> Invoke(
        ClaimsPrincipal user,
        [FromServices] IUseCase<GetAlLExercisesInput, IEnumerable<ExerciseData>> getAllExercisesUseCase)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? user.FindFirstValue("sub");

        if (!Guid.TryParse(userIdClaim, out var userId))
            return TypedResults.Unauthorized();

        var result = await getAllExercisesUseCase.Execute(new GetAlLExercisesInput(userId));
        return TypedResults.Ok(result);
    }

}
