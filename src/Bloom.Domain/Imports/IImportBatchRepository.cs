using Bloom.Domain.Shared;
using Bloom.Domain.Users;

namespace Bloom.Domain.Imports;

public interface IImportBatchRepository : IRepository<ImportBatch, ImportBatchId>
{
    /// <summary>The user's imports, newest first.</summary>
    Task<IReadOnlyList<ImportBatch>> ForUser(UserId userId, CancellationToken ct = default);
}
