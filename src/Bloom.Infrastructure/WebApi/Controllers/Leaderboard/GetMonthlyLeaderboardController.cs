using Bloom.Application.Contracts.Ports;
using Bloom.Application.Leaderboard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bloom.Infrastructure.WebApi.Controllers.Leaderboard;

public sealed record GetMonthlyLeaderboardRequest(
    [FromQuery] int? Year,
    [FromQuery] int? Month,
    [FromServices] IUseCase<GetMonthlyLeaderboardInput, GetMonthlyLeaderboardOutput> UseCase
);

public sealed record LeaderboardEntryResponse(
    string Username,
    bool IsCurrentUser,
    decimal TotalVolumeKg,
    int NewPrs,
    int ActiveDays,
    int Workouts,
    bool IsHidden
);

public sealed record LeaderboardResponse(int Year, int Month, IReadOnlyList<LeaderboardEntryResponse> Entries);

public static class GetMonthlyLeaderboardController
{
    public static async Task<Ok<LeaderboardResponse>> Invoke(
        [AsParameters] GetMonthlyLeaderboardRequest request,
        CancellationToken ct
    )
    {
        var now = DateTime.UtcNow;
        var output = await request.UseCase.Execute(new GetMonthlyLeaderboardInput(
            request.Year ?? now.Year,
            request.Month ?? now.Month
        ), ct);

        // User ids stay server-side; clients only ever see usernames.
        return TypedResults.Ok(new LeaderboardResponse(
            output.Year,
            output.Month,
            output.Entries
                .Select(e => new LeaderboardEntryResponse(
                    e.Username, e.IsCurrentUser, e.TotalVolumeKg, e.NewPrs, e.ActiveDays, e.Workouts, e.IsHidden))
                .ToList()
        ));
    }
}
