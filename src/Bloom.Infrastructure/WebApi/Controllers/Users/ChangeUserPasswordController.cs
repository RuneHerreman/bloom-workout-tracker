using System.ComponentModel.DataAnnotations;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Bloom.Infrastructure.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Bloom.Infrastructure.WebApi.Controllers.Users;

public sealed record ChangeUserPasswordRequest(
    [FromBody] ChangeUserPasswordBody Body,
    [FromServices] IUseCase<ChangeUserPasswordInput, ChangeUserPasswordOutput> UseCase,
    [FromServices] IOptions<JwtOptions> JwtOptions,
    [FromServices] TimeProvider Clock,
    HttpContext HttpContext
);

public sealed record ChangeUserPasswordBody(
    [Required] string OldPassword,
    [Required, MinLength(8)] string NewPassword
);

public static class ChangeUserPasswordController
{
    public static async Task<Results<NoContent, BadRequest>> Invoke(
        [AsParameters] ChangeUserPasswordRequest request,
        CancellationToken ct
    )
    {
        var persistent = SessionClaims.IsPersistent(request.HttpContext.User);
        var output = await request.UseCase.Execute(new ChangeUserPasswordInput(
            request.Body.OldPassword,
            request.Body.NewPassword,
            persistent
        ), ct);

        // Every older token (other devices included) is now refused at its next
        // renewal; this one carries the new stamp so the caller stays signed in.
        AuthCookie.Append(request.HttpContext, request.JwtOptions.Value, output.Token, persistent, request.Clock.GetUtcNow());

        return TypedResults.NoContent();
    }
}
