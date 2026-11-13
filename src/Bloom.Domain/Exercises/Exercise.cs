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

    /// <summary>
    /// The muscles the exercise uses, from <see cref="MuscleTree"/>: an area on its own
    /// ("Back") or muscles in it ("Lats"). Every one gets a full working set.
    /// </summary>
    public IReadOnlyList<TargetMuscle> TargetMuscles => _targetMuscles.AsReadOnly();

    public bool IsCustom => OwnerUserId is not null;

    private Exercise() { }

    private Exercise(
        ExerciseId id,
        ExerciseName name,
        ExerciseDescription description,
        ExerciseType type,
        UserId? ownerUserId
    ) : base(id)
    {
        Name = name;
        Description = description;
        Type = type;
        OwnerUserId = ownerUserId;
    }

    /// <summary>A catalog exercise: every muscle must come from the tree.</summary>
    public static Exercise Create(
        string name,
        string description,
        ExerciseType type,
        IEnumerable<string> muscleGroups,
        ExerciseId? id = null
    )
    {
        var muscles = muscleGroups.ToList();
        var unknown = muscles.Where(m => !MuscleTree.IsKnown(m)).ToList();
        if (unknown.Count > 0)
            throw new ArgumentException($"Unknown muscles for catalog exercise '{name}': {string.Join(", ", unknown)}");

        var exercise = new Exercise(
            id ?? EntityId.New<ExerciseId>(),
            ExerciseName.Create(name),
            ExerciseDescription.Create(description),
            type,
            ownerUserId: null
        );
        exercise.SetMuscles(muscles);

        exercise.ValidateState();

        return exercise;
    }

    /// <summary>A user's own exercise: names outside the tree are allowed and stand as their own area.</summary>
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
            ownerUserId
        );
        exercise.SetMuscles(muscleGroups);

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
        SetMuscles(muscleGroups);

        ValidateState();
    }

    /// <summary>Replaces the muscles, tidied by <see cref="MuscleTree.Normalize"/>.</summary>
    public void SetMuscles(IEnumerable<string> muscles)
    {
        _targetMuscles.Clear();
        _targetMuscles.AddRange(MuscleTree.Normalize(muscles).Select(TargetMuscle.Create));
    }

    /// <summary>True when the muscles already equal this list (after tidying), in order.</summary>
    public bool HasMuscles(IEnumerable<string> muscles) =>
        _targetMuscles.Select(m => m.Value).SequenceEqual(MuscleTree.Normalize(muscles));

    public override void ValidateState() { }
}
