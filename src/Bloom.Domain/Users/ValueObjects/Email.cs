using System.Text.RegularExpressions;
using Bloom.Domain.Shared;

namespace Bloom.Domain.Users.ValueObjects;

public record Email : ValueObject
{
    // Minimal RFC 5321 structure: local@domain.tld
    private static readonly Regex Pattern =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email() { Value = string.Empty; }

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

        if (Value.Length > 320)
            throw new ArgumentException("Email address is too long.");

        if (!Pattern.IsMatch(Value))
            throw new ArgumentException("Email address is not valid.");
    }
}
