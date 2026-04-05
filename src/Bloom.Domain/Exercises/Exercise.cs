using Bloom.Domain.Shared;

namespace Bloom.Domain.Exercises;

public readonly record struct ExerciseId(Guid Value) : IEntityId;

public enum ExerciseType
{
    Cardio,
    Strength,
    Plyometric
}

public class Exercise: AggregateRoot<ExerciseId>
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public ExerciseType Type { get; private set; }
    public string PrimaryMuscleGroup { get; private set; } = null!;

    // EF Core requires a parameterless constructor
    private Exercise() {}

    private Exercise(ExerciseId id, string name, string description, ExerciseType type, string primaryMuscleGroup) : base(id)
    {
        Name = name;
        Description = description;
        Type = type;
        PrimaryMuscleGroup = primaryMuscleGroup;
    }

    public static Exercise Create(string name, string description, ExerciseType type, string primaryMuscleGroup, ExerciseId? exerciseId = null)
    {
        Exercise exercise = new(
            exerciseId ?? EntityId.New<ExerciseId>(),
            name,
            description,
            type,
            primaryMuscleGroup
        );
        exercise.ValidateState();
        return exercise;
    }

    public void Update(string name, string description, ExerciseType type, string primaryMuscleGroup)
    {
        Name = name;
        Description = description;
        Type = type;
        PrimaryMuscleGroup = primaryMuscleGroup;
        ValidateState();
    }

    public override void ValidateState()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException("Exercise name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            throw new InvalidOperationException("Exercise description cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(PrimaryMuscleGroup))
        {
            throw new InvalidOperationException("Primary muscle group cannot be empty.");
        }
    }
}