using Bloom.Domain.LoggedWorkouts.ValueObjects;

namespace UnitTests.Domain.LoggedWorkouts.ValueObjects;

public sealed class WeightTests
{
    [Fact]
    public void Create_WithPositiveValue_ShouldRoundToTwoDecimals()
    {
        Weight weight = Weight.Create(80.567m, WeightUnit.Kg);

        Assert.Equal(80.57m, weight.Value);
        Assert.Equal(WeightUnit.Kg, weight.Unit);
    }

    [Fact]
    public void Create_WithLbsUnit_ShouldSetUnit()
    {
        Weight weight = Weight.Create(180m, WeightUnit.Lbs);

        Assert.Equal(WeightUnit.Lbs, weight.Unit);
    }

    [Fact]
    public void Create_WithZero_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => Weight.Create(-1m, WeightUnit.Kg));
    }

    [Fact]
    public void Create_WithNegative_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => Weight.Create(-5m, WeightUnit.Kg));
    }

    [Fact]
    public void WeightUnit_ContainsExpectedValues()
    {
        Assert.Equal(0, (int)WeightUnit.Kg);
        Assert.Equal(1, (int)WeightUnit.Lbs);
    }
}
