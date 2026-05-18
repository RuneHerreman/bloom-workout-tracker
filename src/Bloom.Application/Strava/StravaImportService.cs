using Bloom.Application.Contracts.Ports;
using Bloom.Application.LoggedWorkouts;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.Strava;
using Bloom.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Strava;

public record ImportLoopResult(int Imported, DateTime? LatestActivityAt);

public class StravaImportService(
    IStravaClient stravaClient,
    IStravaActivityMapper mapper,
    ILogger<StravaImportService> logger
)
{
    public async Task<string> EnsureValidToken(
        StravaConnection conn,
        IStravaConnectionRepository connRepo,
        IUnitOfWork uow,
        CancellationToken ct)
    {
        if (!conn.IsExpired()) return conn.AccessToken;

        logger.LogInformation("Refreshing Strava token for athlete {AthleteId}", conn.StravaAthleteId);
        var refreshed = await stravaClient.TryRefreshToken(conn.RefreshToken, ct)
            ?? throw new InvalidOperationException("Failed to refresh Strava token");

        conn.UpdateTokens(refreshed.AccessToken, refreshed.RefreshToken, refreshed.ExpiresAt);
        await connRepo.Save(conn);
        await uow.Do(ct);
        return conn.AccessToken;
    }

    public async Task<ImportLoopResult> ImportLoop(
        string token,
        long? afterUnix,
        UserId userId,
        IUnitOfWork uow,
        CancellationToken ct)
    {
        var logRepo = uow.Repo<ILoggedWorkoutRepository>();
        var imported = 0;
        var page = 1;
        DateTime? latestActivityAt = null;

        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var activities = await stravaClient.GetActivities(token, page, 100, afterUnix, ct);
            if (activities.Count == 0) break;

            var pageExternalIds = activities.Select(a => StravaActivityMapper.ExternalId(a.Id)).ToList();
            var existingIds = await logRepo.GetExistingExternalIds(userId, pageExternalIds, ct);

            var pageMax = activities.Max(a => a.StartDate);
            if (latestActivityAt is null || pageMax > latestActivityAt.Value)
                latestActivityAt = pageMax;

            var logsThisPage = new List<LoggedWorkout>();
            foreach (var activity in activities)
            {
                ct.ThrowIfCancellationRequested();

                var externalId = StravaActivityMapper.ExternalId(activity.Id);
                if (existingIds.Contains(externalId)) continue;

                StravaActivityStreamsResult? streams = null;
                if (activity.Distance > 0)
                {
                    try { streams = await stravaClient.GetActivityStreams(token, activity.Id, ct); }
                    catch (Exception ex) { logger.LogWarning(ex, "Could not fetch streams for activity {ActivityId}", activity.Id); }
                }

                var workoutInput = await mapper.Map(activity, streams, ct);
                if (workoutInput is null) continue;

                List<LoggedExercise> exercises;
                try
                {
                    exercises = workoutInput.Exercises.Select(e => e.ToLoggedExercise()).ToList();
                }
                catch (ArgumentException ex)
                {
                    logger.LogWarning(ex, "Skipping Strava activity {ActivityId} due to invalid workout input.", activity.Id);
                    continue;
                }

                var log = LoggedWorkout.Create(userId, workoutInput.Name, exercises, workoutInput.Note, workoutInput.LoggedAt, workoutInput.ExternalId);
                logsThisPage.Add(log);
                imported++;
            }

            foreach (var log in logsThisPage)
                await logRepo.Save(log);

            if (logsThisPage.Count > 0)
                await uow.Do(ct);

            if (activities.Count < 100) break;
            page++;
        }

        return new ImportLoopResult(imported, latestActivityAt);
    }
}
