using Bloom.Domain.Shared;

namespace Bloom.Domain.WorkoutTemplates.ValueObjects;

public record PlannedDuration: ValueObject
{
    public TimeSpan Value { get; }

    private PlannedDuration() { }

    private PlannedDuration(TimeSpan value)
    {
        Value = value;
    }

    public static PlannedDuration Create(TimeSpan value)
    {
        var duration = new PlannedDuration(value);
        duration.Validate();
        return duration;
    }

    private void Validate()
    {
        Asserts.EnsureGreaterThan(Value, TimeSpan.Zero);
    }
}