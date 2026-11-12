using Bloom.Application.Users;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using UnitTests.Application.Mocks;
using UnitTests.Application.Shared;

namespace UnitTests.Application.Users;

public sealed class UpdateGearIconTests : ApplicationTestBase
{
    private async Task<User> SeedUser(params string[] gear)
    {
        var user = User.Create("user@example.com", "alice", "hash", "Alice", "Smith", 72.5m, 180, 4, new DateOnly(1990, 1, 1));
        user.UpdateGear([.. gear]);
        await UserRepository.Save(user);
        return user;
    }

    private UpdateGearIcon CreateUseCase(UserId userId) => new(UnitOfWork, StubCurrentUser.With(userId), CreateLogger<UpdateGearIcon>());

    [Fact]
    public async Task Execute_WithKnownKind_ShouldSetIcon()
    {
        var user = await SeedUser("Old faithful");

        await CreateUseCase(user.Id).Execute(new UpdateGearIconInput("Old faithful", "shoe"));

        Assert.Equal("shoe", (await UserRepository.ById(user.Id)).Value.GearIcons["Old faithful"]);
    }

    [Fact]
    public async Task Execute_WithNullKind_ShouldClearIcon()
    {
        var user = await SeedUser("Old faithful");
        user.SetGearIcon("Old faithful", "bike");

        await CreateUseCase(user.Id).Execute(new UpdateGearIconInput("Old faithful", null));

        Assert.Empty((await UserRepository.ById(user.Id)).Value.GearIcons);
    }

    [Fact]
    public async Task Execute_WithUnknownKind_ShouldThrow()
    {
        var user = await SeedUser("Old faithful");

        await Assert.ThrowsAsync<BloomGeneralException>(
            () => CreateUseCase(user.Id).Execute(new UpdateGearIconInput("Old faithful", "rocket")));
    }

    [Fact]
    public async Task Execute_WithUnknownItem_ShouldThrow()
    {
        var user = await SeedUser("Old faithful");

        await Assert.ThrowsAsync<BloomGeneralException>(
            () => CreateUseCase(user.Id).Execute(new UpdateGearIconInput("Something else", "shoe")));
    }

    [Fact]
    public void RemovingGear_ShouldDropItsIcon()
    {
        var user = User.Create("user@example.com", "alice", "hash", "Alice", "Smith", 72.5m, 180, 4, new DateOnly(1990, 1, 1));
        user.UpdateGear(["Shoes", "Bike"]);
        user.SetGearIcon("Shoes", "shoe");
        user.SetGearIcon("Bike", "bike");

        user.UpdateGear(["Bike"]);

        Assert.Equal(new Dictionary<string, string> { ["Bike"] = "bike" }, user.GearIcons);
    }

    [Fact]
    public void RenamingGear_ShouldKeepItsIcon()
    {
        var user = User.Create("user@example.com", "alice", "hash", "Alice", "Smith", 72.5m, 180, 4, new DateOnly(1990, 1, 1));
        user.UpdateGear(["Shoes"]);
        user.SetGearIcon("Shoes", "shoe");

        user.RenameGear("Shoes", "Pegasus 41");

        Assert.Equal(new Dictionary<string, string> { ["Pegasus 41"] = "shoe" }, user.GearIcons);
    }
}
