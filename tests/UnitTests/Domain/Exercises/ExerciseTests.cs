using Bloom.Domain.Exercises;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;

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
            ["Quadriceps"],
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

    [Fact]
    public void Create_Catalog_WithNameOutsideTree_ShouldThrow()
    {
        Action act = () => Exercise.Create("Neck Curl", "Neck.", ExerciseType.Strength, ["Neck"]);

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void CreateCustom_WithNameOutsideTree_ShouldKeepIt()
    {
        Exercise exercise = Exercise.CreateCustom(EntityId.New<UserId>(), "Neck Curl", "Neck.", ExerciseType.Strength, ["neck harness", "traps"]);

        Assert.Equal(["neck harness", "Traps"], exercise.TargetMuscles.Select(m => m.Value));
    }

    [Fact]
    public void SetMuscles_ShouldTidyTheList()
    {
        Exercise exercise = Exercise.Create("Row", "Pull.", ExerciseType.Strength, ["Back"]);

        // Old names map onto the tree, duplicates go, and a listed muscle makes its area redundant.
        exercise.SetMuscles(["back", "Upper back", "Lats", "Rhomboids", "Biceps", "Quadriceps"]);

        Assert.Equal(["Mid back", "Lats", "Biceps", "Quads"], exercise.TargetMuscles.Select(m => m.Value));
        Assert.True(exercise.HasMuscles(["Mid back", "Lats", "Biceps", "Quads"]));
        Assert.False(exercise.HasMuscles(["Mid back", "Lats"]));
    }

    [Fact]
    public void SetMuscles_FullBody_ShouldBecomeEveryArea()
    {
        Exercise exercise = Exercise.Create("Burpee", "All of it.", ExerciseType.Plyometric, ["Full Body"]);

        Assert.Equal(["Chest", "Back", "Shoulders", "Arms", "Core", "Legs"], exercise.TargetMuscles.Select(m => m.Value));
    }

    [Theory]
    [InlineData("Lats", "Back")]
    [InlineData("quadriceps", "Legs")]
    [InlineData("Back", "Back")]
    [InlineData("Neck", "Neck")]
    public void MuscleTree_AreaOf_ShouldFindTheArea(string name, string area)
    {
        Assert.Equal(area, MuscleTree.AreaOf(name));
    }
}
