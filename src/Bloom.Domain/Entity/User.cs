namespace Bloom.Domain.Entity;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string PasswordHash { get; set; }
    public decimal Height { get; set; }
    public decimal Weight { get; set; }
    public int ActiveDays { get; set; }
}