using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.Exercises.ValueObjects;
using Bloom.Domain.Shared;

namespace Bloom.Domain.Exercises;

public readonly record struct ExerciseId(Guid Value) : IEntityId;

public class Exercise : AggregateRoot<ExerciseId>
{
    private readonly List<TargetMuscle> _targetMuscles = [];

    public ExerciseName Name { get; private set; }
    public ExerciseDescription Description { get; private set; }
    public ExerciseType Type { get; private set; }
    public IReadOnlyList<TargetMuscle> TargetMuscles => _targetMuscles.AsReadOnly();

    private Exercise() { }

    private Exercise(
        ExerciseId id,
        ExerciseName name,
        ExerciseDescription description,
        ExerciseType type,
        List<TargetMuscle> targetMuscles
    ) : base(id)
    {
        Name = name;
        Description = description;
        Type = type;
        _targetMuscles = targetMuscles;
    }

    public static Exercise Create(
        string name,
        string description,
        ExerciseType type,
        IEnumerable<string> muscleGroups,
        ExerciseId? id = null
    )
    {
        var exercise = new Exercise(
            id ?? EntityId.New<ExerciseId>(),
            ExerciseName.Create(name),
            ExerciseDescription.Create(description),
            type,
            muscleGroups.Select(TargetMuscle.Create).ToList()
        );

        exercise.ValidateState();

        return exercise;
    }

    public override void ValidateState() { }
}
