using Bloom.Application.Contracts;

namespace Bloom.Infrastructure.WebApi.Controllers.Responses;

public record User(
    Guid Id,
    string Email,
    string Username,
    string FirstName,
    string LastName,
    decimal Weight,
    int Height,
    int ActiveDays,
    string? TechnicalPoints
);

public static class UserExtensions
{
    public static User ToResponse(this UserData data)
    {
        return new User(
            data.Id,
            data.Email,
            data.Username,
            data.FirstName,
            data.LastName,
            data.Weight,
            data.Height,
            data.ActiveDays,
            data.TechnicalPoints
        );
    }
}
