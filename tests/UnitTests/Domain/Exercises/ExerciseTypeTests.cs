using Bloom.Domain.Exercises.Enums;

namespace UnitTests.Domain.Exercises;

public sealed class ExerciseTypeTests
{
    [Fact]
    public void Enum_ContainsExpectedValues()
    {
        Assert.Equal(0, (int)ExerciseType.Strength);
        Assert.Equal(1, (int)ExerciseType.Cardio);
        Assert.Equal(2, (int)ExerciseType.Plyometric);
    }
}
