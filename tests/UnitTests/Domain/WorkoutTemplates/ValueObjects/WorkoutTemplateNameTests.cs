using Bloom.Domain.WorkoutTemplates.ValueObjects;

namespace UnitTests.Domain.WorkoutTemplates.ValueObjects;

public sealed class WorkoutTemplateNameTests
{
    [Fact]
    public void Create_WithValidValue_ShouldTrimAndStore()
    {
        WorkoutTemplateName name = WorkoutTemplateName.Create("  Push Day  ");

        Assert.Equal("Push Day", name.Value);
    }

    [Fact]
    public void Create_WithEmpty_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => WorkoutTemplateName.Create(""));
    }

    [Fact]
    public void Create_TooLong_ShouldThrow()
    {
        string tooLong = new('a', 101);

        Assert.Throws<ArgumentException>(() => WorkoutTemplateName.Create(tooLong));
    }
}
