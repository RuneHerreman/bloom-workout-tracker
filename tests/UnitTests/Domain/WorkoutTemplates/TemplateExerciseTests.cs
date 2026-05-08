using Bloom.Domain.Exercises;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.Shared;
using Bloom.Domain.WorkoutTemplates;

namespace UnitTests.Domain.WorkoutTemplates;

public sealed class TemplateExerciseTests
{
    [Fact]
    public void Create_WithValidInput_ShouldInitialize()
    {
        ExerciseId exerciseId = EntityId.New<ExerciseId>();
        List<PlannedSet> sets = [PlannedSet.CreateStrengthLike(ExerciseType.Strength, 0, 10)];

        TemplateExercise te = TemplateExercise.Create(exerciseId, 0, sets);

        Assert.Equal(exerciseId, te.ExerciseId);
        Assert.Equal(0, te.Order);
        Assert.Single(te.Sets);
    }

    [Fact]
    public void Create_WithProvidedId_ShouldUseId()
    {
        TemplateExerciseId id = EntityId.New<TemplateExerciseId>();
        ExerciseId exerciseId = EntityId.New<ExerciseId>();
        List<PlannedSet> sets = [PlannedSet.CreateStrengthLike(ExerciseType.Strength, 0, 10)];

        TemplateExercise te = TemplateExercise.Create(exerciseId, 0, sets, id);

        Assert.Equal(id, te.Id);
    }

    [Fact]
    public void Create_WithEmptySets_ShouldThrow()
    {
        ExerciseId exerciseId = EntityId.New<ExerciseId>();

        Assert.Throws<ArgumentException>(
            () => TemplateExercise.Create(exerciseId, 0, []));
    }

    [Fact]
    public void Create_WithNegativeOrder_ShouldThrow()
    {
        ExerciseId exerciseId = EntityId.New<ExerciseId>();
        List<PlannedSet> sets = [PlannedSet.CreateStrengthLike(ExerciseType.Strength, 0, 10)];

        Assert.Throws<ArgumentException>(
            () => TemplateExercise.Create(exerciseId, -1, sets));
    }
}
