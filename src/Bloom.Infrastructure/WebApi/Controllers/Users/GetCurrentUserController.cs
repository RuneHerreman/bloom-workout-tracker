using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Bloom.Infrastructure.WebApi.Controllers.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Users;

public sealed record GetCurrentUserRequest(
    [FromServices] IUseCase<GetCurrentUserInput, GetCurrentUserOutput> UseCase
);

public static class GetCurrentUserController
{
    public static async Task<Results<Ok<User>, BadRequest>> Invoke(
        [AsParameters] GetCurrentUserRequest request
    )
    {
        var output = await request.UseCase.Execute(new GetCurrentUserInput());
        return TypedResults.Ok(output.User.ToResponse());
    }
}
