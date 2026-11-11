using Aornis;
using Bloom.Domain.Imports;
using Bloom.Domain.Users;

namespace UnitTests.Application.Mocks;

public sealed class InMemoryImportBatchRepository : IImportBatchRepository
{
    private readonly Dictionary<ImportBatchId, ImportBatch> _store = new();

    public Task<bool> Exists(ImportBatchId id) => Task.FromResult(_store.ContainsKey(id));

    public Task<Optional<ImportBatch>> ById(ImportBatchId id)
    {
        _store.TryGetValue(id, out ImportBatch? entity);
        return Task.FromResult(Optional.Of(entity));
    }

    public Task Save(ImportBatch aggregateRoot)
    {
        _store[aggregateRoot.Id] = aggregateRoot;
        return Task.CompletedTask;
    }

    public Task Remove(ImportBatch aggregateRoot)
    {
        _store.Remove(aggregateRoot.Id);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ImportBatch>> ForUser(UserId userId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<ImportBatch>>(_store.Values.Where(b => b.UserId == userId).OrderByDescending(b => b.ImportedAt).ToList());
}
