using Bloom.Domain.Shared;
using Bloom.Domain.Users.DomainEvents;
using Bloom.Domain.Users.ValueObjects;

namespace Bloom.Domain.Users;

public readonly record struct UserId(Guid Value) : IEntityId;

public class User : AggregateRoot<UserId>
{
    public Email Email { get; private set; }
    public Username Username { get; private set; }
    public HashedPassword HashedPassword { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public decimal Weight { get; private set; }
    public int Height { get; private set; }
    public int ActiveDays { get; private set; }
    public DateOnly BirthDate { get; private set; }
    public string? TechnicalPoints { get; private set; }
    public List<string> Gear { get; private set; } = [];

    private User() { }

    private User(
        UserId id,
        Email email,
        Username username,
        HashedPassword hashedPassword,
        string firstName,
        string lastName,
        decimal weight,
        int height,
        int activeDays,
        DateOnly birthDate) : base(id)
    {
        Email = email;
        Username = username;
        HashedPassword = hashedPassword;
        FirstName = firstName;
        LastName = lastName;
        Weight = weight;
        Height = height;
        ActiveDays = activeDays;
        BirthDate = birthDate;
    }

    public static User Create(
        string email,
        string username,
        string hashedPassword,
        string firstName,
        string lastName,
        decimal weight,
        int height,
        int activeDays,
        DateOnly birthDate,
        UserId? id = null)
    {
        var user = new User(
            id ?? EntityId.New<UserId>(),
            Email.Create(email),
            Username.Create(username),
            HashedPassword.Create(hashedPassword),
            firstName,
            lastName,
            weight,
            height,
            activeDays,
            birthDate);

        user.ValidateState();
        user.RaiseDomainEvent(new UserRegistered(user.Id));

        return user;
    }

    public void UpdateTechnicalPoints(string? technicalPoints)
    {
        TechnicalPoints = technicalPoints;
    }

    public void UpdateGear(List<string> gear)
    {
        Asserts.EnsureTrue(gear.All(g => !string.IsNullOrWhiteSpace(g)));
        Gear = gear;
        RaiseDomainEvent(new UserUpdated(Id));
    }

    public void ChangePassword(string newHashedPassword)
    {
        HashedPassword = HashedPassword.Create(newHashedPassword);
    }

    public void UpdateInfo(
        string email,
        string username,
        string firstName,
        string lastName,
        decimal weight,
        int height,
        int activeDays,
        DateOnly birthDate)
    {
        Email = Email.Create(email);
        Username = Username.Create(username);
        FirstName = firstName;
        LastName = lastName;
        Weight = weight;
        Height = height;
        ActiveDays = activeDays;
        BirthDate = birthDate;

        ValidateState();
        RaiseDomainEvent(new UserUpdated(Id));
    }

    public override void ValidateState()
    {
        Asserts.EnsureNotEmpty(Email);
        Asserts.EnsureNotEmpty(Username);
        Asserts.EnsureNotEmpty(HashedPassword);
        Asserts.EnsureNotEmpty(FirstName);
        Asserts.EnsureNotEmpty(LastName);
        Asserts.EnsureGreaterThan(Weight, 0m);
        Asserts.EnsureGreaterThan(Height, 0);
        Asserts.EnsureNotNegative(ActiveDays);
        Asserts.EnsureLessThan(ActiveDays, 8);
        Asserts.EnsureTrue(BirthDate < DateOnly.FromDateTime(DateTime.UtcNow));
    }
}