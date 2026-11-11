using Bloom.Domain.Body;
using Bloom.Domain.Users;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Repositories;

public class BodyMeasurementRepository(DomainDbContext context)
    : EfCoreGenericRepository<BodyMeasurement, BodyMeasurementId>(context), IBodyMeasurementRepository
{
    public async Task<IReadOnlyList<BodyMeasurement>> ForUser(UserId userId, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default)
    {
        var q = _context.BodyMeasurements.Where(m => m.UserId == userId);
        if (from is { } f) q = q.Where(m => m.Date >= f);
        if (to is { } t) q = q.Where(m => m.Date <= t);
        return await q.OrderBy(m => m.Date).ToListAsync(ct);
    }

    public Task<BodyMeasurement?> OnDate(UserId userId, DateOnly date, CancellationToken ct = default) =>
        _context.BodyMeasurements.FirstOrDefaultAsync(m => m.UserId == userId && m.Date == date, ct);

    public Task<BodyMeasurement?> Latest(UserId userId, CancellationToken ct = default) =>
        _context.BodyMeasurements.Where(m => m.UserId == userId).OrderByDescending(m => m.Date).FirstOrDefaultAsync(ct);
}
