using Bloom.Domain.Exercises.ValueObjects;

namespace UnitTests.Domain.Exercises.ValueObjects;

public sealed class ExerciseDescriptionTests
{
    [Fact]
    public void Create_WithValidValue_ShouldSetValue()
    {
        ExerciseDescription description = ExerciseDescription.Create("Compound movement.");

        Assert.Equal("Compound movement.", description.Value);
    }

    [Fact]
    public void Create_WithEmptyValue_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => ExerciseDescription.Create(""));
    }

    [Fact]
    public void Create_WithWhitespaceValue_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => ExerciseDescription.Create("   "));
    }
}
