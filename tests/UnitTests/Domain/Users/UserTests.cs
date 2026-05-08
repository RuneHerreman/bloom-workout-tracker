using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Domain.Users.DomainEvents;

namespace UnitTests.Domain.Users;

public sealed class UserTests
{
    [Fact]
    public void Create_WithValidInput_ShouldInitializeAndRaiseEvent()
    {
        User user = User.Create("user@example.com", "alice", "hashed-password", 72.5m, 180, 4);

        Assert.Equal("user@example.com", user.Email.Value);
        Assert.Equal("alice", user.Username.Value);
        Assert.Equal("hashed-password", user.HashedPassword.Value);
        Assert.Equal(72.5m, user.Weight);
        Assert.Equal(180, user.Height);
        Assert.Equal(4, user.ActiveDays);
        Assert.Single(user.DomainEvents);
        Assert.IsType<UserRegistered>(user.DomainEvents.First());
    }

    [Fact]
    public void Create_WithProvidedId_ShouldUseId()
    {
        UserId id = EntityId.New<UserId>();

        User user = User.Create("user@example.com", "alice", "hashed-password", 72.5m, 180, 4, id);

        Assert.Equal(id, user.Id);
    }

    [Fact]
    public void ChangePassword_ShouldReplaceHashedPassword()
    {
        User user = User.Create("user@example.com", "alice", "old-hash", 72.5m, 180, 4);

        user.ChangePassword("new-hash");

        Assert.Equal("new-hash", user.HashedPassword.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveWeight_ShouldThrow(double weight)
    {
        Assert.Throws<ArgumentException>(
            () => User.Create("user@example.com", "alice", "hashed-password", (decimal)weight, 180, 4));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveHeight_ShouldThrow(int height)
    {
        Assert.Throws<ArgumentException>(
            () => User.Create("user@example.com", "alice", "hashed-password", 72.5m, height, 4));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(8)]
    public void Create_WithActiveDaysOutOfRange_ShouldThrow(int activeDays)
    {
        Assert.Throws<ArgumentException>(
            () => User.Create("user@example.com", "alice", "hashed-password", 72.5m, 180, activeDays));
    }
}
