using Bloom.Domain.Users.ValueObjects;

namespace UnitTests.Domain.Users.ValueObjects;

public sealed class HashedPasswordTests
{
    [Fact]
    public void Create_WithValidValue_ShouldStoreValue()
    {
        HashedPassword password = HashedPassword.Create("hash-value");

        Assert.Equal("hash-value", password.Value);
    }

    [Fact]
    public void Create_WithEmpty_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => HashedPassword.Create(""));
    }
}
