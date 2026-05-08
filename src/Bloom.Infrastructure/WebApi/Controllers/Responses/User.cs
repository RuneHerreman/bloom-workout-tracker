using Bloom.Application.Contracts;

namespace Bloom.Infrastructure.WebApi.Controllers.Responses;

public record User(
    Guid Id,
    string Email,
    string Username,
    decimal Weight,
    int Height,
    int ActiveDays
);

public static class UserExtensions
{
    public static User ToResponse(this UserData data)
    {
        return new User(
            data.Id,
            data.Email,
            data.Username,
            data.Weight,
            data.Height,
            data.ActiveDays
        );
    }
}
