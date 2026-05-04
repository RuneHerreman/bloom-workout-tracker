using Bloom.Domain.Shared.DomainEvents;

namespace Bloom.Domain.Users.DomainEvents;

public class UserDomainEvent(string eventName 
) : BaseDomainEvent(eventName, nameof(User))
{
    
}