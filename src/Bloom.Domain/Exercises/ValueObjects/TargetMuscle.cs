using Bloom.Domain.Shared;

namespace Bloom.Domain.Exercises.ValueObjects;

public record TargetMuscle : ValueObject
{
    public string Value { get; }

    private TargetMuscle() { }

    private TargetMuscle(string value)
    {
        Value = value;
    }

    public static TargetMuscle Create(string muscleGroup)
    {
        var muscle = new TargetMuscle(muscleGroup);
        muscle.Validate();
        return muscle;
    }

    private void Validate()
    {
        Asserts.EnsureNotEmpty(Value);
    }
}
