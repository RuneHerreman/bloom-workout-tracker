using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Configuration.Convertors;

/// <summary>
/// Encrypts sensitive token columns at rest using DataProtection.
/// Rows written before encryption was introduced are returned as-is so existing
/// connections keep working; they get encrypted on the next token refresh.
/// </summary>
public sealed class ProtectedTokenConverter(IDataProtector protector)
    : ValueConverter<string, string>(
        value => protector.Protect(value),
        stored => Unprotect(protector, stored))
{
    private static string Unprotect(IDataProtector protector, string stored)
    {
        try
        {
            return protector.Unprotect(stored);
        }
        catch (CryptographicException)
        {
            return stored;
        }
    }
}
