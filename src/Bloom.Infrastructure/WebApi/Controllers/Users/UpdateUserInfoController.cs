using System.ComponentModel.DataAnnotations;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Users;

public sealed record UpdateUserInfoRequest(
    [FromBody] UpdateUserInfoBody Body,
    [FromServices] IUseCase<UpdateUserInfoInput, UpdateUserInfoOutput> UseCase
);

public sealed record UpdateUserInfoBody(
    [Required, EmailAddress] string Email,
    [Required, MinLength(3), MaxLength(128)] string Username,
    [Required, MinLength(1), MaxLength(100)] string FirstName,
    [Required, MinLength(1), MaxLength(100)] string LastName,
    [Range(typeof(decimal), "0.1", "500")] decimal Weight,
    [Range(1, 300)] int Height,
    [Range(0, 7)] int ActiveDays
);

public sealed record UpdateUserInfoResponse(Guid UserId);

public static class UpdateUserInfoController
{
    public static async Task<Results<Ok<UpdateUserInfoResponse>, BadRequest>> Invoke(
        [AsParameters] UpdateUserInfoRequest request,
        CancellationToken ct
    )
    {
        var output = await request.UseCase.Execute(new UpdateUserInfoInput(
            request.Body.Email,
            request.Body.Username,
            request.Body.FirstName,
            request.Body.LastName,
            request.Body.Weight,
            request.Body.Height,
            request.Body.ActiveDays
        ), ct);

        return TypedResults.Ok(new UpdateUserInfoResponse(output.UserId));
    }
}
