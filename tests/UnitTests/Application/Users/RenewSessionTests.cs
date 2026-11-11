using Bloom.Application.Contracts.Ports;
using Bloom.Application.Users;
using Bloom.Domain.Shared;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using UnitTests.Application.Mocks;
using UnitTests.Application.Shared;

namespace UnitTests.Application.Users;

public sealed class RenewSessionTests : ApplicationTestBase
{
    private readonly StubPasswordHasher _passwordHasher = new();
    private readonly StubTokenIssuer _tokenIssuer = new();

    private async Task<User> SeedUser()
    {
        var user = User.Create(
            "user@example.com", "alice", _passwordHasher.HashPassword("secret"),
            "Alice", "Smith", 72.5m, 180, 4, new DateOnly(1990, 1, 1));
        await UserRepository.Save(user);
        return user;
    }

    private RenewSession BuildUseCase() => new(UnitOfWork, _tokenIssuer, CreateLogger<RenewSession>());

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Execute_WithCurrentStamp_ShouldIssueTokenKeepingRememberMe(bool persistent)
    {
        var user = await SeedUser();

        var output = await BuildUseCase().Execute(
            new RenewSessionInput(user.Id, SessionTicket.StampOf(user), persistent));

        var ticket = Assert.Single(_tokenIssuer.Issued);
        Assert.Equal(user.Id, ticket.UserId);
        Assert.Equal(persistent, ticket.Persistent);
        Assert.Equal(SessionTicket.StampOf(user), ticket.Stamp);
        Assert.False(string.IsNullOrEmpty(output.Token));
    }

    [Fact]
    public async Task Execute_AfterPasswordChange_ShouldRefuse()
    {
        var user = await SeedUser();
        var oldStamp = SessionTicket.StampOf(user);

        user.ChangePassword(_passwordHasher.HashPassword("new-secret"));
        await UserRepository.Save(user);

        await Assert.ThrowsAsync<UnauthenticatedException>(
            () => BuildUseCase().Execute(new RenewSessionInput(user.Id, oldStamp, false)));
        Assert.Empty(_tokenIssuer.Issued);
    }

    [Fact]
    public async Task Execute_ForDeletedUser_ShouldRefuse()
    {
        await Assert.ThrowsAsync<UnauthenticatedException>(
            () => BuildUseCase().Execute(new RenewSessionInput(EntityId.New<UserId>(), "stamp", false)));
    }

    [Fact]
    public void StampOf_ShouldChangeWithPassword()
    {
        var user = User.Create(
            "user@example.com", "alice", _passwordHasher.HashPassword("one"),
            "Alice", "Smith", 72.5m, 180, 4, new DateOnly(1990, 1, 1));
        var before = SessionTicket.StampOf(user);

        user.ChangePassword(_passwordHasher.HashPassword("two"));

        Assert.NotEqual(before, SessionTicket.StampOf(user));
    }
}
