using System.ComponentModel.DataAnnotations;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Users;

public sealed record UpdateUserGearRequest(
    [FromBody] UpdateUserGearBody Body,
    [FromServices] IUseCase<UpdateUserGearInput, UpdateUserGearOutput> UseCase
);

public sealed record UpdateUserGearBody(
    [Required]
    [MaxLength(100, ErrorMessage = "At most 100 gear items are allowed")]
    List<string> Gear
);

public static class UpdateUserGearController
{
    public static async Task<NoContent> Invoke(
        [AsParameters] UpdateUserGearRequest request,
        CancellationToken ct
    )
    {
        await request.UseCase.Execute(new UpdateUserGearInput(
            request.Body.Gear
        ), ct);

        return TypedResults.NoContent();
    }
}
