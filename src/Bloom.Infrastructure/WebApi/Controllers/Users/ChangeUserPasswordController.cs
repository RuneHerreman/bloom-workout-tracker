using System.ComponentModel.DataAnnotations;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Users;

public sealed record ChangeUserPasswordRequest(
    [FromBody] ChangeUserPasswordBody Body,
    [FromServices] IUseCase<ChangeUserPasswordInput> UseCase
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
        await request.UseCase.Execute(new ChangeUserPasswordInput(
            request.Body.OldPassword,
            request.Body.NewPassword
        ), ct);

        return TypedResults.NoContent();
    }
}
