using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Strava;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Strava;

public sealed record SyncStravaActivitiesInput;
public sealed record SyncStravaActivitiesOutput(int Imported);

public class SyncStravaActivities(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    StravaImportService importService,
    ILogger<SyncStravaActivities> logger
) : IUseCase<SyncStravaActivitiesInput, SyncStravaActivitiesOutput>
{
    // The frontend triggers a sync on every app load; skip when we synced recently
    // so those loads stay cheap and we don't burn Strava's API quota.
    private static readonly TimeSpan SyncCooldown = TimeSpan.FromMinutes(5);

    public async Task<SyncStravaActivitiesOutput> Execute(SyncStravaActivitiesInput input, CancellationToken ct = default)
    {
        var connRepo = uow.Repo<IStravaConnectionRepository>();
        var connection = await connRepo.ByUserId(currentUser.UserId, ct);
        if (!connection.HasValue) return new SyncStravaActivitiesOutput(0);

        var conn = connection.Value;

        if (conn.LastSyncedAt.HasValue && DateTime.UtcNow - conn.LastSyncedAt.Value < SyncCooldown)
        {
            logger.LogDebug("Skipping Strava sync for user {UserId}; last sync was {LastSyncedAt}", currentUser.UserId, conn.LastSyncedAt);
            return new SyncStravaActivitiesOutput(0);
        }
        var token = await importService.EnsureValidToken(conn, connRepo, uow, ct);

        var after = conn.LastSyncedAt;
        var afterUnix = after.HasValue ? (long?)new DateTimeOffset(after.Value, TimeSpan.Zero).ToUnixTimeSeconds() : null;

        logger.LogInformation("Syncing Strava activities for user {UserId} since {After}", currentUser.UserId, after);

        var result = await importService.ImportLoop(token, afterUnix, currentUser.UserId, uow, ct);

        // Use the latest seen activity date as the sync cursor so activities uploaded late
        // by Strava aren't missed on the next run.
        conn.UpdateLastSyncedAt(result.LatestActivityAt ?? DateTime.UtcNow);
        await connRepo.Save(conn);
        await uow.Do(ct);

        logger.LogInformation("Strava sync complete: {Count} new activities", result.Imported);
        return new SyncStravaActivitiesOutput(result.Imported);
    }
}
