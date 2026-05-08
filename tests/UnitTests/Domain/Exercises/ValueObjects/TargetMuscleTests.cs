using Bloom.Domain.Exercises.ValueObjects;

namespace UnitTests.Domain.Exercises.ValueObjects;

public sealed class TargetMuscleTests
{
    [Fact]
    public void Create_WithValidValue_ShouldSetValue()
    {
        TargetMuscle muscle = TargetMuscle.Create("Chest");

        Assert.Equal("Chest", muscle.Value);
    }

    [Fact]
    public void Create_WithEmptyValue_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => TargetMuscle.Create(""));
    }
}
