using System.ComponentModel.DataAnnotations;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Bloom.Infrastructure.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Bloom.Infrastructure.WebApi.Controllers.Users;

public sealed record LoginUserRequest(
    [FromBody] LoginUserBody Body,
    [FromServices] IUseCase<LoginUserInput, LoginUserOutput> UseCase,
    [FromServices] IOptions<JwtOptions> JwtOptions,
    [FromServices] TimeProvider Clock,
    HttpContext HttpContext
);

public sealed record LoginUserBody(
    [Required, EmailAddress] string Email,
    [Required] string Password,
    bool RememberMe = false
);

public static class LoginUserController
{
    public static async Task<NoContent> Invoke(
        [AsParameters] LoginUserRequest request,
        CancellationToken ct
    )
    {
        var output = await request.UseCase.Execute(new LoginUserInput(
            request.Body.Email,
            request.Body.Password,
            request.Body.RememberMe
        ), ct);

        AuthCookie.Append(request.HttpContext, request.JwtOptions.Value, output.Token, output.Persistent, request.Clock.GetUtcNow());

        return TypedResults.NoContent();
    }
}
