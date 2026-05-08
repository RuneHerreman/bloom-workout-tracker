using Bloom.Domain.Shared;

namespace UnitTests.Domain.Shared;

public sealed class EntityTests
{
    [Fact]
    public void Constructor_WithoutId_ShouldInitializeDefaultId()
    {
        DefaultEntity entity = new();

        Assert.Equal(default, entity.Id);
    }

    [Fact]
    public void Equals_WithSameIdAndType_ShouldBeEqual()
    {
        TestId id = new(Guid.NewGuid());
        TestEntity first = new(id);
        TestEntity second = new(id);

        Assert.True(first.Equals(second));
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
        Assert.True(first == second);
    }

    [Fact]
    public void Equals_WithDifferentId_ShouldNotBeEqual()
    {
        TestEntity first = new(new TestId(Guid.NewGuid()));
        TestEntity second = new(new TestId(Guid.NewGuid()));

        Assert.False(first.Equals(second));
        Assert.True(first != second);
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldNotBeEqual()
    {
        TestId id = new(Guid.NewGuid());
        TestEntity first = new(id);
        OtherEntity second = new(id);

        Assert.False(first.Equals(second));
        Assert.False(first == second);
    }

    [Fact]
    public void Equals_WithNullObject_ShouldReturnFalse()
    {
        TestEntity entity = new(new TestId(Guid.NewGuid()));

        Assert.False(entity.Equals((object?)null));
    }

    [Fact]
    public void Equals_WithSameReference_ShouldReturnTrue()
    {
        TestEntity entity = new(new TestId(Guid.NewGuid()));

        Assert.True(entity.Equals((object)entity));
    }

    [Fact]
    public void Equals_WithSameReferenceEntityOverload_ShouldReturnTrue()
    {
        TestEntity entity = new(new TestId(Guid.NewGuid()));

        Assert.True(entity.Equals((Entity<TestId>)entity));
    }

    [Fact]
    public void Equals_ObjectWithSameType_ShouldBeEqual()
    {
        TestId id = new(Guid.NewGuid());
        TestEntity first = new(id);
        TestEntity second = new(id);

        Assert.True(first.Equals((object)second));
    }

    [Fact]
    public void Equals_ObjectWithNonEntityType_ShouldReturnFalse()
    {
        TestEntity first = new(new TestId(Guid.NewGuid()));

        Assert.False(first.Equals((object)"not-an-entity"));
    }

    [Fact]
    public void Equals_WithOtherNullEntity_ShouldReturnFalse()
    {
        TestEntity entity = new(new TestId(Guid.NewGuid()));

        Assert.False(entity.Equals((Entity<TestId>?)null));
    }

    [Fact]
    public void OperatorEquals_WithBothNull_ShouldBeTrue()
    {
        TestEntity? left = null;
        TestEntity? right = null;

        Assert.True(left == right);
    }

    [Fact]
    public void OperatorEquals_WithLeftNull_ShouldBeFalse()
    {
        TestEntity? left = null;
        TestEntity right = new(new TestId(Guid.NewGuid()));

        Assert.False(left == right);
    }

    [Fact]
    public void OperatorEquals_WithRightNull_ShouldBeFalse()
    {
        TestEntity left = new(new TestId(Guid.NewGuid()));
        TestEntity? right = null;

        Assert.False(left == right);
    }

    public readonly record struct TestId(Guid Value) : IEntityId;

    public sealed class TestEntity(TestId id) : Entity<TestId>(id)
    {
        public override void ValidateState() { }
    }

    public sealed class OtherEntity(TestId id) : Entity<TestId>(id)
    {
        public override void ValidateState() { }
    }

    public sealed class DefaultEntity : Entity<TestId>
    {
        public override void ValidateState() { }
    }
}
