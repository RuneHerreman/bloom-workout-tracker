using Aornis;
using Bloom.Domain.Body;
using Bloom.Domain.Users;

namespace UnitTests.Application.Mocks;

public sealed class InMemoryBodyMeasurementRepository : IBodyMeasurementRepository
{
    private readonly Dictionary<BodyMeasurementId, BodyMeasurement> _store = new();

    public Task<bool> Exists(BodyMeasurementId id) => Task.FromResult(_store.ContainsKey(id));

    public Task<Optional<BodyMeasurement>> ById(BodyMeasurementId id)
    {
        _store.TryGetValue(id, out BodyMeasurement? entity);
        return Task.FromResult(Optional.Of(entity));
    }

    public Task Save(BodyMeasurement aggregateRoot)
    {
        _store[aggregateRoot.Id] = aggregateRoot;
        return Task.CompletedTask;
    }

    public Task Remove(BodyMeasurement aggregateRoot)
    {
        _store.Remove(aggregateRoot.Id);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<BodyMeasurement>> ForUser(UserId userId, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<BodyMeasurement>>(_store.Values
            .Where(m => m.UserId == userId && (from is null || m.Date >= from) && (to is null || m.Date <= to))
            .OrderBy(m => m.Date)
            .ToList());

    public Task<BodyMeasurement?> OnDate(UserId userId, DateOnly date, CancellationToken ct = default) =>
        Task.FromResult(_store.Values.FirstOrDefault(m => m.UserId == userId && m.Date == date));

    public Task<BodyMeasurement?> Latest(UserId userId, CancellationToken ct = default) =>
        Task.FromResult(_store.Values.Where(m => m.UserId == userId).OrderByDescending(m => m.Date).FirstOrDefault());
}
