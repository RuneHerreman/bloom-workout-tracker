namespace Bloom.Application.Common.Behaviours;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserEmail { get; }
}