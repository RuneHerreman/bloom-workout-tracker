using Bloom.Application.Common.Behaviours;
using Bloom.Domain.Entity;

namespace UnitTests.Mock;

public class MockJwtGenerator : IJwtTokenGenerator
{
    public string GenerateToken(User user) => "fake-jwt-token-123";
}