using System.Collections.Concurrent;

namespace Bloom.Infrastructure.Caching;

/// <summary>
/// Per-user version token included in exercise catalog cache keys.
/// Bumping a user's version makes their previously cached search results unreachable,
/// so custom exercise changes are visible immediately.
/// </summary>
public sealed class ExerciseCatalogCacheVersion
{
    private readonly ConcurrentDictionary<Guid, long> _versions = new();

    public long Get(Guid userId) => _versions.GetValueOrDefault(userId, 0);

    public void Bump(Guid userId) => _versions.AddOrUpdate(userId, 1, (_, v) => v + 1);
}
