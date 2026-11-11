using Bloom.Application.Users;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using UnitTests.Application.Mocks;
using UnitTests.Application.Shared;

namespace UnitTests.Application.Users;

public sealed class UpdateLeaderboardVisibilityTests : ApplicationTestBase
{
    private async Task<User> SeedUser()
    {
        var user = User.Create("user@example.com", "alice", "hash", "Alice", "Smith", 72.5m, 180, 4, new DateOnly(1990, 1, 1));
        await UserRepository.Save(user);
        return user;
    }

    private UpdateLeaderboardVisibility BuildUseCase(StubCurrentUser currentUser) =>
        new(UnitOfWork, currentUser, CreateLogger<UpdateLeaderboardVisibility>());

    [Fact]
    public async Task Execute_WithFalse_ShouldHideUser()
    {
        var user = await SeedUser();

        await BuildUseCase(StubCurrentUser.With(user.Id)).Execute(new UpdateLeaderboardVisibilityInput(false));

        var saved = await UserRepository.ById(user.Id);
        Assert.False(saved.Value.ShowOnLeaderboard);
    }

    [Fact]
    public async Task Execute_WithTrue_ShouldShowUserAgain()
    {
        var user = await SeedUser();
        user.SetLeaderboardVisibility(false);

        await BuildUseCase(StubCurrentUser.With(user.Id)).Execute(new UpdateLeaderboardVisibilityInput(true));

        var saved = await UserRepository.ById(user.Id);
        Assert.True(saved.Value.ShowOnLeaderboard);
    }

    [Fact]
    public async Task Execute_WithMissingUser_ShouldThrow()
    {
        await Assert.ThrowsAsync<UserNotFoundException>(
            () => BuildUseCase(StubCurrentUser.Random()).Execute(new UpdateLeaderboardVisibilityInput(false)));
    }
}
