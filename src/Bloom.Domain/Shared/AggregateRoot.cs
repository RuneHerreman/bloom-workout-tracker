namespace Bloom.Domain.Shared;

public interface IAggregateRoot { }

public abstract class AggregateRoot<TId>: Entity<TId>, IAggregateRoot
    where TId: struct, IEntityId
{
    protected AggregateRoot() { }
    
    protected AggregateRoot(TId id) : base(id) { }
}