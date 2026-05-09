using Bloom.Application.Contracts;
using Bloom.Application.LoggedWorkouts;
using Bloom.Shared.Exceptions;
using UnitTests.Application.Mocks;

namespace UnitTests.Application.LoggedWorkouts;

public sealed class FindLoggedWorkoutByIdTests
{
    [Fact]
    public async Task Execute_AsOwner_ShouldReturnLog()
    {
        Guid userId = Guid.NewGuid();
        Guid id = Guid.NewGuid();
        var data = new List<LoggedWorkoutData>
        {
            new() { Id = id, UserId = userId, LoggedAt = DateTime.UtcNow }
        };
        var useCase = new FindLoggedWorkoutById(StubCurrentUser.With(userId), new MockFindLoggedWorkoutsQuery(data));

        var output = await useCase.Execute(new FindLoggedWorkoutByIdInput(id));

        Assert.Equal(id, output.Log.Id);
    }

    [Fact]
    public async Task Execute_AsOtherUser_ShouldThrowAccessDenied()
    {
        Guid id = Guid.NewGuid();
        var data = new List<LoggedWorkoutData>
        {
            new() { Id = id, UserId = Guid.NewGuid(), LoggedAt = DateTime.UtcNow }
        };
        var useCase = new FindLoggedWorkoutById(StubCurrentUser.Random(), new MockFindLoggedWorkoutsQuery(data));

        await Assert.ThrowsAsync<LoggedWorkoutAccessDeniedException>(
            () => useCase.Execute(new FindLoggedWorkoutByIdInput(id)));
    }

    [Fact]
    public async Task Execute_WithMissingId_ShouldThrow()
    {
        var useCase = new FindLoggedWorkoutById(StubCurrentUser.Random(), new MockFindLoggedWorkoutsQuery([]));

        await Assert.ThrowsAsync<LoggedWorkoutNotFoundException>(
            () => useCase.Execute(new FindLoggedWorkoutByIdInput(Guid.NewGuid())));
    }
}
