namespace Bloom.Application.DTO;

public record UserDTO(
    Guid Id,
    string Email,
    string Name,
    decimal Height,
    decimal Weight,
    int ActiveDays
);