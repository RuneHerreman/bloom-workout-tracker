using System.ComponentModel.DataAnnotations;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Users;

public sealed record UpdateLeaderboardVisibilityRequest(
    [FromBody] UpdateLeaderboardVisibilityBody Body,
    [FromServices] IUseCase<UpdateLeaderboardVisibilityInput, UpdateLeaderboardVisibilityOutput> UseCase
);

public sealed record UpdateLeaderboardVisibilityBody(
    [Required] bool? ShowOnLeaderboard
);

public static class UpdateLeaderboardVisibilityController
{
    public static async Task<Results<NoContent, ValidationProblem>> Invoke(
        [AsParameters] UpdateLeaderboardVisibilityRequest request,
        CancellationToken ct
    )
    {
        // Checked explicitly: a missing bool must not silently become false (hiding the user).
        if (request.Body.ShowOnLeaderboard is not { } show)
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(UpdateLeaderboardVisibilityBody.ShowOnLeaderboard)] = ["The ShowOnLeaderboard field is required."]
            });

        await request.UseCase.Execute(new UpdateLeaderboardVisibilityInput(show), ct);

        return TypedResults.NoContent();
    }
}
