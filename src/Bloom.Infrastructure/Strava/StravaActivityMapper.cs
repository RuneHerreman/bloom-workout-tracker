using System.IO.Compression;
using System.Security;
using System.Text;
using Bloom.Application.LoggedWorkouts;
using Bloom.Domain.Exercises.ValueObjects;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bloom.Infrastructure.Strava;

public class StravaActivityMapper(DomainDbContext context, ILogger<StravaActivityMapper> logger)
{
    // Maps Strava sport_type to exercise names in the seeded catalog
    private static readonly Dictionary<string, string> SportTypeToExerciseName = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Run"] = "Outdoor Run",
        ["TrailRun"] = "Trail Run",
        ["VirtualRun"] = "Treadmill Run",
        ["Ride"] = "Cycling",
        ["VirtualRide"] = "Indoor Cycling",
        ["MountainBikeRide"] = "Mountain Biking",
        ["GravelRide"] = "Cycling",
        ["EBikeRide"] = "Cycling",
        ["Swim"] = "Swimming",
        ["OpenWaterSwim"] = "Open Water Swimming",
        ["Walk"] = "Walking",
        ["Hike"] = "Hiking",
        ["NordicSki"] = "Nordic Walking",
        ["Rowing"] = "Rowing (On Water)",
        ["Kayaking"] = "Rowing (On Water)",
        ["Soccer"] = "Soccer",
        ["Football"] = "Soccer",
        ["Basketball"] = "Basketball",
        ["Handball"] = "Handball",
        ["Rugby"] = "Rugby",
        ["Golf"] = "Golf",
        ["Yoga"] = "Bench Press",         // Fallback strength exercise
        ["WeightTraining"] = "Bench Press", // Fallback strength exercise
        ["CrossfitWorkout"] = "Bench Press",
        ["Workout"] = "Bench Press",
    };

    // Strava types that have meaningful distance data (cardio)
    private static readonly HashSet<string> CardioTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Run", "TrailRun", "VirtualRun", "Ride", "VirtualRide", "MountainBikeRide",
        "GravelRide", "EBikeRide", "Swim", "OpenWaterSwim", "Walk", "Hike",
        "NordicSki", "Rowing", "Kayaking", "Soccer", "Football", "Basketball",
        "Handball", "Rugby", "Golf",
    };

    public async Task<CreateLoggedWorkoutInput?> Map(StravaActivity activity, StravaStreams? streams, CancellationToken ct = default)
    {
        var exerciseName = SportTypeToExerciseName.GetValueOrDefault(activity.SportType, "Outdoor Run");
        var name = ExerciseName.Create(exerciseName);
        var exercise = await context.Exercises
            .Where(e => e.Name == name)
            .Select(e => new { e.Id, e.Type })
            .FirstOrDefaultAsync(ct);

        if (exercise is null)
        {
            logger.LogWarning("No exercise found for Strava sport type '{SportType}' (looked up '{ExerciseName}'). Skipping activity {ActivityId}.",
                activity.SportType, exerciseName, activity.Id);
            return null;
        }

        var isCardio = activity.Distance > 0;
        var gpxData = isCardio && streams is not null ? await BuildGpx(activity, streams) : null;

        LoggedSetInput set;
        if (isCardio)
        {
            var durationSeconds = activity.MovingTime > 0 ? activity.MovingTime : activity.ElapsedTime;
            var distanceKm = Math.Max(0.001m, Math.Round((decimal)activity.Distance / 1000m, 2));
            set = new LoggedSetInput("Cardio", 0, TimeSpan.FromSeconds(durationSeconds), distanceKm, "Km", null, null, null, null);
        }
        else
        {
            set = new LoggedSetInput("Strength", 0, null, null, null, 1, 0m, "Kg", null);
        }

        var exerciseInput = new LoggedExerciseInput(exercise.Id.Value, 0, [set], gpxData);

        var note = isCardio ? null : $"Strava import — {activity.ElapsedTime / 60} min {activity.SportType}";

        return new CreateLoggedWorkoutInput(
            activity.Name,
            [exerciseInput],
            note,
            activity.StartDate,
            $"strava:{activity.Id}"
        );
    }

    private static async Task<string?> BuildGpx(StravaActivity activity, StravaStreams streams)
    {
        var latLng = streams.LatLng?.Data;
        var times = streams.Time?.Data;
        var altitudes = streams.Altitude?.Data;

        if (latLng is null || latLng.Count == 0 || times is null || times.Count == 0)
            return null;

        var sb = new StringBuilder();
        sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.Append("<gpx version=\"1.1\" creator=\"Bloom\" xmlns=\"http://www.topografix.com/GPX/1/1\">");
        sb.Append($"<trk><name>{SecurityElement.Escape(activity.Name)}</name><trkseg>");

        int count = Math.Min(latLng.Count, times.Count);
        for (int i = 0; i < count; i++)
        {
            var point = latLng[i];
            if (point.Count < 2) continue;
            var t = activity.StartDate.AddSeconds(times[i]);
            var alt = altitudes is not null && i < altitudes.Count ? altitudes[i] : (double?)null;

            sb.Append($"<trkpt lat=\"{point[0]:F6}\" lon=\"{point[1]:F6}\">");
            if (alt.HasValue) sb.Append($"<ele>{alt.Value:F1}</ele>");
            sb.Append($"<time>{t:yyyy-MM-ddTHH:mm:ssZ}</time>");
            sb.Append("</trkpt>");
        }

        sb.Append("</trkseg></trk></gpx>");
        return await CompressToBase64(sb.ToString());
    }

    private static async Task<string> CompressToBase64(string xml)
    {
        var bytes = Encoding.UTF8.GetBytes(xml);
        using var ms = new MemoryStream();
        await using (var gz = new GZipStream(ms, CompressionLevel.Optimal, leaveOpen: true))
            await gz.WriteAsync(bytes);
        return Convert.ToBase64String(ms.ToArray());
    }
}
