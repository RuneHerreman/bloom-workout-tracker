using Bloom.Domain.Shared.DomainEvents;

namespace UnitTests.Application.Mocks;

public sealed class TestDomainEventPublisher : IDomainEventPublisher
{
    private readonly List<IDomainEventListener> _listeners = [];
    public List<IDomainEvent> PublishedEvents { get; } = [];

    public async Task Publish(IDomainEvent domainEvent)
    {
        PublishedEvents.Add(domainEvent);

        foreach (var listener in _listeners)
            await listener.Listen(domainEvent);
    }

    public void Register(IDomainEventListener listener)
    {
        _listeners.Add(listener);
    }
}
