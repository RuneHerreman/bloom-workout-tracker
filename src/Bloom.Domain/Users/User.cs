using Bloom.Domain.Shared;
using Bloom.Domain.Users.DomainEvents;
using Bloom.Domain.Users.ValueObjects;

namespace Bloom.Domain.Users;

public readonly record struct UserId(Guid Value) : IEntityId;

public class User: AggregateRoot<UserId>
{
    public Email Email { get; private set; }
    public Username Username { get; private set; }
    public HashedPassword HashedPassword { get; private set; }
    public decimal Weight { get; private set; }
    public int Height { get; private set; }
    public int ActiveDays { get; private set; }

    private User() { }

    private User(
        UserId id,
        Email email,
        Username username,
        HashedPassword hashedPassword,
        decimal weight,
        int height,
        int activeDays) : base(id)
    {
        Email = email;
        Username = username;
        HashedPassword = hashedPassword;
        Weight = weight;
        Height = height;
        ActiveDays = activeDays;
    }

    public static User Create(
        string email,
        string username,
        string hashedPassword,
        decimal weight,
        int height,
        int activeDays,
        UserId? id = null
    )
    {
        var user = new User(
            id ?? EntityId.New<UserId>(),
            Email.Create(email),
            Username.Create(username),
            HashedPassword.Create(hashedPassword),
            weight,
            height,
            activeDays);

        user.ValidateState();
        user.RaiseDomainEvent(new UserRegistered(user.Id));

        return user;
    }

    public void ChangePassword(string newHashedPassword)
    {
        HashedPassword = HashedPassword.Create(newHashedPassword);
    }

    public void UpdateInfo(
        string email,
        string username,
        decimal weight,
        int height,
        int activeDays)
    {
        Email = Email.Create(email);
        Username = Username.Create(username);
        Weight = weight;
        Height = height;
        ActiveDays = activeDays;

        ValidateState();
        RaiseDomainEvent(new UserUpdated(Id));
    }

    public override void ValidateState()
    {
        Asserts.EnsureNotEmpty(Email);
        Asserts.EnsureNotEmpty(Username);
        Asserts.EnsureNotEmpty(HashedPassword);
        Asserts.EnsureGreaterThan(Weight, 0m);
        Asserts.EnsureGreaterThan(Height, 0);
        Asserts.EnsureNotNegative(ActiveDays);
        Asserts.EnsureLessThan(ActiveDays, 8);
    }
}