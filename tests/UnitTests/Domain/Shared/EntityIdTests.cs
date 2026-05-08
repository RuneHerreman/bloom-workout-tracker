using Bloom.Domain.Shared;

namespace UnitTests.Domain.Shared;

public sealed class EntityIdTests
{
    [Fact]
    public void New_WithProvidedGuid_ShouldUseGuid()
    {
        Guid guid = Guid.NewGuid();

        TestId id = EntityId.New<TestId>(guid);

        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void New_WithoutGuid_ShouldCreateNonEmptyValue()
    {
        TestId id = EntityId.New<TestId>();

        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void New_CalledMultipleTimes_ShouldUseCachedFactory()
    {
        TestId first = EntityId.New<TestId>();
        TestId second = EntityId.New<TestId>();

        Assert.NotEqual(first.Value, second.Value);
    }

    [Fact]
    public void New_WithMissingGuidConstructor_ShouldThrow()
    {
        Action act = () => EntityId.New<BadId>();

        Exception? exception = Record.Exception(act);

        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }

    public readonly record struct TestId(Guid Value) : IEntityId;

    public readonly record struct BadId(int Number) : IEntityId
    {
        public Guid Value => Guid.Empty;
    }
}
