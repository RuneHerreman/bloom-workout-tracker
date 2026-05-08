using Bloom.Domain.Shared;
using Bloom.Domain.Shared.DomainEvents;

namespace UnitTests.Domain.Shared;

public sealed class AggregateRootTests
{
    [Fact]
    public void Constructor_WithoutId_ShouldInitializeDefaultIdAndNoEvents()
    {
        DefaultAggregate aggregate = new();

        TestId id = aggregate.Id;

        Assert.Equal(default, id);
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void RaiseDomainEvent_ShouldAddEvent()
    {
        TestAggregate aggregate = new(new TestId(Guid.NewGuid()));
        TestDomainEvent domainEvent = new("Created", "Aggregate");

        aggregate.AddEvent(domainEvent);

        Assert.Single(aggregate.DomainEvents);
        Assert.Contains(domainEvent, aggregate.DomainEvents);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        TestAggregate aggregate = new(new TestId(Guid.NewGuid()));
        aggregate.AddEvent(new TestDomainEvent("Created", "Aggregate"));
        aggregate.AddEvent(new TestDomainEvent("Updated", "Aggregate"));

        aggregate.ClearDomainEvents();

        Assert.Empty(aggregate.DomainEvents);
    }

    public readonly record struct TestId(Guid Value) : IEntityId;

    public sealed class TestAggregate(TestId id) : AggregateRoot<TestId>(id)
    {
        public void AddEvent(TestDomainEvent domainEvent) => RaiseDomainEvent(domainEvent);
        public override void ValidateState() { }
    }

    public sealed class TestDomainEvent(string eventName, string aggregateName)
        : BaseDomainEvent(eventName, aggregateName);

    public sealed class DefaultAggregate : AggregateRoot<TestId>
    {
        public override void ValidateState() { }
    }
}
