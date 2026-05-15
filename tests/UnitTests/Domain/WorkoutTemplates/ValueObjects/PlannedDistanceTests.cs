using Bloom.Domain.WorkoutTemplates.ValueObjects;

namespace UnitTests.Domain.WorkoutTemplates.ValueObjects;

public sealed class PlannedDistanceTests
{
    [Fact]
    public void Create_WithPositiveValue_ShouldRoundToTwoDecimals()
    {
        PlannedDistance distance = PlannedDistance.Create(5.123m, PlannedDistanceUnit.Km);

        Assert.Equal(5.123m, distance.Value);
        Assert.Equal(PlannedDistanceUnit.Km, distance.Unit);
    }

    [Fact]
    public void Create_WithMilesUnit_ShouldSetUnit()
    {
        PlannedDistance distance = PlannedDistance.Create(2m, PlannedDistanceUnit.Miles);

        Assert.Equal(PlannedDistanceUnit.Miles, distance.Unit);
    }

    [Fact]
    public void Create_WithZero_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => PlannedDistance.Create(0m, PlannedDistanceUnit.Km));
    }

    [Fact]
    public void PlannedDistanceUnit_ContainsExpectedValues()
    {
        Assert.Equal(0, (int)PlannedDistanceUnit.Km);
        Assert.Equal(1, (int)PlannedDistanceUnit.Miles);
    }
}
