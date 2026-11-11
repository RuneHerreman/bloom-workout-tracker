using System.Security.Cryptography;
using System.Text;
using Bloom.Domain.Users;
using Bloom.Domain.Users.ValueObjects;

namespace Bloom.Application.Contracts.Ports;

/// <summary>
/// What a session token carries. <see cref="Stamp"/> is derived from the
/// password hash, so changing the password invalidates every token issued
/// before it the next time that token is renewed. <see cref="Persistent"/>
/// is the login form's "Remember me".
/// </summary>
public sealed record SessionTicket(
    UserId UserId,
    Email Email,
    Username Username,
    string Stamp,
    bool Persistent
)
{
    public static SessionTicket For(User user, bool persistent) =>
        new(user.Id, user.Email, user.Username, StampOf(user), persistent);

    public static string StampOf(User user)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(user.HashedPassword.Value));
        return Convert.ToHexString(hash, 0, 16);
    }
}

public interface ITokenIssuer
{
    string Issue(SessionTicket ticket);
}
