using System.Reflection;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.WorkoutTemplates;
using Bloom.Domain.WorkoutTemplates.ValueObjects;

namespace UnitTests.Domain.WorkoutTemplates;

public sealed class PlannedSetTests
{
    [Fact]
    public void CreateCardio_WithValidInput_ShouldSetCardioFields()
    {
        PlannedSet set = PlannedSet.CreateCardio(0, TimeSpan.FromMinutes(20), 5m, PlannedDistanceUnit.Km);

        Assert.Equal(ExerciseType.Cardio, set.Type);
        Assert.Equal(0, set.Order);
        Assert.NotNull(set.Duration);
        Assert.NotNull(set.Distance);
        Assert.Null(set.Reps);
    }

    [Fact]
    public void CreateStrengthLike_WithStrengthType_ShouldSetReps()
    {
        PlannedSet set = PlannedSet.CreateStrengthLike(ExerciseType.Strength, 0, 10);

        Assert.Equal(ExerciseType.Strength, set.Type);
        Assert.NotNull(set.Reps);
        Assert.Null(set.Duration);
        Assert.Null(set.Distance);
    }

    [Fact]
    public void CreateStrengthLike_WithPlyometricType_ShouldSetReps()
    {
        PlannedSet set = PlannedSet.CreateStrengthLike(ExerciseType.Plyometric, 1, 6);

        Assert.Equal(ExerciseType.Plyometric, set.Type);
        Assert.NotNull(set.Reps);
    }

    [Fact]
    public void CreateStrengthLike_WithCardioType_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PlannedSet.CreateStrengthLike(ExerciseType.Cardio, 0, 10));
    }

    [Fact]
    public void CreateCardio_WithNegativeOrder_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(
            () => PlannedSet.CreateCardio(-1, TimeSpan.FromMinutes(10), 1m, PlannedDistanceUnit.Km));
    }

    [Fact]
    public void ValidateState_WithUnsupportedType_ShouldThrow()
    {
        PlannedSet set = PlannedSet.CreateStrengthLike(ExerciseType.Strength, 0, 5);
        FieldInfo field = typeof(PlannedSet).GetField("<Type>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        field.SetValue(set, (ExerciseType)999);

        Assert.Throws<ArgumentOutOfRangeException>(() => set.ValidateState());
    }
}
