using Bloom.Domain.Exercises;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.LoggedWorkouts.DomainEvents;
using Bloom.Domain.LoggedWorkouts.ValueObjects;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;

namespace UnitTests.Domain.LoggedWorkouts;

public sealed class LoggedWorkoutTests
{
    [Fact]
    public void Create_WithValidInput_ShouldInitializeAndRaiseEvent()
    {
        UserId userId = EntityId.New<UserId>();
        List<LoggedExercise> exercises =
        [
            LoggedExercise.Create(
                EntityId.New<ExerciseId>(),
                0,
                [LoggedSet.CreateStrength(0, 8, 60m, WeightUnit.Kg, 2)])
        ];

        LoggedWorkout workout = LoggedWorkout.Create(userId, "Leg Day", exercises);

        Assert.Equal(userId, workout.UserId);
        Assert.Equal("Leg Day", workout.Name);
        Assert.Null(workout.Note);
        Assert.Single(workout.LoggedExercises);
        Assert.Single(workout.DomainEvents);
        Assert.IsType<WorkoutLogged>(workout.DomainEvents.First());
    }

    [Fact]
    public void Create_WithNote_ShouldStoreNote()
    {
        UserId userId = EntityId.New<UserId>();
        List<LoggedExercise> exercises =
        [
            LoggedExercise.Create(
                EntityId.New<ExerciseId>(),
                0,
                [LoggedSet.CreateStrength(0, 8, 60m, WeightUnit.Kg, 2)])
        ];

        LoggedWorkout workout = LoggedWorkout.Create(userId, "Push Day", exercises, note: "Increase chest weight next time");

        Assert.Equal("Increase chest weight next time", workout.Note);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ShouldThrow(string name)
    {
        UserId userId = EntityId.New<UserId>();
        List<LoggedExercise> exercises =
        [
            LoggedExercise.Create(
                EntityId.New<ExerciseId>(),
                0,
                [LoggedSet.CreateStrength(0, 8, 60m, WeightUnit.Kg, 2)])
        ];

        Assert.Throws<ArgumentException>(
            () => LoggedWorkout.Create(userId, name, exercises));
    }

    [Fact]
    public void Update_WithValidInput_ShouldReplaceStateAndRaiseEvent()
    {
        UserId userId = EntityId.New<UserId>();
        LoggedWorkout workout = LoggedWorkout.Create(userId, "Leg Day",
        [
            LoggedExercise.Create(
                EntityId.New<ExerciseId>(),
                0,
                [LoggedSet.CreateStrength(0, 8, 60m, WeightUnit.Kg, 2)])
        ]);

        DateTime newLoggedAt = DateTime.UtcNow.AddDays(-1);
        List<LoggedExercise> updatedExercises =
        [
            LoggedExercise.Create(
                EntityId.New<ExerciseId>(),
                1,
                [LoggedSet.CreateCardio(0, TimeSpan.FromMinutes(15), 3m, DistanceUnit.Km)])
        ];

        workout.Update("Cardio Session", "Felt great", newLoggedAt, updatedExercises);

        Assert.Equal(newLoggedAt, workout.LoggedAt);
        Assert.Equal("Cardio Session", workout.Name);
        Assert.Equal("Felt great", workout.Note);
        Assert.Single(workout.LoggedExercises);
        Assert.Equal(2, workout.DomainEvents.Count);
        Assert.IsType<LoggedWorkoutUpdated>(workout.DomainEvents.Last());
    }

    [Fact]
    public void Create_WithEmptyExercises_ShouldThrow()
    {
        UserId userId = EntityId.New<UserId>();

        Assert.Throws<ArgumentException>(
            () => LoggedWorkout.Create(userId, "Leg Day", []));
    }
}
