using Bloom.Application.Users;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using UnitTests.Application.Mocks;
using UnitTests.Application.Shared;

namespace UnitTests.Application.Users;

public sealed class UpdateUserGearTests : ApplicationTestBase
{
    private async Task<User> SeedUser()
    {
        var user = User.Create("user@example.com", "alice", "hash", "Alice", "Smith", 72.5m, 180, 4, new DateOnly(1990, 1, 1));
        await UserRepository.Save(user);
        return user;
    }

    [Fact]
    public async Task Execute_WithValidGear_ShouldUpdateGear()
    {
        var user = await SeedUser();
        var useCase = new UpdateUserGear(UnitOfWork, StubCurrentUser.With(user.Id), CreateLogger<UpdateUserGear>());

        await useCase.Execute(new UpdateUserGearInput(["Nike Vaporfly", "Canyon Ultimate", "Garmin Forerunner"]));

        var saved = await UserRepository.ById(user.Id);
        Assert.Equal(["Nike Vaporfly", "Canyon Ultimate", "Garmin Forerunner"], saved.Value.Gear);
    }

    [Fact]
    public async Task Execute_WithEmptyList_ShouldClearGear()
    {
        var user = await SeedUser();
        user.UpdateGear(["Old Shoes"]);
        var useCase = new UpdateUserGear(UnitOfWork, StubCurrentUser.With(user.Id), CreateLogger<UpdateUserGear>());

        await useCase.Execute(new UpdateUserGearInput([]));

        var saved = await UserRepository.ById(user.Id);
        Assert.Empty(saved.Value.Gear);
    }

    [Fact]
    public async Task Execute_WithBlankGearItem_ShouldThrow()
    {
        var user = await SeedUser();
        var useCase = new UpdateUserGear(UnitOfWork, StubCurrentUser.With(user.Id), CreateLogger<UpdateUserGear>());

        await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.Execute(new UpdateUserGearInput(["Valid", "   "])));
    }

    [Fact]
    public async Task Execute_WithMissingUser_ShouldThrow()
    {
        var useCase = new UpdateUserGear(UnitOfWork, StubCurrentUser.Random(), CreateLogger<UpdateUserGear>());

        await Assert.ThrowsAsync<UserNotFoundException>(
            () => useCase.Execute(new UpdateUserGearInput(["Shoes"])));
    }
}
