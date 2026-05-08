using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Users;

public sealed record DeleteUserRequest(
    [FromRoute] Guid UserId,
    [FromServices] IUseCase<DeleteUserInput> UseCase
);

public static class DeleteUserController
{
    public static async Task<Results<NoContent, BadRequest>> Invoke(
        [AsParameters] DeleteUserRequest request
    )
    {
        await request.UseCase.Execute(new DeleteUserInput(request.UserId));

        return TypedResults.NoContent();
    }
}
