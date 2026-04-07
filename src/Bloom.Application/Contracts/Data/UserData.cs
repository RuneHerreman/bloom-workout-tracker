namespace Bloom.Application.Contracts.Data;

public record UserData(
    Guid Id,
    string Email,
    string Name,
    decimal Height,
    decimal Weight,
    int ActiveDays
);