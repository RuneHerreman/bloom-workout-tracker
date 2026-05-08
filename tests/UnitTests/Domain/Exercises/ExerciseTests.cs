using Bloom.Domain.Exercises;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.Shared;

namespace UnitTests.Domain.Exercises;

public sealed class ExerciseTests
{
    [Fact]
    public void Create_WithValidInput_ShouldInitializeProperties()
    {
        Exercise exercise = Exercise.Create(
            "Bench Press",
            "Compound chest movement.",
            ExerciseType.Strength,
            ["Chest", "Triceps"]
        );

        Assert.Equal("Bench Press", exercise.Name.Value);
        Assert.Equal("Compound chest movement.", exercise.Description.Value);
        Assert.Equal(ExerciseType.Strength, exercise.Type);
        Assert.Equal(2, exercise.TargetMuscles.Count);
        Assert.Contains(exercise.TargetMuscles, m => m.Value == "Chest");
        Assert.Contains(exercise.TargetMuscles, m => m.Value == "Triceps");
    }

    [Fact]
    public void Create_WithProvidedId_ShouldUseId()
    {
        ExerciseId id = EntityId.New<ExerciseId>();

        Exercise exercise = Exercise.Create(
            "Squat",
            "Compound leg movement.",
            ExerciseType.Strength,
            ["Quads"],
            id
        );

        Assert.Equal(id, exercise.Id);
    }

    [Fact]
    public void Create_WithoutId_ShouldGenerateNonEmptyId()
    {
        Exercise exercise = Exercise.Create(
            "Plank",
            "Core stability.",
            ExerciseType.Plyometric,
            ["Core"]
        );

        Assert.NotEqual(Guid.Empty, exercise.Id.Value);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrow()
    {
        Action act = () => Exercise.Create(
            "",
            "Description",
            ExerciseType.Strength,
            ["Chest"]
        );

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_WithEmptyDescription_ShouldThrow()
    {
        Action act = () => Exercise.Create(
            "Name",
            "",
            ExerciseType.Strength,
            ["Chest"]
        );

        Assert.Throws<ArgumentException>(act);
    }
}
