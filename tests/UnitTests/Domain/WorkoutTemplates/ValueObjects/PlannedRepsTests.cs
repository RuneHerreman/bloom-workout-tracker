using Bloom.Domain.WorkoutTemplates.ValueObjects;

namespace UnitTests.Domain.WorkoutTemplates.ValueObjects;

public sealed class PlannedRepsTests
{
    [Fact]
    public void Create_WithPositive_ShouldSetValue()
    {
        PlannedReps reps = PlannedReps.Create(8);

        Assert.Equal(8, reps.Value);
    }

    [Fact]
    public void Create_WithZero_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => PlannedReps.Create(0));
    }
}
