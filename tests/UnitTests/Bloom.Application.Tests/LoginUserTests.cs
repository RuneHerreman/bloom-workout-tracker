using Bloom.Application.Commands;
using Bloom.Application.Common.Security;
using UnitTests.Mock;
using UnitTests.Mocks;
using Xunit;

namespace UnitTests.Bloom.Application.Tests;

public class LoginUserTests
{
    [Fact]
    public async Task LoginUser_ValidCredentials_ReturnsToken()
    {
        var repo = new MockUserRepository();
        var jwt = new MockJwtGenerator();
        var loginHandler = new LoginHandler(repo, jwt, new MockLogger<LoginHandler>());
        var registerHandler = new RegisterUserHandler(repo, jwt, new MockLogger<RegisterUserHandler>());
    
        await registerHandler.Handle(new RegisterUserCommand(
                "test@bloom.com", "Test", 
                "pass123",
                180, 75, 5),
            CancellationToken.None);
    
        var result = await loginHandler.Handle(new LoginCommand(
                "test@bloom.com", "pass123"), 
            CancellationToken.None);
    
        Assert.True(result.IsSuccess);
    }
}