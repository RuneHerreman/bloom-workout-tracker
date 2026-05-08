using Bloom.Domain.Users;
using Bloom.Domain.Users.ValueObjects;

namespace Bloom.Application.Contracts.Ports;

public interface ITokenIssuer
{
    string Issue(UserId userId, Email email, Username username);
}
