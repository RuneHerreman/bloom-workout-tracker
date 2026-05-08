using Bloom.Application.Contracts.Ports;

namespace UnitTests.Application.Mocks;

public sealed class StubPasswordHasher : IPasswordHasher
{
    private const string HashPrefix = "hashed:";

    public string HashPassword(string password) => $"{HashPrefix}{password}";

    public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
        => hashedPassword == $"{HashPrefix}{providedPassword}";
}
