using Bloom.Domain.Shared;

namespace Bloom.Domain.WorkoutTemplates.ValueObjects;

public record WorkoutTemplateName: ValueObject
{
    public string Value { get; }

    private WorkoutTemplateName() { }

    private WorkoutTemplateName(string value)
    {
        Value = value;
    }

    public static WorkoutTemplateName Create(string value)
    {
        var name = new WorkoutTemplateName(value.Trim());
        name.Validate();
        return name;
    }

    private void Validate()
    {
        Asserts.EnsureNotEmpty(Value);
        Asserts.EnsureLessThan(Value.Length, 101); // max 100 chars
    }
}