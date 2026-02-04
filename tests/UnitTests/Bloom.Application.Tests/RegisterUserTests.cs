using Bloom.Application.Commands;
using UnitTests.Mock;
using Xunit;
using Assert = NUnit.Framework.Assert;

namespace UnitTests.Bloom.Application.Tests;

public class RegisterUserTests
{
    [Fact]
    public async Task ValidCommand_CreatesUser_ReturnsToken()
    {
        var repo = new MockUserRepository();
        var jwt = new MockJwtGenerator();
        var handler = new RegisterUserHandler(repo, jwt);

        var result = await handler.Handle(new RegisterUserCommand(
                "test@bloom.com", 
                "Test", 
                "pass123", 
                180, 
                75,
                5), 
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.That(result.Value, Is.EqualTo("fake-jwt-token-123"));
        Assert.Equals(repo.CreatedUsers.Count, 1);
    }
}