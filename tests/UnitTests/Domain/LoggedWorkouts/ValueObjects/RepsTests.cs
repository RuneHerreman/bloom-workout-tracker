using Bloom.Domain.LoggedWorkouts.ValueObjects;

namespace UnitTests.Domain.LoggedWorkouts.ValueObjects;

public sealed class RepsTests
{
    [Fact]
    public void Create_WithPositiveValue_ShouldSetValue()
    {
        Reps reps = Reps.Create(10);

        Assert.Equal(10, reps.Value);
    }

    [Fact]
    public void Create_WithZero_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => Reps.Create(0));
    }

    [Fact]
    public void Create_WithNegative_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => Reps.Create(-1));
    }
}
