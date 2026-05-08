using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.LoggedWorkouts.ValueObjects;

namespace UnitTests.Domain.LoggedWorkouts;

public sealed class LoggedStrengthSetTests
{
    [Fact]
    public void Create_WithValidInput_ShouldInitializeProperties()
    {
        LoggedStrengthSet set = LoggedStrengthSet.Create(10, 80m, WeightUnit.Kg, 2);

        Assert.Equal(10, set.Reps.Value);
        Assert.Equal(80m, set.Weight.Value);
        Assert.Equal(WeightUnit.Kg, set.Weight.Unit);
        Assert.Equal(2, set.RIR.Value);
    }

    [Fact]
    public void Create_WithInvalidReps_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(
            () => LoggedStrengthSet.Create(0, 80m, WeightUnit.Kg, 2));
    }
}
