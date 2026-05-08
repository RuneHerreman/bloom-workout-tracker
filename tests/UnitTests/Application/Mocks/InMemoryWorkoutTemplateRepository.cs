using Aornis;
using Bloom.Domain.WorkoutTemplates;

namespace UnitTests.Application.Mocks;

public sealed class InMemoryWorkoutTemplateRepository : IWorkoutTemplateRepository
{
    private readonly Dictionary<WorkoutTemplateId, WorkoutTemplate> _store = new();

    public Task<bool> Exists(WorkoutTemplateId id)
        => Task.FromResult(_store.ContainsKey(id));

    public Task<Optional<WorkoutTemplate>> ById(WorkoutTemplateId id)
    {
        _store.TryGetValue(id, out WorkoutTemplate? entity);
        return Task.FromResult(Optional.Of(entity));
    }

    public Task Save(WorkoutTemplate aggregateRoot)
    {
        _store[aggregateRoot.Id] = aggregateRoot;
        return Task.CompletedTask;
    }

    public Task Remove(WorkoutTemplate aggregateRoot)
    {
        _store.Remove(aggregateRoot.Id);
        return Task.CompletedTask;
    }
}
