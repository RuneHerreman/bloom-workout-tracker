using Bloom.Domain.Shared;
using Bloom.Domain.Users;

namespace Bloom.Domain.Body;

public interface IBodyMeasurementRepository : IRepository<BodyMeasurement, BodyMeasurementId>
{
    /// <summary>The user's entries, oldest first, optionally limited to a date range (inclusive).</summary>
    Task<IReadOnlyList<BodyMeasurement>> ForUser(UserId userId, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default);

    Task<BodyMeasurement?> OnDate(UserId userId, DateOnly date, CancellationToken ct = default);

    /// <summary>The entry with the latest date, if any.</summary>
    Task<BodyMeasurement?> Latest(UserId userId, CancellationToken ct = default);
}
