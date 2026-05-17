using Bloom.Application.Users;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using UnitTests.Application.Mocks;
using UnitTests.Application.Shared;

namespace UnitTests.Application.Users;

public sealed class DeleteUserTests : ApplicationTestBase
{
    private async Task<User> SeedUser()
    {
        var user = User.Create("user@example.com", "alice", "hashed:secret", "Alice", "Smith", 72.5m, 180, 4, new DateOnly(1990, 1, 1));
        await UserRepository.Save(user);
        return user;
    }

    [Fact]
    public async Task Execute_ShouldRemoveUser()
    {
        var user = await SeedUser();
        var useCase = new DeleteUser(UnitOfWork, StubCurrentUser.With(user.Id), CreateLogger<DeleteUser>());

        await useCase.Execute(new DeleteUserInput());

        Assert.False(await UserRepository.Exists(user.Id));
    }

    [Fact]
    public async Task Execute_WithMissingUser_ShouldThrowNotFound()
    {
        var missingId = EntityId.New<UserId>();
        var useCase = new DeleteUser(UnitOfWork, StubCurrentUser.With(missingId), CreateLogger<DeleteUser>());

        await Assert.ThrowsAsync<UserNotFoundException>(
            () => useCase.Execute(new DeleteUserInput()));
    }
}
