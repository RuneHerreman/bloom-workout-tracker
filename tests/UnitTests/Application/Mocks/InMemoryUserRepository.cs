using Aornis;
using Bloom.Domain.Users;
using Bloom.Domain.Users.ValueObjects;

namespace UnitTests.Application.Mocks;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<UserId, User> _store = new();

    public Task<bool> Exists(UserId id)
        => Task.FromResult(_store.ContainsKey(id));

    public Task<Optional<User>> ById(UserId id)
    {
        _store.TryGetValue(id, out User? entity);
        return Task.FromResult(Optional.Of(entity));
    }

    public Task<bool> ExistsByEmail(Email email)
        => Task.FromResult(_store.Values.Any(u => u.Email == email));

    public Task<Optional<User>> ByEmail(Email email)
    {
        var user = _store.Values.FirstOrDefault(u => u.Email == email);
        return Task.FromResult(Optional.Of(user));
    }

    public Task Save(User aggregateRoot)
    {
        _store[aggregateRoot.Id] = aggregateRoot;
        return Task.CompletedTask;
    }

    public Task Remove(User aggregateRoot)
    {
        _store.Remove(aggregateRoot.Id);
        return Task.CompletedTask;
    }
}
