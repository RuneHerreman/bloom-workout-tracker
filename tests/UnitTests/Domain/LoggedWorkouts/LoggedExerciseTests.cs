using Bloom.Domain.Exercises;
using Bloom.Domain.Exercises.Enums;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.LoggedWorkouts.ValueObjects;
using Bloom.Domain.Shared;

namespace UnitTests.Domain.LoggedWorkouts;

public sealed class LoggedExerciseTests
{
    [Fact]
    public void Create_WithValidInput_ShouldInitializeAndValidate()
    {
        ExerciseId exerciseId = EntityId.New<ExerciseId>();
        List<LoggedSet> sets =
        [
            LoggedSet.CreateStrength(0, 8, 60m, WeightUnit.Kg, 2)
        ];

        LoggedExercise loggedExercise = LoggedExercise.Create(exerciseId, 0, sets);

        Assert.Equal(exerciseId, loggedExercise.ExerciseId);
        Assert.Equal(0, loggedExercise.Order);
        Assert.Single(loggedExercise.Sets);
    }

    [Fact]
    public void Create_WithMixedSets_ExposesTypedConvenienceCollections()
    {
        ExerciseId exerciseId = EntityId.New<ExerciseId>();
        List<LoggedSet> sets =
        [
            LoggedSet.CreateStrength(0, 5, 100m, WeightUnit.Kg, 1),
            LoggedSet.CreatePlyometric(1, 5, 20m, WeightUnit.Kg, 1),
            LoggedSet.CreateCardio(2, TimeSpan.FromMinutes(10), 2m, DistanceUnit.Km)
        ];

        LoggedExercise loggedExercise = LoggedExercise.Create(exerciseId, 0, sets);

        Assert.Single(loggedExercise.StrengthSets);
        Assert.Single(loggedExercise.PlyometricSets);
        Assert.Single(loggedExercise.CardioSets);
    }

    [Fact]
    public void Create_WithEmptySets_ShouldThrow()
    {
        ExerciseId exerciseId = EntityId.New<ExerciseId>();

        Assert.Throws<ArgumentException>(
            () => LoggedExercise.Create(exerciseId, 0, []));
    }

    [Fact]
    public void Create_WithNegativeOrder_ShouldThrow()
    {
        ExerciseId exerciseId = EntityId.New<ExerciseId>();
        List<LoggedSet> sets = [LoggedSet.CreateStrength(0, 5, 50m, WeightUnit.Kg, 1)];

        Assert.Throws<ArgumentException>(
            () => LoggedExercise.Create(exerciseId, -1, sets));
    }
}
