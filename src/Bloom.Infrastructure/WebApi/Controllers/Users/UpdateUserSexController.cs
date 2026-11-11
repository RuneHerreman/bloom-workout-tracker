using System.ComponentModel.DataAnnotations;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Users;

public sealed record UpdateUserSexRequest(
    [FromBody] UpdateUserSexBody Body,
    [FromServices] IUseCase<UpdateUserSexInput, UpdateUserSexOutput> UseCase
);

/// <param name="Sex">"Male", "Female", or null to clear it.</param>
public sealed record UpdateUserSexBody([MaxLength(16)] string? Sex);

public static class UpdateUserSexController
{
    public static async Task<NoContent> Invoke([AsParameters] UpdateUserSexRequest request, CancellationToken ct)
    {
        await request.UseCase.Execute(new UpdateUserSexInput(request.Body.Sex), ct);
        return TypedResults.NoContent();
    }
}
