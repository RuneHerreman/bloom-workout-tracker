namespace Bloom.Application.Common.Security;

public class Hashing
{
    public static string Hash(string value) => BCrypt.Net.BCrypt.HashPassword(value);
    public static bool Verify(string value, string hash) => BCrypt.Net.BCrypt.Verify(value, hash);
}