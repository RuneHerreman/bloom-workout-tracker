using Bloom.Domain.Users.ValueObjects;

namespace UnitTests.Domain.Users.ValueObjects;

public sealed class UsernameTests
{
    [Fact]
    public void Create_WithValidValue_ShouldTrimAndStore()
    {
        Username username = Username.Create("  alice  ");

        Assert.Equal("alice", username.Value);
    }

    [Fact]
    public void Create_TooShort_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => Username.Create("ab"));
    }

    [Fact]
    public void Create_TooLong_ShouldThrow()
    {
        string tooLong = new('a', 129);

        Assert.Throws<ArgumentException>(() => Username.Create(tooLong));
    }

    [Fact]
    public void Create_WithEmpty_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => Username.Create(""));
    }
}
