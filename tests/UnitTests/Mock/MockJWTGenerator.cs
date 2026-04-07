using Bloom.Domain.Users;

namespace UnitTests.Mock;

public class MockJwtGenerator
{
    public string GenerateToken(User user) => "fake-jwt-token-123";
}