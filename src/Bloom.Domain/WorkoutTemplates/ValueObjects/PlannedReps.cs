using Bloom.Domain.Shared;

namespace Bloom.Domain.WorkoutTemplates.ValueObjects;

public record PlannedReps: ValueObject
{
    public int Value { get; }

    private PlannedReps() { }

    private PlannedReps(int value)
    {
        Value = value;
    }

    public static PlannedReps Create(int value)
    {
        var reps = new PlannedReps(value);
        reps.Validate();
        return reps;
    }

    private void Validate()
    {
        Asserts.EnsureGreaterThan(Value, 0);
    }
}