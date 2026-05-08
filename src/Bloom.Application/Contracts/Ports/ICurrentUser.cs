using Bloom.Domain.Users;

namespace Bloom.Application.Contracts.Ports;

public interface ICurrentUser
{
    UserId UserId { get; }
}
