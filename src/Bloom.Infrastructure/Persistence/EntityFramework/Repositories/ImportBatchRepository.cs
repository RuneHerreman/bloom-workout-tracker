using Bloom.Domain.Imports;
using Bloom.Domain.Users;
using Bloom.Infrastructure.Persistence.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Bloom.Infrastructure.Persistence.EntityFramework.Repositories;

public class ImportBatchRepository(DomainDbContext context)
    : EfCoreGenericRepository<ImportBatch, ImportBatchId>(context), IImportBatchRepository
{
    public async Task<IReadOnlyList<ImportBatch>> ForUser(UserId userId, CancellationToken ct = default) =>
        await _context.ImportBatches
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.ImportedAt)
            .ToListAsync(ct);
}
