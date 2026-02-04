namespace Bloom.Domain.Entity;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public decimal Height { get; set; }
    public decimal Weight { get; set; }
    public int ActiveDays { get; set; }
    
    public User() {}

    public User(Guid id, string email, string name, string passwordHash, decimal height, decimal weight, int activeDays)
    {
        Id = id;
        Email = email;
        Name = name;
        PasswordHash = passwordHash;
        Height = height;
        Weight = weight;
        ActiveDays = activeDays;
    }
}