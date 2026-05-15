using Bloom.Domain.Shared;

namespace Bloom.Domain.WorkoutTemplates.ValueObjects;

public record PlannedDistance: ValueObject
{
    public decimal Value { get; }
    public PlannedDistanceUnit Unit { get; }

    private PlannedDistance() { }

    private PlannedDistance(decimal value, PlannedDistanceUnit unit)
    {
        Value = value;
        Unit = unit;
    }

    public static PlannedDistance Create(decimal value, PlannedDistanceUnit unit)
    {
        var distance = new PlannedDistance(decimal.Round(value, 3), unit);
        distance.Validate();
        return distance;
    }

    private void Validate()
    {
        Asserts.EnsureGreaterThan(Value, 0m);
    }
}

public enum PlannedDistanceUnit
{
    Km,
    Miles
}