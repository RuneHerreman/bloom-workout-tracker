using Bloom.Application.Commands;
using Bloom.Application.Common.Security;
using UnitTests.Mock;
using UnitTests.Mocks;
using Xunit;

namespace UnitTests.Bloom.Application.Tests;

public class RegisterUserTests
{
    [Fact]
    public async Task ValidCommand_CreatesUser_ReturnsToken()
    {
        var repo = new MockUserRepository();
        var jwt = new MockJwtGenerator();
        var handler = new RegisterUserHandler(repo, jwt, new MockLogger<RegisterUserHandler>());

        var result = await handler.Handle(new RegisterUserCommand(
                "test@bloom.com",
                "Test",
                Hashing.Hash("pass123"),
                180,
                75,
                5),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("fake-jwt-token-123", result.Value);
        Assert.Single(repo.CreatedUsers);
    }
}