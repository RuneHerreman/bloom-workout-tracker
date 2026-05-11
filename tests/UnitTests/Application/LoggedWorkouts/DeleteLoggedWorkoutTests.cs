using Bloom.Application.LoggedWorkouts;
using Bloom.Domain.Exercises;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.LoggedWorkouts.ValueObjects;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using UnitTests.Application.Mocks;
using UnitTests.Application.Shared;

namespace UnitTests.Application.LoggedWorkouts;

public sealed class DeleteLoggedWorkoutTests : ApplicationTestBase
{
    private async Task<LoggedWorkout> SeedLoggedWorkout(UserId userId)
    {
        var workout = LoggedWorkout.Create(userId,
            "Test Workout",
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
        var useCase = new DeleteLoggedWorkout(UnitOfWork, StubCurrentUser.With(userId), CreateLogger<DeleteLoggedWorkout>());

        await useCase.Execute(new DeleteLoggedWorkoutInput(workout.Id.Value));

        Assert.False(await LoggedWorkoutRepository.Exists(workout.Id));
    }

    [Fact]
    public async Task Execute_WithMissingWorkout_ShouldThrow()
    {
        var useCase = new DeleteLoggedWorkout(UnitOfWork, StubCurrentUser.Random(), CreateLogger<DeleteLoggedWorkout>());

        await Assert.ThrowsAsync<LoggedWorkoutNotFoundException>(
            () => useCase.Execute(new DeleteLoggedWorkoutInput(Guid.NewGuid())));
    }

    [Fact]
    public async Task Execute_WithUnauthorizedUser_ShouldThrow()
    {
        UserId ownerId = EntityId.New<UserId>();
        var workout = await SeedLoggedWorkout(ownerId);
        var useCase = new DeleteLoggedWorkout(UnitOfWork, StubCurrentUser.Random(), CreateLogger<DeleteLoggedWorkout>());

        await Assert.ThrowsAsync<LoggedWorkoutAccessDeniedException>(
            () => useCase.Execute(new DeleteLoggedWorkoutInput(workout.Id.Value)));
    }
}
