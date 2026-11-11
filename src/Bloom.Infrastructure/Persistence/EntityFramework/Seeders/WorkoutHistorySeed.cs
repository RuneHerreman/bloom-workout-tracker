using System.Text.Json;
using Bloom.Domain.Exercises;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.LoggedWorkouts.Enums;
using Bloom.Domain.LoggedWorkouts.ValueObjects;
using Bloom.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Seeders;

/// <summary>
/// Real training history (Sep 2024 onwards) for the development user, read from
/// <c>Data/workout-history.json</c>. That file is generated from the "Gym Progress"
/// spreadsheet by <c>tools/gym-history/import_gym_history.py</c>; exercises are
/// referenced by catalog name and resolved here.
/// </summary>
internal static class WorkoutHistorySeed
{
    private const string ResourceName = "Bloom.Infrastructure.Persistence.EntityFramework.Seeders.Data.workout-history.json";

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private sealed record HistoryWorkout(string Date, string Name, string? Note, List<HistoryExercise> Exercises);

    private sealed record HistoryExercise(string Exercise, string? Note, List<HistorySet>? Sets, HistoryCardio? Cardio);

    private sealed record HistorySet(int Reps, decimal Weight, string? Marker);

    private sealed record HistoryCardio(double Minutes, decimal Km);

    public static List<LoggedWorkout> Build(UserId userId, IReadOnlyCollection<Exercise> catalog, ILogger logger)
    {
        using var stream = typeof(WorkoutHistorySeed).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{ResourceName}' not found.");
        var history = JsonSerializer.Deserialize<List<HistoryWorkout>>(stream, JsonOptions) ?? [];

        var byName = catalog
            .Where(e => !e.IsCustom)
            .GroupBy(e => e.Name.Value, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

        var missing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var workouts = new List<LoggedWorkout>();

        foreach (var w in history)
        {
            var exercises = new List<LoggedExercise>();
            foreach (var ex in w.Exercises)
            {
                if (!byName.TryGetValue(ex.Exercise, out var exerciseId))
                {
                    missing.Add(ex.Exercise);
                    continue;
                }

                var sets = ex.Cardio is { } c
                    ? [LoggedSet.CreateCardio(1, TimeSpan.FromMinutes(c.Minutes), c.Km, DistanceUnit.Km)]
                    : (ex.Sets ?? []).Select((s, i) => LoggedSet.CreateStrength(
                        i + 1, s.Reps, s.Weight, WeightUnit.Kg, rir: null,
                        marker: s.Marker is null ? null : Enum.Parse<SetMarker>(s.Marker))).ToList();
                if (sets.Count == 0) continue;

                exercises.Add(LoggedExercise.Create(exerciseId, exercises.Count + 1, sets, note: ex.Note));
            }

            if (exercises.Count == 0) continue;

            // Logged at midday UTC so the local calendar day is right in any European time zone.
            var date = DateOnly.Parse(w.Date);
            workouts.Add(LoggedWorkout.Create(
                userId, w.Name, exercises, w.Note,
                loggedAt: date.ToDateTime(new TimeOnly(12, 0), DateTimeKind.Utc)));
        }

        if (missing.Count > 0)
            logger.LogWarning("Workout history seed: no catalog exercise named {Names}; those entries were skipped", string.Join(", ", missing));

        return workouts;
    }
}
