using Bloom.Domain.Users;

namespace Bloom.Application.Contracts.Ports;

public interface IAuthorizationService
{
    string GenerateToken(UserId userId, string email, string username);
    bool ValidateToken(string token);
    UserId? GetUserIdFromToken(string token);
}