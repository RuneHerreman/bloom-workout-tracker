using System.ComponentModel.DataAnnotations;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Users;

public sealed record RegisterUserRequest(
    [FromBody] RegisterUserBody Body,
    [FromServices] IUseCase<RegisterUserInput, RegisterUserOutput> UseCase
);

public sealed record RegisterUserBody(
    [Required, EmailAddress] string Email,
    [Required, MinLength(3), MaxLength(128)] string Username,
    [Required, MinLength(8)] string Password
);

public sealed record RegisterUserResponse(Guid UserId, string Token);

public static class RegisterUserController
{
    public static async Task<Results<Ok<RegisterUserResponse>, BadRequest>> Invoke(
        [AsParameters] RegisterUserRequest request
    )
    {
        var output = await request.UseCase.Execute(new RegisterUserInput(
            request.Body.Email,
            request.Body.Username,
            request.Body.Password
        ));

        return TypedResults.Ok(new RegisterUserResponse(output.UserId, output.Token));
    }
}
