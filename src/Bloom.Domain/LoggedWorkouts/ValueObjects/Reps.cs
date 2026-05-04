using Bloom.Domain.Shared;

namespace Bloom.Domain.LoggedWorkouts.ValueObjects;

public record Reps: ValueObject
{
    public int Value { get; }

    private Reps() { }

    private Reps(int value)
    {
        Value = value;
    }

    public static Reps Create(int value)
    {
        var reps = new Reps(value);
        reps.Validate();
        return reps;
    }

    private void Validate()
    {
        Asserts.EnsureGreaterThan(Value, 0);
    }
}