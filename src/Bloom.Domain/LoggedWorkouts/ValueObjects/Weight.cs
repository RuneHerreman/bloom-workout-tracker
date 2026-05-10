using Bloom.Domain.Shared;

namespace Bloom.Domain.LoggedWorkouts.ValueObjects;

public record Weight: ValueObject
{
    public decimal Value { get; init; }
    public WeightUnit Unit { get; init; }

    private Weight() { }

    private Weight(decimal value, WeightUnit unit)
    {
        Value = value;
        Unit = unit;
    }

    public static Weight Create(decimal value, WeightUnit unit)
    {
        var weight = new Weight(decimal.Round(value, 2), unit);
        weight.Validate();
        return weight;
    }

    private void Validate()
    {
        Asserts.EnsureNotNegative(Value);
    }
}

public enum WeightUnit
{
    Kg,
    Lbs
}