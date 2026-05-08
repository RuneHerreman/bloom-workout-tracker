using Bloom.Application.LoggedWorkouts;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using UnitTests.Application.Shared;

namespace UnitTests.Application.LoggedWorkouts;

public sealed class CreateLoggedWorkoutTests : ApplicationTestBase
{
    private readonly CreateLoggedWorkout _useCase;

    public CreateLoggedWorkoutTests()
    {
        _useCase = new CreateLoggedWorkout(UnitOfWork, CreateLogger<CreateLoggedWorkout>());
    }

    private static CreateLoggedWorkoutInput BuildInput(Guid userId)
    {
        return new CreateLoggedWorkoutInput(
            userId,
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

        var output = await _useCase.Execute(BuildInput(user.Id.Value));

        Assert.NotEqual(Guid.Empty, output.LoggedWorkoutId);
        var saved = await LoggedWorkoutRepository.ById(EntityId.New<Bloom.Domain.LoggedWorkouts.LoggedWorkoutId>(output.LoggedWorkoutId));
        Assert.True(saved.HasValue);
    }

    [Fact]
    public async Task Execute_WithMissingUser_ShouldThrow()
    {
        await Assert.ThrowsAsync<UserNotFoundException>(
            () => _useCase.Execute(BuildInput(Guid.NewGuid())));
    }
}
