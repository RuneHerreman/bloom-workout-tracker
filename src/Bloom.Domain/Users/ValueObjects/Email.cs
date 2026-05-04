using Bloom.Domain.Shared;

namespace Bloom.Domain.Users.ValueObjects;

public record Email: ValueObject
{
    public string Value { get; }

    private Email() { }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        var email = new Email(value.Trim().ToLowerInvariant());
        email.Validate();
        return email;
    }

    private void Validate()
    {
        Asserts.EnsureNotEmpty(Value);

        if (!Value.Contains('@'))
            throw new ArgumentException("Email is not valid.");
    }
}