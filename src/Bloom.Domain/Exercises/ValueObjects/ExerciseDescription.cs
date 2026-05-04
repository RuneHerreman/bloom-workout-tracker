using Bloom.Domain.LoggedWorkouts.ValueObjects;
using Bloom.Domain.Shared;

namespace Bloom.Domain.Exercises.ValueObjects;

public record ExerciseDescription: ValueObject
{
    public string Value { get; }
    
    private ExerciseDescription() {}

    private ExerciseDescription(string description)
    {
        Value = description;
    }
    
    public static ExerciseDescription Create(string description)
    {
        var exerciseDescription = new ExerciseDescription(description);
        
        exerciseDescription.Validate();
        
        return exerciseDescription;
    }

    private void Validate()
    {
        Asserts.EnsureNotEmpty(Value);
    }
};