namespace Bloom.Domain.Shared.DomainEvents;

public interface IDomainEventPublisher
{
    Task Publish(IDomainEvent domainEvent);
    void Register(IDomainEventListener listener);
}
