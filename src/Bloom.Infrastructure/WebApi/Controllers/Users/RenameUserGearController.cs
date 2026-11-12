using System.ComponentModel.DataAnnotations;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Users;

public sealed record RenameUserGearRequest(
    [FromBody] RenameUserGearBody Body,
    [FromServices] IUseCase<RenameUserGearInput, RenameUserGearOutput> UseCase
);

public sealed record RenameUserGearBody(
    [Required] string From,
    [Required] [MaxLength(100)] string To
);

public sealed record RenameUserGearResponse(int Workouts, int Templates);

public static class RenameUserGearController
{
    public static async Task<Ok<RenameUserGearResponse>> Invoke(
        [AsParameters] RenameUserGearRequest request,
        CancellationToken ct
    )
    {
        var output = await request.UseCase.Execute(new RenameUserGearInput(request.Body.From, request.Body.To), ct);
        return TypedResults.Ok(new RenameUserGearResponse(output.Workouts, output.Templates));
    }
}
