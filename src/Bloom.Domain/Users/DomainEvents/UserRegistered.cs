namespace Bloom.Domain.Users.DomainEvents;

public class UserRegistered(
    UserId userId
) : UserDomainEvent(nameof(UserRegistered))
{
    public string UserId = userId.Value.ToString();
}