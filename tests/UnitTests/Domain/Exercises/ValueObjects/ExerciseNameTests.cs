using Bloom.Domain.Exercises.ValueObjects;

namespace UnitTests.Domain.Exercises.ValueObjects;

public sealed class ExerciseNameTests
{
    [Fact]
    public void Create_WithValidValue_ShouldSetValue()
    {
        ExerciseName name = ExerciseName.Create("Bench Press");

        Assert.Equal("Bench Press", name.Value);
    }

    [Fact]
    public void Create_WithEmptyValue_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => ExerciseName.Create(""));
    }

    [Fact]
    public void Create_WithWhitespaceValue_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => ExerciseName.Create("  "));
    }
}
