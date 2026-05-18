using System.IO.Compression;
using System.Security;
using System.Text;
using Bloom.Application.Contracts.Ports;
using Bloom.Application.LoggedWorkouts;
using Bloom.Domain.Exercises;
using Bloom.Domain.Exercises.Enums;
using Microsoft.Extensions.Logging;

namespace Bloom.Application.Strava;

public class StravaActivityMapper(IExerciseRepository exerciseRepository, ILogger<StravaActivityMapper> logger)
    : IStravaActivityMapper
{
    public static string ExternalId(long activityId) => $"strava:{activityId}";
    private static readonly Dictionary<string, string> SportTypeToExerciseName = new(StringComparer.OrdinalIgnoreCase)
    {
        // Running
        ["Run"] = "Outdoor Run",
        ["TrailRun"] = "Trail Run",
        ["VirtualRun"] = "Treadmill Run",
        // Cycling
        ["Ride"] = "Road Cycling",
        ["VirtualRide"] = "Indoor Cycling",
        ["MountainBikeRide"] = "Mountain Biking",
        ["EMountainBikeRide"] = "Mountain Biking",
        ["GravelRide"] = "Gravel Cycling",
        ["EBikeRide"] = "E-Bike Ride",
        ["Velomobile"] = "Road Cycling",
        ["Handcycle"] = "Hand Cycling",
        // Swimming & water
        ["Swim"] = "Swimming",
        ["OpenWaterSwim"] = "Open Water Swimming",
        ["Rowing"] = "Rowing (On Water)",
        ["VirtualRow"] = "Rowing Machine",
        ["Kayaking"] = "Kayaking",
        ["Canoeing"] = "Canoeing",
        ["StandUpPaddling"] = "Stand Up Paddleboarding",
        ["Surfing"] = "Surfing",
        ["Surf"] = "Surfing",
        ["Kitesurf"] = "Surfing",
        ["Windsurf"] = "Surfing",
        ["Sail"] = "Sailing",
        // Walking & hiking
        ["Walk"] = "Walking",
        ["Hike"] = "Hiking",
        ["Snowshoe"] = "Snowshoeing",
        // Snow & ice
        ["NordicSki"] = "Cross Country Skiing",
        ["AlpineSki"] = "Skiing",
        ["BackcountrySki"] = "Ski Touring",
        ["RollerSki"] = "Cross Country Skiing",
        ["Snowboard"] = "Outdoor Snowboarding",
        ["IceSkate"] = "Ice Skating",
        ["InlineSkate"] = "Inline Skating",
        ["Skateboard"] = "Skateboarding",
        // Team & racket sports
        ["Soccer"] = "Soccer",
        ["Football"] = "Soccer",
        ["Basketball"] = "Basketball",
        ["Handball"] = "Handball",
        ["Rugby"] = "Rugby",
        ["Golf"] = "Golf",
        ["Tennis"] = "Tennis",
        ["Squash"] = "Squash",
        ["Badminton"] = "Badminton",
        ["TableTennis"] = "Table Tennis",
        ["Volleyball"] = "Volleyball",
        ["Lacrosse"] = "Lacrosse",
        ["Cricket"] = "Cricket",
        ["Pickleball"] = "Pickleball",
        ["Padel"] = "Padel",
        // Combat
        ["Boxing"] = "Boxing",
        ["MartialArts"] = "Martial Arts",
        ["Wrestling"] = "Wrestling",
        // Climbing
        ["RockClimbing"] = "Climbing",
        // Fitness & gym
        ["Yoga"] = "Yoga",
        ["Pilates"] = "Pilates",
        ["CrossfitWorkout"] = "CrossFit WOD",
        ["Crossfit"] = "CrossFit WOD",
        ["HighIntensityIntervalTraining"] = "HIIT",
        ["Elliptical"] = "Elliptical",
        ["StairStepper"] = "Stair Climber",
        ["Stretching"] = "Stretching",
        ["Dance"] = "Dance",
        ["WeightTraining"] = "Weight Training",
        ["Workout"] = "Weight Training",
        // Accessible sports
        ["Wheelchair"] = "Wheelchair Racing",
    };

    public async Task<CreateLoggedWorkoutInput?> Map(StravaActivityResult activity, StravaActivityStreamsResult? streams, CancellationToken ct = default)
    {
        if (!SportTypeToExerciseName.TryGetValue(activity.SportType, out var exerciseName))
        {
            logger.LogWarning("Unknown Strava sport type '{SportType}' for activity {ActivityId} — falling back to 'Outdoor Run'",
                activity.SportType, activity.Id);
            exerciseName = "Outdoor Run";
        }

        var exercise = await exerciseRepository.ByName(exerciseName, ct);

        if (!exercise.HasValue)
        {
            logger.LogWarning("No exercise found for Strava sport type '{SportType}' (looked up '{ExerciseName}'). Skipping activity {ActivityId}.",
                activity.SportType, exerciseName, activity.Id);
            return null;
        }

        LoggedSetInput set;
        string? gpxData = null;

        if (exercise.Value.Type == ExerciseType.Cardio)
        {
            var durationSeconds = activity.MovingTime > 0 ? activity.MovingTime : activity.ElapsedTime;
            if (durationSeconds <= 0)
            {
                logger.LogWarning("Strava activity {ActivityId} has non-positive duration ({DurationSeconds}s). Skipping.", activity.Id, durationSeconds);
                return null;
            }

            var distanceKm = Math.Round((decimal)activity.Distance / 1000m, 2);
            if (distanceKm <= 0m)
            {
                logger.LogWarning("Strava activity {ActivityId} has non-positive distance ({DistanceKm}km). Clamping to 0.01km.", activity.Id, distanceKm);
                distanceKm = 0.001m;
            }

            set = new LoggedSetInput("Cardio", 0, TimeSpan.FromSeconds(durationSeconds), distanceKm, "Km", null, null, null, null);

            if (streams is not null)
                gpxData = await BuildGpx(activity, streams);
        }
        else
        {
            var setType = exercise.Value.Type == ExerciseType.Plyometric ? "Plyometric" : "Strength";
            set = new LoggedSetInput(setType, 0, null, null, null, 1, 0m, "Kg", null);
        }

        var note = exercise.Value.Type != ExerciseType.Cardio
            ? $"Strava import — {activity.ElapsedTime / 60} min {activity.SportType}"
            : null;

        var exerciseInput = new LoggedExerciseInput(exercise.Value.Id.Value, 0, [set], gpxData);

        return new CreateLoggedWorkoutInput(
            activity.Name,
            [exerciseInput],
            note,
            activity.StartDate,
            ExternalId(activity.Id)
        );
    }

    private static async Task<string?> BuildGpx(StravaActivityResult activity, StravaActivityStreamsResult streams)
    {
        if (streams.LatLng.Count == 0 || streams.Time.Count == 0)
            return null;

        var sb = new StringBuilder();
        sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.Append("<gpx version=\"1.1\" creator=\"Bloom\" xmlns=\"http://www.topografix.com/GPX/1/1\">");
        sb.Append($"<trk><name>{SecurityElement.Escape(activity.Name)}</name><trkseg>");

        var count = Math.Min(streams.LatLng.Count, streams.Time.Count);
        for (var i = 0; i < count; i++)
        {
            var point = streams.LatLng[i];
            if (point.Count < 2) continue;
            var t = activity.StartDate.AddSeconds(streams.Time[i]);
            var alt = streams.Altitude is not null && i < streams.Altitude.Count ? streams.Altitude[i] : (double?)null;

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
