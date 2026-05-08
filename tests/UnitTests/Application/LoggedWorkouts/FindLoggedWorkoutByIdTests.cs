using Bloom.Application.Contracts;
using Bloom.Application.LoggedWorkouts;
using Bloom.Shared.Exceptions;

namespace UnitTests.Application.LoggedWorkouts;

public sealed class FindLoggedWorkoutByIdTests
{
    [Fact]
    public async Task Execute_WithExistingId_ShouldReturnLog()
    {
        Guid id = Guid.NewGuid();
        var data = new List<LoggedWorkoutData>
        {
            new() { Id = id, UserId = Guid.NewGuid(), LoggedAt = DateTime.UtcNow }
        };
        var useCase = new FindLoggedWorkoutById(new MockFindLoggedWorkoutsQuery(data));

        var output = await useCase.Execute(new FindLoggedWorkoutByIdInput(id));

        Assert.Equal(id, output.Log.Id);
    }

    [Fact]
    public async Task Execute_WithMissingId_ShouldThrow()
    {
        var useCase = new FindLoggedWorkoutById(new MockFindLoggedWorkoutsQuery([]));

        await Assert.ThrowsAsync<LoggedWorkoutNotFoundException>(
            () => useCase.Execute(new FindLoggedWorkoutByIdInput(Guid.NewGuid())));
    }
}
