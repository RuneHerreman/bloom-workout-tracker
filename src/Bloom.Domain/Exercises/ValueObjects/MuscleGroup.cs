using Bloom.Domain.Shared;

namespace Bloom.Domain.Exercises.ValueObjects;

public record MuscleGroup: ValueObject
{
    public string Value { get; }
    
    private MuscleGroup() {}
    
    private MuscleGroup(string value)
    {
        Value = value;
    }

    public static MuscleGroup Create(string muscleGroup)
    {
        var muscle = new MuscleGroup(muscleGroup);

        muscle.Validate();

        return muscle;
    }

    private void Validate()
    {
        Asserts.EnsureNotEmpty(Value);
    }
}