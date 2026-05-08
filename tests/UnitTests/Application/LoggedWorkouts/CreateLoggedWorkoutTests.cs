using Bloom.Application.LoggedWorkouts;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using UnitTests.Application.Mocks;
using UnitTests.Application.Shared;

namespace UnitTests.Application.LoggedWorkouts;

public sealed class CreateLoggedWorkoutTests : ApplicationTestBase
{
    private static CreateLoggedWorkoutInput BuildInput()
    {
        return new CreateLoggedWorkoutInput(
            [
                new LoggedExerciseInput(
                    Guid.NewGuid(),
                    0,
                    [
                        new LoggedSetInput("Strength", 0, null, null, null, 8, 60m, "Kg", 2)
                    ])
            ]);
    }

    [Fact]
    public async Task Execute_WithExistingUser_ShouldCreateLoggedWorkout()
    {
        User user = User.Create("user@example.com", "alice", "hash");
        await UserRepository.Save(user);
        var useCase = new CreateLoggedWorkout(UnitOfWork, StubCurrentUser.With(user.Id), CreateLogger<CreateLoggedWorkout>());

        var output = await useCase.Execute(BuildInput());

        Assert.NotEqual(Guid.Empty, output.LoggedWorkoutId);
        var saved = await LoggedWorkoutRepository.ById(EntityId.New<Bloom.Domain.LoggedWorkouts.LoggedWorkoutId>(output.LoggedWorkoutId));
        Assert.True(saved.HasValue);
    }

    [Fact]
    public async Task Execute_WithMissingUser_ShouldThrow()
    {
        var useCase = new CreateLoggedWorkout(UnitOfWork, StubCurrentUser.Random(), CreateLogger<CreateLoggedWorkout>());

        await Assert.ThrowsAsync<UserNotFoundException>(
            () => useCase.Execute(BuildInput()));
    }
}
