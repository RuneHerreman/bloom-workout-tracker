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
    HttpContext HttpContext
);

public sealed record LoginUserBody(
    [Required, EmailAddress] string Email,
    [Required] string Password
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
            request.Body.Password
        ), ct);

        var opts = request.JwtOptions.Value;
        request.HttpContext.Response.Cookies.Append(opts.CookieName, output.Token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = request.HttpContext.Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.AddMinutes(opts.ExpiryMinutes),
            Path = "/"
        });

        return TypedResults.NoContent();
    }
}
