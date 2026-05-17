using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Domain.Users.DomainEvents;

namespace UnitTests.Domain.Users;

public sealed class UserTests
{
    [Fact]
    public void Create_WithValidInput_ShouldInitializeAndRaiseEvent()
    {
        User user = User.Create("user@example.com", "alice", "hashed-password", "Alice", "Smith", 72.5m, 180, 4, new DateOnly(1990, 1, 1));

        Assert.Equal("user@example.com", user.Email.Value);
        Assert.Equal("alice", user.Username.Value);
        Assert.Equal("hashed-password", user.HashedPassword.Value);
        Assert.Equal("Alice", user.FirstName);
        Assert.Equal("Smith", user.LastName);
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

        User user = User.Create("user@example.com", "alice", "hashed-password", "Alice", "Smith", 72.5m, 180, 4, new DateOnly(1990, 1, 1), id);

        Assert.Equal(id, user.Id);
    }

    [Fact]
    public void ChangePassword_ShouldReplaceHashedPassword()
    {
        User user = User.Create("user@example.com", "alice", "old-hash", "Alice", "Smith", 72.5m, 180, 4, new DateOnly(1990, 1, 1));

        user.ChangePassword("new-hash");

        Assert.Equal("new-hash", user.HashedPassword.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyFirstName_ShouldThrow(string firstName)
    {
        Assert.Throws<ArgumentException>(
            () => User.Create("user@example.com", "alice", "hashed-password", firstName, "Smith", 72.5m, 180, 4, new DateOnly(1990, 1, 1)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyLastName_ShouldThrow(string lastName)
    {
        Assert.Throws<ArgumentException>(
            () => User.Create("user@example.com", "alice", "hashed-password", "Alice", lastName, 72.5m, 180, 4, new DateOnly(1990, 1, 1)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveWeight_ShouldThrow(double weight)
    {
        Assert.Throws<ArgumentException>(
            () => User.Create("user@example.com", "alice", "hashed-password", "Alice", "Smith", (decimal)weight, 180, 4, new DateOnly(1990, 1, 1)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveHeight_ShouldThrow(int height)
    {
        Assert.Throws<ArgumentException>(
            () => User.Create("user@example.com", "alice", "hashed-password", "Alice", "Smith", 72.5m, height, 4, new DateOnly(1990, 1, 1)));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(8)]
    public void Create_WithActiveDaysOutOfRange_ShouldThrow(int activeDays)
    {
        Assert.Throws<ArgumentException>(
            () => User.Create("user@example.com", "alice", "hashed-password", "Alice", "Smith", 72.5m, 180, activeDays, new DateOnly(1990, 1, 1)));
    }

    [Fact]
    public void UpdateInfo_WithValidInput_ShouldReplaceFieldsAndRaiseEvent()
    {
        User user = User.Create("user@example.com", "alice", "hashed-password", "Alice", "Smith", 72.5m, 180, 4, new DateOnly(1990, 1, 1));
        user.ClearDomainEvents();

        user.UpdateInfo("new@example.com", "alice2", "Alicia", "Jones", 75m, 181, 5, new DateOnly(1990, 1, 1));

        Assert.Equal("new@example.com", user.Email.Value);
        Assert.Equal("alice2", user.Username.Value);
        Assert.Equal("Alicia", user.FirstName);
        Assert.Equal("Jones", user.LastName);
        Assert.Equal(75m, user.Weight);
        Assert.Equal(181, user.Height);
        Assert.Equal(5, user.ActiveDays);
        Assert.Single(user.DomainEvents);
        Assert.IsType<UserUpdated>(user.DomainEvents.First());
    }

    [Fact]
    public void UpdateInfo_WithInvalidEmail_ShouldThrow()
    {
        User user = User.Create("user@example.com", "alice", "hashed-password", "Alice", "Smith", 72.5m, 180, 4, new DateOnly(1990, 1, 1));

        Assert.Throws<ArgumentException>(
            () => user.UpdateInfo("not-an-email", "alice", "Alice", "Smith", 72.5m, 180, 4, new DateOnly(1990, 1, 1)));
    }
}
