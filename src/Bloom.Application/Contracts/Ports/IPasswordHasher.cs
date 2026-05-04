namespace Bloom.Application.Contracts.Ports;

public interface IPasswordHasher
{
    public string HashPassword(string password);
    public bool VerifyHashedPassword(string hashedPassword, string providedPassword);
}