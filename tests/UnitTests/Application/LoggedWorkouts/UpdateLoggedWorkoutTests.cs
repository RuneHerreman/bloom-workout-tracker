using Bloom.Application.LoggedWorkouts;
using Bloom.Domain.Exercises;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.LoggedWorkouts.ValueObjects;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using UnitTests.Application.Shared;

namespace UnitTests.Application.LoggedWorkouts;

public sealed class UpdateLoggedWorkoutTests : ApplicationTestBase
{
    private readonly UpdateLoggedWorkout _useCase;

    public UpdateLoggedWorkoutTests()
    {
        _useCase = new UpdateLoggedWorkout(UnitOfWork, CreateLogger<UpdateLoggedWorkout>());
    }

    private async Task<LoggedWorkout> SeedLoggedWorkout(UserId userId)
    {
        var workout = LoggedWorkout.Create(userId,
        [
            LoggedExercise.Create(
                EntityId.New<ExerciseId>(),
                0,
                [LoggedSet.CreateStrength(0, 8, 60m, WeightUnit.Kg, 2)])
        ]);

        await LoggedWorkoutRepository.Save(workout);
        return workout;
    }

    [Fact]
    public async Task Execute_WithOwnedWorkout_ShouldUpdate()
    {
        UserId userId = EntityId.New<UserId>();
        var workout = await SeedLoggedWorkout(userId);

        DateTime newDate = DateTime.UtcNow.AddDays(-1);
        var input = new UpdateLoggedWorkoutInput(
            workout.Id.Value,
            userId.Value,
            newDate,
            [
                new LoggedExerciseInput(
                    Guid.NewGuid(),
                    0,
                    [new LoggedSetInput("Cardio", 0, TimeSpan.FromMinutes(20), 5m, "Km", null, null, null, null)])
            ]);

        var output = await _useCase.Execute(input);

        Assert.Equal(workout.Id.Value, output.LoggedWorkoutId);
        var saved = await LoggedWorkoutRepository.ById(workout.Id);
        Assert.True(saved.HasValue);
        Assert.Equal(newDate, saved.Value.LoggedAt);
    }

    [Fact]
    public async Task Execute_WithMissingWorkout_ShouldThrow()
    {
        var input = new UpdateLoggedWorkoutInput(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            [
                new LoggedExerciseInput(
                    Guid.NewGuid(), 0,
                    [new LoggedSetInput("Strength", 0, null, null, null, 5, 50m, "Kg", 1)])
            ]);

        await Assert.ThrowsAsync<LoggedWorkoutNotFoundException>(() => _useCase.Execute(input));
    }

    [Fact]
    public async Task Execute_WithUnauthorizedUser_ShouldThrow()
    {
        UserId ownerId = EntityId.New<UserId>();
        var workout = await SeedLoggedWorkout(ownerId);

        var input = new UpdateLoggedWorkoutInput(
            workout.Id.Value,
            Guid.NewGuid(),
            DateTime.UtcNow,
            [
                new LoggedExerciseInput(
                    Guid.NewGuid(), 0,
                    [new LoggedSetInput("Strength", 0, null, null, null, 5, 50m, "Kg", 1)])
            ]);

        await Assert.ThrowsAsync<LoggedWorkoutAccessDeniedException>(() => _useCase.Execute(input));
    }
}
