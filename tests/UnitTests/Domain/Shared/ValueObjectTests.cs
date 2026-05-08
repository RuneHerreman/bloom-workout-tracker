using Bloom.Domain.Shared;

namespace UnitTests.Domain.Shared;

public sealed class ValueObjectTests
{
    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        SampleValueObject first = new(5, "value");
        SampleValueObject second = new(5, "value");

        Assert.True(first == second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        SampleValueObject first = new(5, "value");
        SampleValueObject second = new(6, "value");

        Assert.False(first == second);
    }

    public sealed record SampleValueObject(int Number, string Text) : ValueObject;
}
