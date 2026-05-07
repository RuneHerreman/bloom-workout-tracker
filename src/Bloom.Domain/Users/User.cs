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

    private User() { }

    private User(
        UserId id,
        Email email,
        Username username,
        HashedPassword hashedPassword) : base(id)
    {
        Email = email;
        Username = username;
        HashedPassword = hashedPassword;
    }

    public static User Create(
        string email,
        string username,
        string hashedPassword,
        UserId? id = null
    )
    {
        var user = new User(
            id ?? EntityId.New<UserId>(),
            Email.Create(email),
            Username.Create(username),
            HashedPassword.Create(hashedPassword));

        user.ValidateState();
        user.RaiseDomainEvent(new UserRegistered(user.Id));

        return user;
    }

    public void ChangePassword(string newHashedPassword)
    {
        HashedPassword = HashedPassword.Create(newHashedPassword);
    }

    public override void ValidateState()
    {
        Asserts.EnsureNotEmpty(Email);
        Asserts.EnsureNotEmpty(Username);
        Asserts.EnsureNotEmpty(HashedPassword);
    }
}