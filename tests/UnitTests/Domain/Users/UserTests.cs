using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Domain.Users.DomainEvents;

namespace UnitTests.Domain.Users;

public sealed class UserTests
{
    [Fact]
    public void Create_WithValidInput_ShouldInitializeAndRaiseEvent()
    {
        User user = User.Create("user@example.com", "alice", "hashed-password");

        Assert.Equal("user@example.com", user.Email.Value);
        Assert.Equal("alice", user.Username.Value);
        Assert.Equal("hashed-password", user.HashedPassword.Value);
        Assert.Single(user.DomainEvents);
        Assert.IsType<UserRegistered>(user.DomainEvents.First());
    }

    [Fact]
    public void Create_WithProvidedId_ShouldUseId()
    {
        UserId id = EntityId.New<UserId>();

        User user = User.Create("user@example.com", "alice", "hashed-password", id);

        Assert.Equal(id, user.Id);
    }

    [Fact]
    public void ChangePassword_ShouldReplaceHashedPassword()
    {
        User user = User.Create("user@example.com", "alice", "old-hash");

        user.ChangePassword("new-hash");

        Assert.Equal("new-hash", user.HashedPassword.Value);
    }
}
