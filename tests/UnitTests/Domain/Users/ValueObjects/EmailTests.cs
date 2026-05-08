using Bloom.Domain.Users.ValueObjects;

namespace UnitTests.Domain.Users.ValueObjects;

public sealed class EmailTests
{
    [Fact]
    public void Create_WithValidValue_ShouldNormalizeAndStore()
    {
        Email email = Email.Create("  User@Example.COM  ");

        Assert.Equal("user@example.com", email.Value);
    }

    [Fact]
    public void Create_WithEmpty_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => Email.Create(""));
    }

    [Fact]
    public void Create_WithoutAtSymbol_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => Email.Create("not-an-email"));
    }
}
