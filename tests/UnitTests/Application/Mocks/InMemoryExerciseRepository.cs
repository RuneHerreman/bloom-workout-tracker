using Aornis;
using Bloom.Domain.Exercises;
using Bloom.Domain.Users;

namespace UnitTests.Application.Mocks;

public sealed class InMemoryExerciseRepository : IExerciseRepository
{
    private readonly Dictionary<ExerciseId, Exercise> _store = new();

    public Task<bool> Exists(ExerciseId id)
        => Task.FromResult(_store.ContainsKey(id));

    public Task<Optional<Exercise>> ById(ExerciseId id)
    {
        _store.TryGetValue(id, out Exercise? entity);
        return Task.FromResult(Optional.Of(entity));
    }

    public Task Save(Exercise aggregateRoot)
    {
        _store[aggregateRoot.Id] = aggregateRoot;
        return Task.CompletedTask;
    }

    public Task Remove(Exercise aggregateRoot)
    {
        _store.Remove(aggregateRoot.Id);
        return Task.CompletedTask;
    }

    public Task<Optional<Exercise>> ByName(string name, CancellationToken ct = default)
    {
        var match = _store.Values.FirstOrDefault(e => e.Name.Value == name);
        return Task.FromResult(Optional.Of(match));
    }

    public Task<Optional<Exercise>> ByNameForUser(string name, UserId ownerUserId, CancellationToken ct = default)
    {
        var match = _store.Values.FirstOrDefault(
            e => e.Name.Value == name && (e.OwnerUserId == null || e.OwnerUserId == ownerUserId));
        return Task.FromResult(Optional.Of(match));
    }
}
