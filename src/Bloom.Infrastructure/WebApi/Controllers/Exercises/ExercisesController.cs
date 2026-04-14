using Bloom.Application.Contracts.Data;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Bloom.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Exercises;

public record GetAllExercisesRequest(
    string Token
);

public class GetAllExercisesController
{
    public static async Task<Results<Ok<IEnumerable<ExerciseData>>, UnauthorizedHttpResult>> Invoke(
        [FromBody] GetAllExercisesRequest input,
        [FromServices] IUseCase<GetAlLExercisesInput, IEnumerable<ExerciseData>> getAllExercisesUseCase)
    {
        try
        {
            var userId = JwtProvider.GetUserId(input.Token);
            
            if (userId is null)
                return TypedResults.Unauthorized();
            
            var result = await getAllExercisesUseCase.Execute(new GetAlLExercisesInput(userId.Value));
            return TypedResults.Ok(result);
        }
        catch (Exception e)
        {
            return TypedResults.Unauthorized();
        }
    }

}
