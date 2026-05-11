using Bloom.Application.Users;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using UnitTests.Application.Mocks;
using UnitTests.Application.Shared;

namespace UnitTests.Application.Users;

public sealed class ChangeUserPasswordTests : ApplicationTestBase
{
    private readonly StubPasswordHasher _passwordHasher = new();

    private async Task<User> SeedUser(string password = "old-secret")
    {
        var user = User.Create(
            "user@example.com",
            "alice",
            _passwordHasher.HashPassword(password),
            "Alice",
            "Smith",
            72.5m, 180, 4);
        await UserRepository.Save(user);
        return user;
    }

    private ChangeUserPassword BuildUseCase(UserId currentUserId) =>
        new(UnitOfWork, StubCurrentUser.With(currentUserId), _passwordHasher, CreateLogger<ChangeUserPassword>());

    [Fact]
    public async Task Execute_WithCorrectOldPassword_ShouldReplaceHash()
    {
        var user = await SeedUser("old-secret");
        var useCase = BuildUseCase(user.Id);

        await useCase.Execute(new ChangeUserPasswordInput("old-secret", "new-secret"));

        var stored = await UserRepository.ById(user.Id);
        Assert.True(stored.HasValue);
        Assert.True(_passwordHasher.VerifyHashedPassword(stored.Value.HashedPassword.Value, "new-secret"));
    }

    [Fact]
    public async Task Execute_WithWrongOldPassword_ShouldThrowInvalidCredentials()
    {
        var user = await SeedUser("old-secret");
        var useCase = BuildUseCase(user.Id);

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => useCase.Execute(new ChangeUserPasswordInput("not-the-password", "new-secret")));

        var stored = await UserRepository.ById(user.Id);
        Assert.True(_passwordHasher.VerifyHashedPassword(stored.Value.HashedPassword.Value, "old-secret"));
    }

    [Fact]
    public async Task Execute_WithMissingUser_ShouldThrowNotFound()
    {
        var missingId = EntityId.New<UserId>();
        var useCase = BuildUseCase(missingId);

        await Assert.ThrowsAsync<UserNotFoundException>(
            () => useCase.Execute(new ChangeUserPasswordInput("old-secret", "new-secret")));
    }
}
