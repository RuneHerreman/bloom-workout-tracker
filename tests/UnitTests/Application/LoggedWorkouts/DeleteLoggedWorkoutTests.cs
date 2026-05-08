using Bloom.Application.LoggedWorkouts;
using Bloom.Domain.Exercises;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.LoggedWorkouts.ValueObjects;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using UnitTests.Application.Shared;

namespace UnitTests.Application.LoggedWorkouts;

public sealed class DeleteLoggedWorkoutTests : ApplicationTestBase
{
    private readonly DeleteLoggedWorkout _useCase;

    public DeleteLoggedWorkoutTests()
    {
        _useCase = new DeleteLoggedWorkout(UnitOfWork, CreateLogger<DeleteLoggedWorkout>());
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
    public async Task Execute_WithOwnedWorkout_ShouldRemove()
    {
        UserId userId = EntityId.New<UserId>();
        var workout = await SeedLoggedWorkout(userId);

        await _useCase.Execute(new DeleteLoggedWorkoutInput(workout.Id.Value, userId.Value));

        Assert.False(await LoggedWorkoutRepository.Exists(workout.Id));
    }

    [Fact]
    public async Task Execute_WithMissingWorkout_ShouldThrow()
    {
        await Assert.ThrowsAsync<LoggedWorkoutNotFoundException>(
            () => _useCase.Execute(new DeleteLoggedWorkoutInput(Guid.NewGuid(), Guid.NewGuid())));
    }

    [Fact]
    public async Task Execute_WithUnauthorizedUser_ShouldThrow()
    {
        UserId ownerId = EntityId.New<UserId>();
        var workout = await SeedLoggedWorkout(ownerId);

        await Assert.ThrowsAsync<LoggedWorkoutAccessDeniedException>(
            () => _useCase.Execute(new DeleteLoggedWorkoutInput(workout.Id.Value, Guid.NewGuid())));
    }
}
