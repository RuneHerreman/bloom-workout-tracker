namespace Bloom.Application.Contracts.Ports;

public interface ICurrentUserService
{
    string UserId { get; }
    string? UserEmail { get; }
}