using System.ComponentModel.DataAnnotations;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Users;

public sealed record UpdateGearIconRequest(
    [FromBody] UpdateGearIconBody Body,
    [FromServices] IUseCase<UpdateGearIconInput, UpdateGearIconOutput> UseCase
);

/// <param name="Kind">An icon kind, or null to use the one guessed from the name.</param>
public sealed record UpdateGearIconBody(
    [Required] string Name,
    [MaxLength(32)] string? Kind
);

public static class UpdateGearIconController
{
    public static async Task<NoContent> Invoke(
        [AsParameters] UpdateGearIconRequest request,
        CancellationToken ct
    )
    {
        await request.UseCase.Execute(new UpdateGearIconInput(request.Body.Name, request.Body.Kind), ct);
        return TypedResults.NoContent();
    }
}
