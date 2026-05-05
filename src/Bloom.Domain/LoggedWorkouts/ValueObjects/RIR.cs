using Bloom.Domain.Shared;

namespace Bloom.Domain.LoggedWorkouts.ValueObjects;

public record RIR: ValueObject
{
    public int Value { get; init; }

    private RIR() { }

    private RIR(int value)
    {
        Value = value;
    }

    public static RIR Create(int value)
    {
        var rir = new RIR(value);
        rir.Validate();
        return rir;
    }

    private void Validate()
    {
        Asserts.EnsureNotNegative(Value);
        Asserts.EnsureLessThan(Value, 11);
    }
}