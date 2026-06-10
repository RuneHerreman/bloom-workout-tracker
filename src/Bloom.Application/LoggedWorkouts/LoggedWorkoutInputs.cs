using Bloom.Domain.Exercises;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.LoggedWorkouts.Enums;
using Bloom.Domain.LoggedWorkouts.ValueObjects;
using Bloom.Domain.Shared;

namespace Bloom.Application.LoggedWorkouts;

public sealed record LoggedExerciseInput(
    Guid ExerciseId,
    int Order,
    List<LoggedSetInput> Sets,
    string? GpxData = null,
    string? Note = null,
    List<string>? Gear = null
);

public sealed record LoggedSetInput(
    string Type,
    int Order,
    TimeSpan? Duration,
    decimal? Distance,
    string? DistanceUnit,
    int? Reps,
    decimal? Weight,
    string? WeightUnit,
    int? Rir,
    string? Marker = null
);

public static class LoggedExerciseInputExtensions
{
    public static LoggedExercise ToLoggedExercise(this LoggedExerciseInput input)
    {
        var sets = input.Sets.Select(s => s.ToLoggedSet()).ToList();
        return LoggedExercise.Create(
            EntityId.New<ExerciseId>(input.ExerciseId),
            input.Order,
            sets,
            input.GpxData,
            input.Note,
            input.Gear
        );
    }

    public static LoggedSet ToLoggedSet(this LoggedSetInput input)
    {
        var type = Enum.Parse<ExerciseType>(input.Type, ignoreCase: true);
        var marker = ParseMarker(input.Marker);

        return type switch
        {
            ExerciseType.Cardio => LoggedSet.CreateCardio(
                input.Order,
                input.Duration ?? throw new ArgumentException("Duration required for Cardio set"),
                input.Distance ?? throw new ArgumentException("Distance required for Cardio set"),
                Enum.Parse<DistanceUnit>(input.DistanceUnit ?? throw new ArgumentException("DistanceUnit required for Cardio set"), ignoreCase: true),
                marker
            ),
            ExerciseType.Strength => LoggedSet.CreateStrength(
                input.Order,
                input.Reps ?? throw new ArgumentException("Reps required for Strength set"),
                input.Weight ?? throw new ArgumentException("Weight required for Strength set"),
                Enum.Parse<WeightUnit>(input.WeightUnit ?? throw new ArgumentException("WeightUnit required for Strength set"), ignoreCase: true),
                input.Rir,
                marker
            ),
            ExerciseType.Plyometric => LoggedSet.CreatePlyometric(
                input.Order,
                input.Reps ?? throw new ArgumentException("Reps required for Plyometric set"),
                input.Weight ?? throw new ArgumentException("Weight required for Plyometric set"),
                Enum.Parse<WeightUnit>(input.WeightUnit ?? throw new ArgumentException("WeightUnit required for Plyometric set"), ignoreCase: true),
                input.Rir,
                marker
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(input.Type), input.Type, "Unsupported exercise type")
        };
    }

    public static SetMarker? ParseMarker(string? marker) =>
        marker?.Trim() switch
        {
            null or "" => null,
            var m when m.Equals("W", StringComparison.OrdinalIgnoreCase) => SetMarker.WarmUp,
            var m when m.Equals("D", StringComparison.OrdinalIgnoreCase) => SetMarker.DropSet,
            var m => Enum.Parse<SetMarker>(m, ignoreCase: true)
        };
}
