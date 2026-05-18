using Bloom.Application.Contracts.Ports;
using Bloom.Application.LoggedWorkouts;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.Strava;
using Bloom.Domain.Users;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bloom.Infrastructure.Strava;

public class StravaActivityImporter(
    StravaApiClient apiClient,
    StravaActivityMapper mapper,
    DomainDbContext context,
    ILogger<StravaActivityImporter> logger
) : IStravaActivityImporter
{
    public async Task<int> ImportAll(StravaConnection connection, UserId userId, DateTime? after = null, CancellationToken ct = default)
    {
        var token = await apiClient.GetValidToken(connection, _ => context.SaveChangesAsync(ct), ct);
        var afterUnix = after.HasValue ? (long?)new DateTimeOffset(after.Value, TimeSpan.Zero).ToUnixTimeSeconds() : null;
        var imported = 0;
        var page = 1;

        while (true)
        {
            var activities = await apiClient.GetActivities(token, page, 100, afterUnix, ct);
            if (activities.Count == 0) break;

            foreach (var activity in activities)
            {
                if (ct.IsCancellationRequested) break;
                if (await ImportActivity(connection, userId, activity, token, ct))
                    imported++;
            }

            if (activities.Count < 100) break;
            page++;
        }

        return imported;
    }

    public async Task<bool> ImportSingle(StravaConnection connection, UserId userId, long activityId, CancellationToken ct = default)
    {
        var token = await apiClient.GetValidToken(connection, _ => context.SaveChangesAsync(ct), ct);
        var activity = await apiClient.GetActivity(token, activityId, ct);

        if (activity is null)
        {
            logger.LogWarning("Strava activity {ActivityId} not found", activityId);
            return false;
        }

        return await ImportActivity(connection, userId, activity, token, ct);
    }

    private async Task<bool> ImportActivity(StravaConnection connection, UserId userId, StravaActivity activity, string token, CancellationToken ct)
    {
        var externalId = $"strava:{activity.Id}";

        var alreadyImported = await context.LoggedWorkouts
            .AnyAsync(lw => lw.UserId == userId && lw.ExternalId == externalId, ct);

        if (alreadyImported)
        {
            logger.LogDebug("Skipping already-imported Strava activity {ActivityId}", activity.Id);
            return false;
        }

        StravaStreams? streams = null;
        try { streams = await apiClient.GetActivityStreams(token, activity.Id, ct); }
        catch (Exception ex) { logger.LogWarning(ex, "Could not fetch streams for activity {ActivityId}", activity.Id); }

        var input = await mapper.Map(activity, streams, ct);
        if (input is null) return false;

        var exercises = input.Exercises.Select(e => e.ToLoggedExercise()).ToList();
        var log = LoggedWorkout.Create(userId, input.Name, exercises, input.Note, input.LoggedAt, externalId);

        context.LoggedWorkouts.Add(log);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Imported Strava activity {ActivityId} as log {LogId}", activity.Id, log.Id);
        return true;
    }
}
