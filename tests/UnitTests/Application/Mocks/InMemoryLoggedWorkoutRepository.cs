using Aornis;
using Bloom.Domain.LoggedWorkouts;
using Bloom.Domain.Users;

namespace UnitTests.Application.Mocks;

public sealed class InMemoryLoggedWorkoutRepository : ILoggedWorkoutRepository
{
    private readonly Dictionary<LoggedWorkoutId, LoggedWorkout> _store = new();

    public Task<bool> Exists(LoggedWorkoutId id)
        => Task.FromResult(_store.ContainsKey(id));

    public Task<Optional<LoggedWorkout>> ById(LoggedWorkoutId id)
    {
        _store.TryGetValue(id, out LoggedWorkout? entity);
        return Task.FromResult(Optional.Of(entity));
    }

    public Task Save(LoggedWorkout aggregateRoot)
    {
        _store[aggregateRoot.Id] = aggregateRoot;
        return Task.CompletedTask;
    }

    public Task Remove(LoggedWorkout aggregateRoot)
    {
        _store.Remove(aggregateRoot.Id);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsWithExternalId(UserId userId, string externalId, CancellationToken ct = default)
        => Task.FromResult(_store.Values.Any(lw => lw.UserId == userId && lw.ExternalId == externalId));
}
