using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Domain.Users.DomainEvents;

namespace UnitTests.Domain.Users.DomainEvents;

public sealed class UserDomainEventsTests
{
    [Fact]
    public void UserRegistered_ShouldExposeIdAndFqdn()
    {
        UserId id = EntityId.New<UserId>();

        UserRegistered evt = new(id);

        Assert.Equal(id.Value.ToString(), evt.UserId);
        Assert.Equal("Bloom.Bloom.User.UserRegistered", evt.FQDN);
    }
}
