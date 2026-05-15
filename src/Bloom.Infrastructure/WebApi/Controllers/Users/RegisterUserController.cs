using System.ComponentModel.DataAnnotations;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Bloom.Infrastructure.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Bloom.Infrastructure.WebApi.Controllers.Users;

public sealed record RegisterUserRequest(
    [FromBody] RegisterUserBody Body,
    [FromServices] IUseCase<RegisterUserInput, RegisterUserOutput> UseCase,
    [FromServices] IOptions<JwtOptions> JwtOptions,
    HttpContext HttpContext
);

public sealed record RegisterUserBody(
    [Required, EmailAddress] string Email,
    [Required, MinLength(3), MaxLength(128)] string Username,
    [Required, MinLength(8)] string Password,
    [Required, MinLength(1), MaxLength(100)] string FirstName,
    [Required, MinLength(1), MaxLength(100)] string LastName,
    [Range(typeof(decimal), "0.1", "500")] decimal Weight,
    [Range(1, 300)] int Height,
    [Range(0, 7)] int ActiveDays
);

public static class RegisterUserController
{
    public static async Task<NoContent> Invoke(
        [AsParameters] RegisterUserRequest request
    )
    {
        var output = await request.UseCase.Execute(new RegisterUserInput(
            request.Body.Email,
            request.Body.Username,
            request.Body.Password,
            request.Body.FirstName,
            request.Body.LastName,
            request.Body.Weight,
            request.Body.Height,
            request.Body.ActiveDays
        ));

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
