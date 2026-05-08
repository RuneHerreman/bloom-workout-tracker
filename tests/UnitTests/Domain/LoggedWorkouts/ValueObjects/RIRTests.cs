using Bloom.Domain.LoggedWorkouts.ValueObjects;

namespace UnitTests.Domain.LoggedWorkouts.ValueObjects;

public sealed class RIRTests
{
    [Fact]
    public void Create_WithZero_ShouldSetValue()
    {
        RIR rir = RIR.Create(0);

        Assert.Equal(0, rir.Value);
    }

    [Fact]
    public void Create_WithMaxAllowed_ShouldSetValue()
    {
        RIR rir = RIR.Create(10);

        Assert.Equal(10, rir.Value);
    }

    [Fact]
    public void Create_WithNegative_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => RIR.Create(-1));
    }

    [Fact]
    public void Create_WithValueAtUpperBound_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => RIR.Create(11));
    }
}
