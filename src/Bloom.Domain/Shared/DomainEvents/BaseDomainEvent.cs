namespace Bloom.Domain.Shared.DomainEvents;

public class BaseDomainEvent(
    string eventName,
    string aggregateName,
    string boundedContext = nameof(Bloom),
    string companyName = nameof(Bloom)
) : IDomainEvent
{
    public string FQDN { get; private init; } = $"{companyName}.{boundedContext}.{aggregateName}.{eventName}";
    public DateTime OccurredOn { get; private init; } = DateTime.UtcNow;
}
