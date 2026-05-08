using Bloom.Domain.Shared.DomainEvents;

namespace UnitTests.Domain.Shared;

public sealed class BaseDomainEventTests
{
    [Fact]
    public void Constructor_WithDefaults_ShouldSetFqdn()
    {
        BaseDomainEvent domainEvent = new("Created", "Aggregate");

        Assert.Equal("Bloom.Bloom.Aggregate.Created", domainEvent.FQDN);
    }

    [Fact]
    public void Constructor_WithCustomBoundedContextAndCompany_ShouldSetFqdn()
    {
        BaseDomainEvent domainEvent = new("Created", "Aggregate", "Workouts", "Acme");

        Assert.Equal("Acme.Workouts.Aggregate.Created", domainEvent.FQDN);
    }

    [Fact]
    public void Constructor_ShouldSetOccurredOnInUtc()
    {
        DateTime before = DateTime.UtcNow;

        BaseDomainEvent domainEvent = new("Created", "Aggregate");
        DateTime after = DateTime.UtcNow;

        Assert.InRange(domainEvent.OccurredOn, before, after);
    }
}
