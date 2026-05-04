using Bloom.Domain.Shared;

namespace Bloom.Domain.Exercises.ValueObjects;

public record ExerciseName: ValueObject
{
    public string Value { get; }
    
    private ExerciseName() {}
    
    private ExerciseName(string name)
    {
        Value = name;
    }

    public static ExerciseName Create(string name)
    {
        var exerciseName = new ExerciseName(name);

        exerciseName.Validate();

        return exerciseName;
    }

    private void Validate()
    {
        Asserts.EnsureNotEmpty(Value);
    }
}