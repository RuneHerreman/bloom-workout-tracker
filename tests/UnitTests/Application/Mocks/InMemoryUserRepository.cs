using Aornis;
using Bloom.Domain.Users;

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
