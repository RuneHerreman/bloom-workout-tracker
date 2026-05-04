using Bloom.Domain.Shared;

namespace Bloom.Domain.Users.ValueObjects;

public record Username: ValueObject
{
    public string Value { get; }

    private Username() { }

    private Username(string value)
    {
        Value = value;
    }

    public static Username Create(string value)
    {
        var username = new Username(value.Trim());
        username.Validate();
        return username;
    }

    private void Validate()
    {
        Asserts.EnsureNotEmpty(Value);
        Asserts.EnsureLessThan(Value.Length, 129); // max 32 chars
        Asserts.EnsureGreaterThan(Value.Length, 2); // min 3 chars
    }
}