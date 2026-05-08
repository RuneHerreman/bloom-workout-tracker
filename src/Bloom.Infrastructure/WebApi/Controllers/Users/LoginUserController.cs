using System.ComponentModel.DataAnnotations;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Users;

public sealed record LoginUserRequest(
    [FromBody] LoginUserBody Body,
    [FromServices] IUseCase<LoginUserInput, LoginUserOutput> UseCase
);

public sealed record LoginUserBody(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public sealed record LoginUserResponse(
    Guid UserId,
    string Username,
    string Email
);

public static class LoginUserController
{
    public static async Task<Results<Ok<LoginUserResponse>, BadRequest>> Invoke(
        [AsParameters] LoginUserRequest request
    )
    {
        var output = await request.UseCase.Execute(new LoginUserInput(
            request.Body.Email,
            request.Body.Password
        ));

        return TypedResults.Ok(new LoginUserResponse(
            output.UserId,
            output.Username,
            output.Email
        ));
    }
}
