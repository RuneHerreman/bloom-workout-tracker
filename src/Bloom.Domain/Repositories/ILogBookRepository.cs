using Bloom.Domain.Entity.Logs;

namespace Bloom.Domain.Repositories;

public interface ILogBookRepository
{
    Task<List<LoggedWorkout>> GetAllUserWorkoutsAsync(Guid userId, CancellationToken ct);
}