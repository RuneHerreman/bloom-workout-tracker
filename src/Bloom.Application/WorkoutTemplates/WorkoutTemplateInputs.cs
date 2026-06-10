using Bloom.Domain.Exercises;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.Shared;
using Bloom.Domain.WorkoutTemplates;
using Bloom.Domain.WorkoutTemplates.ValueObjects;

namespace Bloom.Application.WorkoutTemplates;

public sealed record TemplateExerciseInput(
    Guid ExerciseId,
    int Order,
    List<PlannedSetInput> Sets,
    string? Note = null,
    List<string>? Gear = null
);

public sealed record PlannedSetInput(
    string Type,
    int Order,
    int? Reps,
    TimeSpan? Duration,
    decimal? Distance,
    string? DistanceUnit
);

internal static class TemplateExerciseInputExtensions
{
    internal static TemplateExercise ToTemplateExercise(this TemplateExerciseInput input)
    {
        var sets = input.Sets.Select(s => s.ToPlannedSet()).ToList();
        return TemplateExercise.Create(
            EntityId.New<ExerciseId>(input.ExerciseId),
            input.Order,
            sets,
            note: input.Note,
            gear: input.Gear
        );
    }

    internal static PlannedSet ToPlannedSet(this PlannedSetInput input)
    {
        var type = Enum.Parse<ExerciseType>(input.Type, ignoreCase: true);

        return type switch
        {
            ExerciseType.Cardio => PlannedSet.CreateCardio(
                input.Order,
                input.Duration ?? throw new ArgumentException("Duration required for Cardio set"),
                input.Distance ?? throw new ArgumentException("Distance required for Cardio set"),
                Enum.Parse<PlannedDistanceUnit>(input.DistanceUnit ?? throw new ArgumentException("DistanceUnit required for Cardio set"), ignoreCase: true)
            ),
            ExerciseType.Strength or ExerciseType.Plyometric => PlannedSet.CreateStrengthLike(
                type,
                input.Order,
                input.Reps ?? throw new ArgumentException("Reps required for Strength/Plyometric set")
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(input.Type), input.Type, "Unsupported exercise type")
        };
    }
}