using Bloom.Domain.Shared.DomainEvents;

namespace Bloom.Application.Contracts.Ports;

public interface IPolicy<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    Task Execute (TDomainEvent domainEvent);
}