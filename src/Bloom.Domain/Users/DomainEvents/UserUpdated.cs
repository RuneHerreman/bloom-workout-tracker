namespace Bloom.Domain.Users.DomainEvents;

public class UserUpdated(
    UserId userId
) : UserDomainEvent(nameof(UserUpdated))
{
    public string UserId = userId.Value.ToString();
}
