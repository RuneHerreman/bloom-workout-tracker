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
    /// <summary>
    /// Icons the user picked for gear items, by item name (see <see cref="GearKinds"/>).
    /// Items without an entry get an icon guessed from their name.
    /// </summary>
    public Dictionary<string, string> GearIcons { get; private set; } = [];
    /// <summary>Whether other users can see this user on the monthly leaderboard. Opt-out: visible by default.</summary>
    public bool ShowOnLeaderboard { get; private set; } = true;
    /// <summary>Optional; only asked for by the macro calculator.</summary>
    public Sex? Sex { get; private set; }

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
        // A removed item takes its icon with it.
        if (GearIcons.Keys.Any(k => !gear.Contains(k)))
            GearIcons = GearIcons.Where(kv => gear.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
        RaiseDomainEvent(new UserUpdated(Id));
    }

    /// <summary>
    /// Picks the icon for one gear item, or clears it (null) to go back to the
    /// guess from its name. Returns false when the user has no such item.
    /// </summary>
    public bool SetGearIcon(string name, string? kind)
    {
        if (kind is not null) Asserts.EnsureTrue(GearKinds.All.Contains(kind));
        if (!Gear.Contains(name)) return false;
        var next = new Dictionary<string, string>(GearIcons);
        if (kind is null) next.Remove(name); else next[name] = kind;
        GearIcons = next;
        RaiseDomainEvent(new UserUpdated(Id));
        return true;
    }

    /// <summary>Renames one gear item in place, keeping its position. Returns false when the user has no such item.</summary>
    public bool RenameGear(string from, string to)
    {
        Asserts.EnsureTrue(!string.IsNullOrWhiteSpace(to));
        if (!Gear.Contains(from)) return false;
        Gear = Gear.Select(g => g == from ? to : g).Distinct().ToList();
        if (GearIcons.TryGetValue(from, out var kind))
        {
            var icons = new Dictionary<string, string>(GearIcons);
            icons.Remove(from);
            icons[to] = kind;
            GearIcons = icons;
        }
        RaiseDomainEvent(new UserUpdated(Id));
        return true;
    }

    public void SetLeaderboardVisibility(bool showOnLeaderboard)
    {
        if (ShowOnLeaderboard == showOnLeaderboard)
            return;

        ShowOnLeaderboard = showOnLeaderboard;
        RaiseDomainEvent(new UserUpdated(Id));
    }

    public void SetSex(Sex? sex)
    {
        if (Sex == sex) return;
        Sex = sex;
        RaiseDomainEvent(new UserUpdated(Id));
    }

    /// <summary>Current weight follows the latest entry in the body log.</summary>
    public void SyncWeight(decimal weightKg)
    {
        Asserts.EnsureGreaterThan(weightKg, 0m);
        Weight = weightKg;
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