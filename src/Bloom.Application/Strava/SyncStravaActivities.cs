using Bloom.Application.Contracts.Ports;
using Bloom.Domain.Strava;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Strava;

public sealed record SyncStravaActivitiesInput;
public sealed record SyncStravaActivitiesOutput(int Imported);

public class SyncStravaActivities(
    IUnitOfWork uow,
    ICurrentUser currentUser,
    IStravaActivityImporter importer,
    ILogger<SyncStravaActivities> logger
) : IUseCase<SyncStravaActivitiesInput, SyncStravaActivitiesOutput>
{
    public async Task<SyncStravaActivitiesOutput> Execute(SyncStravaActivitiesInput input, CancellationToken ct = default)
    {
        var connection = await uow.Repo<IStravaConnectionRepository>().ByUserId(currentUser.UserId, ct);

        if (!connection.HasValue)
            return new SyncStravaActivitiesOutput(0);

        var conn = connection.Value;
        var after = conn.LastSyncedAt;

        logger.LogInformation("Syncing Strava activities for user {UserId} since {After}", currentUser.UserId, after);

        var imported = await importer.ImportAll(conn, currentUser.UserId, after, ct);

        conn.UpdateLastSyncedAt(DateTime.UtcNow);
        await uow.Repo<IStravaConnectionRepository>().Save(conn);
        await uow.Do(ct);

        logger.LogInformation("Strava sync complete: {Count} new activities", imported);

        return new SyncStravaActivitiesOutput(imported);
    }
}
