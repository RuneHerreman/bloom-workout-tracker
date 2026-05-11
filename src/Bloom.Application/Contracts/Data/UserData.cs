namespace Bloom.Application.Contracts;

public sealed record UserData
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public decimal Weight { get; init; }
    public int Height { get; init; }
    public int ActiveDays { get; init; }
}
