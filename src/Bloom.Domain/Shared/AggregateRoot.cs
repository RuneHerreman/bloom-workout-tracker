using Bloom.Domain.Shared.DomainEvents;

namespace Bloom.Domain.Shared;
public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    where TId : struct, IEntityId
{
    private readonly IList<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    protected AggregateRoot(TId id) : base(id) {}
    protected AggregateRoot() { }
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
