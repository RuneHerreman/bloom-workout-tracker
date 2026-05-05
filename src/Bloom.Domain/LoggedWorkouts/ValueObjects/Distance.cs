using Bloom.Domain.Shared;

namespace Bloom.Domain.LoggedWorkouts.ValueObjects;

public record Distance: ValueObject
{
    public decimal Value { get; init; }
    public DistanceUnit Unit { get; init; }

    private Distance() { }

    private Distance(decimal value, DistanceUnit unit)
    {
        Value = value;
        Unit = unit;
    }

    public static Distance Create(decimal value, DistanceUnit unit)
    {
        var distance = new Distance(decimal.Round(value, 2), unit);
        distance.Validate();
        return distance;
    }

    private void Validate()
    {
        Asserts.EnsureGreaterThan(Value, 0m);
    }
}

public enum DistanceUnit
{
    Km,
    Miles
}