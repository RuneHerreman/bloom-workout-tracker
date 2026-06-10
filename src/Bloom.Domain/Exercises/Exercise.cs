using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.Exercises.ValueObjects;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;

namespace Bloom.Domain.Exercises;

public readonly record struct ExerciseId(Guid Value) : IEntityId;

public class Exercise : AggregateRoot<ExerciseId>
{
    private readonly List<TargetMuscle> _targetMuscles = [];

    public ExerciseName Name { get; private set; }
    public ExerciseDescription Description { get; private set; }
    public ExerciseType Type { get; private set; }
    public UserId? OwnerUserId { get; private set; }
    public IReadOnlyList<TargetMuscle> TargetMuscles => _targetMuscles.AsReadOnly();

    public bool IsCustom => OwnerUserId is not null;

    private Exercise() { }

    private Exercise(
        ExerciseId id,
        ExerciseName name,
        ExerciseDescription description,
        ExerciseType type,
        List<TargetMuscle> targetMuscles,
        UserId? ownerUserId
    ) : base(id)
    {
        Name = name;
        Description = description;
        Type = type;
        _targetMuscles = targetMuscles;
        OwnerUserId = ownerUserId;
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
            muscleGroups.Select(TargetMuscle.Create).ToList(),
            ownerUserId: null
        );

        exercise.ValidateState();

        return exercise;
    }

    public static Exercise CreateCustom(
        UserId ownerUserId,
        string name,
        string description,
        ExerciseType type,
        IEnumerable<string> muscleGroups,
        ExerciseId? id = null
    )
    {
        Asserts.EnsureNotEmpty(ownerUserId);

        var exercise = new Exercise(
            id ?? EntityId.New<ExerciseId>(),
            ExerciseName.Create(name),
            ExerciseDescription.Create(description),
            type,
            muscleGroups.Select(TargetMuscle.Create).ToList(),
            ownerUserId
        );

        exercise.ValidateState();

        return exercise;
    }

    public void Update(
        string name,
        string description,
        ExerciseType type,
        IEnumerable<string> muscleGroups
    )
    {
        Name = ExerciseName.Create(name);
        Description = ExerciseDescription.Create(description);
        Type = type;
        _targetMuscles.Clear();
        _targetMuscles.AddRange(muscleGroups.Select(TargetMuscle.Create));

        ValidateState();
    }

    public override void ValidateState() { }
}
