using Bloom.Domain.LoggedWorkouts.ValueObjects;

namespace UnitTests.Domain.LoggedWorkouts.ValueObjects;

public sealed class DurationTests
{
    [Fact]
    public void Create_WithPositiveTimeSpan_ShouldSetValue()
    {
        Duration duration = Duration.Create(TimeSpan.FromMinutes(30));

        Assert.Equal(TimeSpan.FromMinutes(30), duration.Value);
    }

    [Fact]
    public void Create_WithZeroTimeSpan_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => Duration.Create(TimeSpan.Zero));
    }

    [Fact]
    public void Create_WithNegativeTimeSpan_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => Duration.Create(TimeSpan.FromSeconds(-1)));
    }
}
