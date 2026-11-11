using Bloom.Application.Users;
using Bloom.Domain.Users;
using Bloom.Shared.Exceptions;
using UnitTests.Application.Mocks;
using UnitTests.Application.Shared;

namespace UnitTests.Application.Users;

public sealed class UpdateUserSexTests : ApplicationTestBase
{
    private async Task<User> SeedUser()
    {
        var user = User.Create("user@example.com", "alice", "hash", "Alice", "Smith", 72.5m, 180, 4, new DateOnly(1990, 1, 1));
        await UserRepository.Save(user);
        return user;
    }

    private UpdateUserSex BuildUseCase(StubCurrentUser currentUser) => new(UnitOfWork, currentUser, CreateLogger<UpdateUserSex>());

    [Theory]
    [InlineData("Male", Sex.Male)]
    [InlineData("female", Sex.Female)]
    [InlineData(" MALE ", Sex.Male)]
    public async Task Execute_WithAName_ShouldSetIt(string input, Sex expected)
    {
        var user = await SeedUser();

        await BuildUseCase(StubCurrentUser.With(user.Id)).Execute(new UpdateUserSexInput(input));

        Assert.Equal(expected, (await UserRepository.ById(user.Id)).Value.Sex);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Execute_WithNothing_ShouldClearIt(string? input)
    {
        var user = await SeedUser();
        user.SetSex(Sex.Female);

        await BuildUseCase(StubCurrentUser.With(user.Id)).Execute(new UpdateUserSexInput(input));

        Assert.Null((await UserRepository.ById(user.Id)).Value.Sex);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("other")]
    public async Task Execute_WithAnythingElse_ShouldBeRejected(string input)
    {
        var user = await SeedUser();

        await Assert.ThrowsAsync<BloomGeneralException>(() => BuildUseCase(StubCurrentUser.With(user.Id)).Execute(new UpdateUserSexInput(input)));
    }

    [Fact]
    public async Task Execute_WithMissingUser_ShouldThrow()
    {
        await Assert.ThrowsAsync<UserNotFoundException>(() => BuildUseCase(StubCurrentUser.Random()).Execute(new UpdateUserSexInput("Male")));
    }
}
