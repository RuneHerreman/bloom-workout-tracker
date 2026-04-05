using Bloom.Domain.Shared;

namespace Bloom.Domain.Users;

public readonly record struct UserId(Guid Value) : IEntityId;

public class User: AggregateRoot<UserId>
{
    public string Email { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public decimal Height { get; private set; }
    public decimal Weight { get; private set; }
    public int ActiveDays { get; private set; }

    // EF Core requires a parameterless constructor
    private User() {}

    private User(
        UserId id,
        string email,
        string name,
        string passwordHash,
        decimal height,
        decimal weight,
        int activeDays) : base(id)
    {
        Email = email;
        Name = name;
        PasswordHash = passwordHash;
        Height = height;
        Weight = weight;
        ActiveDays = activeDays;
    }

    public static User Create(
        string email,
        string name,
        string passwordHash,
        decimal height,
        decimal weight,
        int activeDays,
        UserId? userId = null)
    {
        User user = new(
            userId ?? EntityId.New<UserId>(),
            email,
            name,
            passwordHash,
            height,
            weight,
            activeDays
        );
        user.ValidateState();
        return user;
    }

    public void UpdateProfile(string name, decimal height, decimal weight)
    {
        Name = name;
        Height = height;
        Weight = weight;
        ValidateState();
    }
    
    public override void ValidateState()
    {
        if (string.IsNullOrWhiteSpace(Email))
            throw new InvalidOperationException("Email cannot be empty.");
        
        if (string.IsNullOrWhiteSpace(Name))
            throw new InvalidOperationException("Name cannot be empty.");
        
        if (string.IsNullOrWhiteSpace(PasswordHash))
            throw new InvalidOperationException("Password cannot be empty.");

        if (Height <= 0 || Height > 300)
            throw new InvalidOperationException("Height must be between 0 and 300 cm.");
        
        if (Weight <= 0 || Weight > 500)
            throw new InvalidOperationException("Weight must be between 0 and 500 kg.");
        
        if (ActiveDays < 0 || ActiveDays > 7)
            throw new InvalidOperationException("ActiveDays must be between 0 and 7.");
    }

}