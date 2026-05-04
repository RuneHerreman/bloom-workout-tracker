using Bloom.Domain.Shared;

namespace Bloom.Domain.Users.ValueObjects;

public record HashedPassword: ValueObject
{
    public string Value { get; }

    private HashedPassword() { }

    private HashedPassword(string value)
    {
        Value = value;
    }

    public static HashedPassword Create(string value)
    {
        var hashedPassword = new HashedPassword(value);
        hashedPassword.Validate();
        return hashedPassword;
    }

    private void Validate()
    {
        Asserts.EnsureNotEmpty(Value);
    }
}