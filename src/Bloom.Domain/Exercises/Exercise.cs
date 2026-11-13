using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.Exercises.ValueObjects;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;

namespace Bloom.Domain.Exercises;

public readonly record struct ExerciseId(Guid Value) : IEntityId;

public class Exercise : AggregateRoot<ExerciseId>
{
    private readonly List<TargetMuscle> _targetMuscles = [];
    private readonly List<TargetMuscle> _secondaryMuscles = [];

    public ExerciseName Name { get; private set; }
    public ExerciseDescription Description { get; private set; }
    public ExerciseType Type { get; private set; }
    public UserId? OwnerUserId { get; private set; }

    /// <summary>Primary movers: each working set counts fully for these.</summary>
    public IReadOnlyList<TargetMuscle> TargetMuscles => _targetMuscles.AsReadOnly();

    /// <summary>Assisting muscles: each working set counts as half a set for these.</summary>
    public IReadOnlyList<TargetMuscle> SecondaryMuscles => _secondaryMuscles.AsReadOnly();

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

    public static Exercise Create(
        string name,
        string description,
        ExerciseType type,
        IEnumerable<string> muscleGroups,
        ExerciseId? id = null,
        IEnumerable<string>? secondaryMuscles = null
    )
    {
        var exercise = new Exercise(
            id ?? EntityId.New<ExerciseId>(),
            ExerciseName.Create(name),
            ExerciseDescription.Create(description),
            type,
            ownerUserId: null
        );
        exercise.SetMuscles(muscleGroups, secondaryMuscles ?? []);

        exercise.ValidateState();

        return exercise;
    }

    public static Exercise CreateCustom(
        UserId ownerUserId,
        string name,
        string description,
        ExerciseType type,
        IEnumerable<string> muscleGroups,
        ExerciseId? id = null,
        IEnumerable<string>? secondaryMuscles = null
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
        exercise.SetMuscles(muscleGroups, secondaryMuscles ?? []);

        exercise.ValidateState();

        return exercise;
    }

    public void Update(
        string name,
        string description,
        ExerciseType type,
        IEnumerable<string> muscleGroups,
        IEnumerable<string>? secondaryMuscles = null
    )
    {
        Name = ExerciseName.Create(name);
        Description = ExerciseDescription.Create(description);
        Type = type;
        SetMuscles(muscleGroups, secondaryMuscles ?? []);

        ValidateState();
    }

    /// <summary>
    /// Replaces primary and secondary muscles. Duplicates are dropped, and a muscle listed
    /// as primary is never also secondary.
    /// </summary>
    public void SetMuscles(IEnumerable<string> primary, IEnumerable<string> secondary)
    {
        var primaryList = Clean(primary);
        var secondaryList = Clean(secondary)
            .Where(s => !primaryList.Contains(s, StringComparer.OrdinalIgnoreCase))
            .ToList();

        _targetMuscles.Clear();
        _targetMuscles.AddRange(primaryList.Select(TargetMuscle.Create));
        _secondaryMuscles.Clear();
        _secondaryMuscles.AddRange(secondaryList.Select(TargetMuscle.Create));
    }

    /// <summary>True when primary and secondary muscles already equal these lists, in order.</summary>
    public bool HasMuscles(IEnumerable<string> primary, IEnumerable<string> secondary) =>
        _targetMuscles.Select(m => m.Value).SequenceEqual(primary) &&
        _secondaryMuscles.Select(m => m.Value).SequenceEqual(secondary);

    private static List<string> Clean(IEnumerable<string> muscles) =>
        muscles.Select(m => m.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

    public override void ValidateState() { }
}
