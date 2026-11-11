using Bloom.Application.Contracts;
using Bloom.Application.Contracts.Data.Filters;
using Bloom.Application.Contracts.Ports;
using Bloom.Domain.LoggedWorkouts.Enums;
using Bloom.Domain.LoggedWorkouts.ValueObjects;
using Bloom.Shared.Exceptions;

namespace Bloom.Application.Leaderboard;

public sealed record GetMonthlyLeaderboardInput(int Year, int Month);

/// <summary>One user's standing for a calendar month (UTC boundaries).</summary>
/// <param name="TotalVolumeKg">Sum of reps × weight (kg) over working sets — warm-ups excluded.</param>
/// <param name="NewPrs">Exercises where this month's best weight beat every earlier month.</param>
/// <param name="ActiveDays">Distinct days with at least one logged workout.</param>
/// <param name="IsHidden">The user opted out of the leaderboard. Only ever true on the current user's
/// own entry: hidden users are left out for everyone else.</param>
public sealed record LeaderboardEntryData(
    Guid UserId,
    string Username,
    bool IsCurrentUser,
    decimal TotalVolumeKg,
    int NewPrs,
    int ActiveDays,
    int Workouts,
    bool IsHidden
);

public sealed record GetMonthlyLeaderboardOutput(int Year, int Month, IReadOnlyList<LeaderboardEntryData> Entries);

/// <summary>
/// Monthly comparison across users who opted in. Only usernames and aggregate numbers are
/// exposed — never individual workouts, names, or emails. A user who opted out is left out
/// for everyone else but still sees their own (hidden) standing.
/// </summary>
public class GetMonthlyLeaderboard(
    ICurrentUser currentUser,
    IFindUsersQuery usersQuery,
    IFindLoggedWorkoutsQuery logsQuery
) : IUseCase<GetMonthlyLeaderboardInput, GetMonthlyLeaderboardOutput>
{
    private const decimal LbsToKg = 0.45359237m;

    public async Task<GetMonthlyLeaderboardOutput> Execute(GetMonthlyLeaderboardInput input, CancellationToken ct = default)
    {
        if (input.Month is < 1 or > 12)
            throw new BloomGeneralException("Month must be between 1 and 12.");
        if (input.Year is < 2000 or > 2100)
            throw new BloomGeneralException("Year must be between 2000 and 2100.");

        var monthStart = new DateTime(input.Year, input.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var users = await usersQuery.Fetch(UserDataFilters.OnLeaderboardFor(currentUser.UserId), ct);
        // Everything up to the end of the month: earlier history is needed to detect PRs.
        var logs = await logsQuery.Fetch(LoggedWorkoutDataFilters.LoggedBefore(monthEnd), ct);
        var logsByUser = logs.ToLookup(l => l.UserId);

        var entries = users
            .Select(u => BuildEntry(u, logsByUser[u.Id].ToList(), monthStart, monthEnd))
            .OrderByDescending(e => e.TotalVolumeKg)
            .ThenByDescending(e => e.NewPrs)
            .ThenByDescending(e => e.ActiveDays)
            .ThenBy(e => e.Username, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new GetMonthlyLeaderboardOutput(input.Year, input.Month, entries);
    }

    private LeaderboardEntryData BuildEntry(UserData user, List<LoggedWorkoutData> logs, DateTime monthStart, DateTime monthEnd)
    {
        var inMonth = logs.Where(l => l.LoggedAt >= monthStart && l.LoggedAt < monthEnd).ToList();
        var before = logs.Where(l => l.LoggedAt < monthStart).ToList();

        var volume = inMonth
            .SelectMany(l => l.LoggedExercises)
            .SelectMany(e => e.Sets)
            .Where(s => s.Marker != SetMarker.WarmUp && s.Reps is > 0 && s.Weight is not null)
            .Sum(s => s.Reps!.Value * ToKg(s.Weight!));

        var bestBefore = BestWeightPerExercise(before);
        var bestInMonth = BestWeightPerExercise(inMonth);
        var newPrs = bestInMonth.Count(kv => bestBefore.TryGetValue(kv.Key, out var prev) && kv.Value > prev);

        var activeDays = inMonth.Select(l => l.LoggedAt.Date).Distinct().Count();

        return new LeaderboardEntryData(
            user.Id,
            user.Username,
            user.Id == currentUser.UserId.Value,
            decimal.Round(volume, 1),
            newPrs,
            activeDays,
            inMonth.Count,
            !user.ShowOnLeaderboard
        );
    }

    private static Dictionary<Guid, decimal> BestWeightPerExercise(IEnumerable<LoggedWorkoutData> logs) =>
        logs
            .SelectMany(l => l.LoggedExercises)
            .SelectMany(e => e.Sets
                .Where(s => s.Weight is not null && s.Marker != SetMarker.WarmUp)
                .Select(s => (e.ExerciseId, Kg: ToKg(s.Weight!))))
            .GroupBy(x => x.ExerciseId)
            .ToDictionary(g => g.Key, g => g.Max(x => x.Kg));

    private static decimal ToKg(WeightData weight) =>
        weight.Unit == WeightUnit.Lbs ? weight.Value * LbsToKg : weight.Value;
}
