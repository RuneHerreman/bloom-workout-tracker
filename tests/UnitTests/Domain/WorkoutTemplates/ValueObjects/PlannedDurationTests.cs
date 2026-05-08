using Bloom.Domain.WorkoutTemplates.ValueObjects;

namespace UnitTests.Domain.WorkoutTemplates.ValueObjects;

public sealed class PlannedDurationTests
{
    [Fact]
    public void Create_WithPositiveTimeSpan_ShouldSetValue()
    {
        PlannedDuration duration = PlannedDuration.Create(TimeSpan.FromMinutes(45));

        Assert.Equal(TimeSpan.FromMinutes(45), duration.Value);
    }

    [Fact]
    public void Create_WithZero_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => PlannedDuration.Create(TimeSpan.Zero));
    }
}
