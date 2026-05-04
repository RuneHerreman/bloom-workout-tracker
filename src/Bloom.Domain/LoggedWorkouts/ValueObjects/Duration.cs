using Bloom.Domain.Shared;

namespace Bloom.Domain.LoggedWorkouts.ValueObjects;

public record Duration: ValueObject
{
    public TimeSpan Value { get; }

    private Duration() { }

    private Duration(TimeSpan value)
    {
        Value = value;
    }

    public static Duration Create(TimeSpan value)
    {
        var duration = new Duration(value);
        duration.Validate();
        return duration;
    }

    private void Validate()
    {
        Asserts.EnsureGreaterThan(Value, TimeSpan.Zero);
    }
}