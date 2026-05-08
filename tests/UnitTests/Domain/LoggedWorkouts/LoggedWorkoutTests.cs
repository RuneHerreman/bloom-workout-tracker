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

        LoggedWorkout workout = LoggedWorkout.Create(userId, exercises);

        Assert.Equal(userId, workout.UserId);
        Assert.Single(workout.LoggedExercises);
        Assert.Single(workout.DomainEvents);
        Assert.IsType<WorkoutLogged>(workout.DomainEvents.First());
    }

    [Fact]
    public void Update_WithValidInput_ShouldReplaceStateAndRaiseEvent()
    {
        UserId userId = EntityId.New<UserId>();
        LoggedWorkout workout = LoggedWorkout.Create(userId,
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

        workout.Update(newLoggedAt, updatedExercises);

        Assert.Equal(newLoggedAt, workout.LoggedAt);
        Assert.Single(workout.LoggedExercises);
        Assert.Equal(2, workout.DomainEvents.Count);
        Assert.IsType<LoggedWorkoutUpdated>(workout.DomainEvents.Last());
    }

    [Fact]
    public void Create_WithEmptyExercises_ShouldThrow()
    {
        UserId userId = EntityId.New<UserId>();

        Assert.Throws<ArgumentException>(
            () => LoggedWorkout.Create(userId, []));
    }
}
