using System.Reflection;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.LoggedWorkouts.ValueObjects;

namespace UnitTests.Domain.LoggedWorkouts;

public sealed class LoggedSetTests
{
    [Fact]
    public void CreateCardio_WithValidInput_ShouldSetCardioFields()
    {
        LoggedSet set = LoggedSet.CreateCardio(0, TimeSpan.FromMinutes(20), 5m, DistanceUnit.Km);

        Assert.Equal(ExerciseType.Cardio, set.Type);
        Assert.Equal(0, set.Order);
        Assert.NotNull(set.Duration);
        Assert.NotNull(set.Distance);
        Assert.Null(set.Reps);
        Assert.Null(set.Weight);
        Assert.Null(set.Rir);
        Assert.Equal(TimeSpan.FromMinutes(20), set.Duration!.Value);
        Assert.Equal(5m, set.Distance!.Value);
    }

    [Fact]
    public void CreateStrength_WithValidInput_ShouldSetStrengthFields()
    {
        LoggedSet set = LoggedSet.CreateStrength(1, 10, 80m, WeightUnit.Kg, 2);

        Assert.Equal(ExerciseType.Strength, set.Type);
        Assert.Equal(1, set.Order);
        Assert.NotNull(set.Reps);
        Assert.NotNull(set.Weight);
        Assert.NotNull(set.Rir);
        Assert.Null(set.Duration);
        Assert.Null(set.Distance);
        Assert.Equal(10, set.Reps!.Value);
        Assert.Equal(80m, set.Weight!.Value);
        Assert.Equal(2, set.Rir!.Value);
    }

    [Fact]
    public void CreatePlyometric_WithValidInput_ShouldSetType()
    {
        LoggedSet set = LoggedSet.CreatePlyometric(0, 8, 20m, WeightUnit.Lbs, 1);

        Assert.Equal(ExerciseType.Plyometric, set.Type);
        Assert.NotNull(set.Reps);
        Assert.NotNull(set.Weight);
        Assert.NotNull(set.Rir);
    }

    [Fact]
    public void CreateCardio_WithNegativeOrder_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(
            () => LoggedSet.CreateCardio(-1, TimeSpan.FromMinutes(10), 1m, DistanceUnit.Km));
    }

    [Fact]
    public void CreateStrength_WithInvalidReps_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(
            () => LoggedSet.CreateStrength(0, 0, 50m, WeightUnit.Kg, 1));
    }

    [Fact]
    public void CreateStrengthLike_WithCardioType_ShouldThrow()
    {
        MethodInfo method = typeof(LoggedSet).GetMethod(
            "CreateStrengthLike",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        TargetInvocationException ex = Assert.Throws<TargetInvocationException>(() =>
            method.Invoke(null, [ExerciseType.Cardio, 0, 1, 1m, WeightUnit.Kg, 1]));

        Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException);
    }

    [Fact]
    public void ValidateState_WithUnsupportedType_ShouldThrow()
    {
        LoggedSet set = LoggedSet.CreateStrength(0, 5, 50m, WeightUnit.Kg, 0);
        FieldInfo field = typeof(LoggedSet).GetField("<Type>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        field.SetValue(set, (ExerciseType)999);

        Assert.Throws<ArgumentOutOfRangeException>(() => set.ValidateState());
    }
}
