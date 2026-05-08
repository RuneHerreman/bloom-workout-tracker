using Bloom.Domain.LoggedWorkouts.ValueObjects;

namespace UnitTests.Domain.LoggedWorkouts.ValueObjects;

public sealed class DistanceTests
{
    [Fact]
    public void Create_WithPositiveValue_ShouldRoundToTwoDecimals()
    {
        Distance distance = Distance.Create(5.123m, DistanceUnit.Km);

        Assert.Equal(5.12m, distance.Value);
        Assert.Equal(DistanceUnit.Km, distance.Unit);
    }

    [Fact]
    public void Create_WithMilesUnit_ShouldSetUnit()
    {
        Distance distance = Distance.Create(3m, DistanceUnit.Miles);

        Assert.Equal(DistanceUnit.Miles, distance.Unit);
    }

    [Fact]
    public void Create_WithZeroValue_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => Distance.Create(0m, DistanceUnit.Km));
    }

    [Fact]
    public void Create_WithNegativeValue_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => Distance.Create(-1m, DistanceUnit.Km));
    }

    [Fact]
    public void DistanceUnit_ContainsExpectedValues()
    {
        Assert.Equal(0, (int)DistanceUnit.Km);
        Assert.Equal(1, (int)DistanceUnit.Miles);
    }
}
